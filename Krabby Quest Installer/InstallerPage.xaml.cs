using KrabbyQuestTools.Common;
using Paloma;
using StinkyFile.Installation;
using StinkyFile.Primitive;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace KrabbyQuestInstaller
{
    /// <summary>
    /// Interaction logic for InstallerPage.xaml
    /// </summary>
    public partial class InstallerPage : Page
    {
        StringBuilder StandardOutput = new StringBuilder();
        StringBuilder Log = new StringBuilder();
        private enum InstallerState
        {
            /// <summary>
            /// Not in a special state
            /// </summary>
            NoSpecialState,
            /// <summary>
            /// Asking to uninstall
            /// </summary>
            AskingToUninstall,
            /// <summary>
            /// Asking to install 3D assets
            /// </summary>
            AskingToInstall3D,
            /// <summary>
            /// Waiting for blender setup to complete
            /// </summary>
            WaitingForCompletion3D,
            /// <summary>
            /// Asking to go to launcher
            /// </summary>
            GoToLauncher,
        }
        private InstallerState CurrentState;
        private bool isPartialInstall = false;
        private string partialTitle;
        private AutoInstall.InstallationType PartialType;

        public InstallationCompletionInfo[] StepInfo = new InstallationCompletionInfo[5];
        public AutoInstall Installation;
        public bool Installing = false;
        public InstallerPage()
        {
            InitializeComponent();
            CheckingBar.Value = 0;
            ExtractBar.Value = 0;
            FinalBar.Value = 0;
            FontBar.Value = 0;
            ModelBar.Value = 0;
            PatchingBar.Value = 0;
            InformationProgressBar.Value = 0;
            InformationBarText.Text = (int)(0.0 * 100) + "%";
            InstallationGrid.Visibility = Visibility.Hidden;
            MessageBackground.Visibility = Visibility.Hidden;            
            LoadUserPrefs();
        }

        void ShowMessage(string title, string body, string OKbutton = "OK", string cancelButton = "Cancel")
        {
            MsgTitle.Text = title;
            MsgBody.Inlines.Clear();
            MsgBody.Text = body;            
            MsgOKButton.Content = OKbutton;
            MsgCancelButton.Content = cancelButton;
            MessageBackground.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Tells the main window to switch focus to Installer screen
        /// </summary>
        void GetAttention()
        {
            ((MainWindow)Application.Current.MainWindow).SwitchScreen(MainWindow.ScreenState.Installer);
        }

        void HideMessage() => MessageBackground.Visibility = Visibility.Collapsed;

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartInstallation();
        }

        void ExtractTarGA(string path, StinkyFile.ManifestChunk chunk, StinkyFile.Primitive.RefPrim<bool> success)
        {
            try
            {
                using (var targa = TargaImage.LoadTargaImage(path))
                {
                    using (var fileStream = File.OpenWrite(
                        System.IO.Path.Combine(Installation.DestinationDirectory,
                        chunk.Name.Remove(chunk.Name.Length - 4, 4) + ".png")))
                    {
                        targa.Save(fileStream, ImageFormat.Png);
                    }
                }
                var destpath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".png");
                Installation.FileChanged(destpath, FileChange.ADD);
                File.Delete(path);
            }
            catch(Exception e)
            {
                success.Value = false;
                return;
            }
            success.Value = true;
        }

        void RunFontConverter(string path, RefPrim<Boolean> success)
        {
            success.Value = false;
            var converter = new FontConverter(path);
            var destination = System.IO.Path.Combine(path, "font.bmp");
            try
            {
                converter.Convert(destination);
                Installation.FileChanged(destination, FileChange.ADD);
                success.Value = true;
            }
            catch(Exception e)
            {
                Installation.PushError(e.Message);
                success.Value = false;
            }
        }

        void LoadUserPrefs()
        {
            var config = Properties.Settings.Default;            
            BlenderPath.Text = config.BlenderDir;
            WorkspaceDir.Text = config.DestinationPath;
            if (string.IsNullOrWhiteSpace(WorkspaceDir.Text))
                WorkspaceDir.Text = Path.GetDirectoryName(Environment.CurrentDirectory); // jump up one directory
            InstallPath.Text = config.SourcePath;
            if (AutoInstall.CanUninstall(WorkspaceDir.Text))
            {
                UninstallButton.Visibility = Visibility.Visible;
                StartButton.Content = "Repair Installation";
            }
            else
            {
                UninstallButton.Visibility = Visibility.Hidden;
                StartButton.Content = "Start Installation";
            }
        }

        void SaveUserPrefs()
        {
            var config = Properties.Settings.Default;
            config.BlenderDir = BlenderPath.Text;
            config.DestinationPath = WorkspaceDir.Text;
            config.SourcePath = InstallPath.Text;
            config.Save();
        }        

        void StartInstallation(StinkyFile.Installation.AutoInstall.InstallationType Type = default)
        {
            if (Installing)
            {
                MessageBox.Show("An installation is in progress. The current installation must be canceled or complete " +
                    "to start another one.", "Cannot Start Installation");
                return;
            }
            //Save user prefs
            SaveUserPrefs();
            //UI enable/disable
            PreferencesGrid.IsEnabled = false;            
            //clear errors
            ErrorStack.Children.Clear();
            //started text
            var textBlock = new TextBlock()
            {
                Text = $"{(Type == default ? "Full" : AutoInstall.GetFriendlyStepName(Type))}" +
                        $" Installation starting...",
                Foreground = Brushes.DarkCyan,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            ErrorStack.Children.Add(textBlock);
            //create installation context
            Installation = new StinkyFile.Installation.AutoInstall()
            {
                DestinationDirectory = WorkspaceDir.Text,
                BlenderPath = BlenderPath.Text,
                SourceDirectory = InstallPath.Text,
                OnTgaExtracting = (object s, (string path, StinkyFile.ManifestChunk chunk, RefPrim<bool> success) tuple) =>
                                        ExtractTarGA(tuple.path, tuple.chunk, tuple.success),
                OnFontCreate = (object s, (string path, RefPrim<bool> success) tuple) =>
                                        RunFontConverter(tuple.path, tuple.success),
                OnProgressChanged = (object s, InstallationCompletionInfo info) => InstallationInformationChanged(info),
                OnStepStarted = StepStarted,
                OnErrorRaised = (object s, string error) => SafePushError(error, Brushes.Red)
            };
            Installation.OnPausedStateChanged += Installation_OnPausedStateChanged;
            Installing = true;
            if (Type != default)
            {
                isPartialInstall = true;
                partialTitle = AutoInstall.GetFriendlyStepName(Type);
                PartialType = Type;
            }
            else isPartialInstall = false;
            //run async task
            Task.Run(() =>
            {
                if (Type == default)
                    return Installation.Start();
                else return Installation.Start(Type);
            }).ContinueWith((Task<bool> task) => OnInstallationCompleted(task));
        }

        private void Installation_OnPausedStateChanged(object sender, bool e)
        {
            Dispatcher.Invoke(delegate
            {
                if (!e)
                    PauseButton.Content = "\xE769;";
                else PauseButton.Content = "\xE768;";
            });
        }

        /// <summary>
        /// Update the information section with current installation status
        /// </summary>
        /// <param name="info"></param>
        void InstallationInformationChanged(InstallationCompletionInfo info)
        {
            int mode = 0;
            ProgressBar target = null;
            switch (info.StepName)
            {
                case "Initialization":
                    target = CheckingBar;
                    break;
                case "Extraction":
                    target = ExtractBar;
                    break;
                case "Font Creation":
                    target = FontBar;
                    break;
                case "3D Model Creation":
                    target = ModelBar;
                    break;
                case "Finalization":
                    target = FinalBar;
                    break;
                case "Patching":
                    target = PatchingBar;
                    break;
            }
            if (target == null)
                target = InformationProgressBar; //just in case
            Dispatcher.Invoke(delegate
            {
                if (info.StandardOutput != null)
                {
#if DEBUG
                    PushError(info.StandardOutput, Brushes.Gray);
#endif
                    Log.AppendLine(info.StandardOutput);
                    StandardOutput.AppendLine(info.StandardOutput);
                }
                InstallationGrid.Visibility = Visibility.Visible;
                TaskNameBlock.Text = info.StepName;
                TimeBlock.Text = info.ElapsedTime.ToString();
                DescBlock.Text = info.CurrentTask;
                target.Maximum = InformationProgressBar.Maximum = 1.0;
                target.Value = InformationProgressBar.Value = info.PercentComplete;
                InformationBarText.Text = (int)(info.PercentComplete * 100) + "%";
            });
        }

        void ShowSuccessMessage()
        {
            CurrentState = InstallerState.GoToLauncher;
            var title = (!isPartialInstall ? "Installation" : partialTitle) + " Complete!";
            ShowMessage(title,
                "The installation seems good so far, but to be sure, you should hop into the game " +
                "and try it out! \n" +
                "\n" +
                "Use the 'Go to Launcher' button to be taken to the launch screen.",
                "Go to Launcher",
                "All set");
        }

        void OnInstallationCompleted(Task<bool> task)
        {
            Installing = false;
            bool success = false;
            Dispatcher.Invoke(delegate
            {
                try
                {
                    success = task.Result;
                    var exception = task.Exception;
                    var errors = Installation.Errors.ToList();
                    if (exception != null)
                        PushError(exception.Message);
                }
                catch (Exception e)
                {
                    PushError(e.Message);
                }
                PreferencesGrid.IsEnabled = true;
                if (success)
                    InstallationGrid.Visibility = Visibility.Hidden;
                //foreach (var error in Installation.Errors)                
                //  PushError(error);                
                var textBlock2 = new TextBlock()
                {
                    Text = $"\n=== FILES INSTALLED ======== \n{Installation.Manifest}",
                    Foreground = Brushes.Orange,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.NoWrap
                };
                ErrorStack.Children.Add(textBlock2);
                var textBlock1 = new TextBlock()
                {
                    Text = $"Installation completed {(!success ? "un" : "")}successfully in " + Installation.ElapsedTime.ToString(),
                    Foreground = Brushes.DarkCyan,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap
                };
                ErrorStack.Children.Add(textBlock1);
                DumpLog(success);
                if (!success)
                    ; // TODO: Non-successful installation help
                ErrorScroller.ScrollToBottom();
                LoadUserPrefs();
                if (success)
                    ShowSuccessMessage();
            });
        }

        void DumpLog(bool success)
        {
            using (var file = File.CreateText("log.txt"))
            {                
                file.WriteLine("=========== INSTALL LOG " + DateTime.Now.ToString() + "===========");
                file.WriteLine(Log.ToString());
                file.WriteLine($"=== FILES INSTALLED ======== \n{Installation.Manifest}");
                file.WriteLine($"Installation completed {(!success ? "un" : "")}successfully in " + Installation.ElapsedTime.ToString());
            }
        }

        void SafePushError(string error, Brush foreground = default) => Dispatcher.Invoke(() => PushError(error, foreground));

        void PushError(string error, Brush Foreground = default)
        {
            if (Foreground == default) Foreground = Brushes.Red;
            var textBlock = new TextBlock()
            {
                Text = error,
                Foreground = Foreground,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            ErrorStack.Children.Add(textBlock);
            Log.AppendLine(error);
        }

        void StepStarted(object s, StinkyFile.Installation.AutoInstall.InstallationType Type)
        {
            if (Type == AutoInstall.InstallationType.CreateModels)
                Dispatcher.Invoke(PromptBlenderPlugin);           
            if (Type == AutoInstall.InstallationType.Finish)
            {
                if (isPartialInstall)
                    return;
                Installation.Paused = true;
                //Patch krabby quest, then finally check the manifest in the finalization step
                PatchKrabbyQuest(false).ContinueWith(delegate { Installation.Paused = false; });
            }            
        }

        /// <summary>
        /// Path change button pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FolderBrowser_Click(object sender, RoutedEventArgs e)
        {
            TextBox destinationBox = ((sender as Button).Parent as Panel).Children.OfType<TextBox>().FirstOrDefault();
            if (destinationBox == null)
            {
                (sender as Button).IsEnabled = false;
                return;
            }
            string path = destinationBox.Text;
            if (FileBrowser.BrowseForDirectory(ref path) == System.Windows.Forms.DialogResult.Cancel)
                return; // cancelled
            destinationBox.Text = path;
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            Installation?.TogglePause();
        }

        private void FileBrowser_Click(object sender, RoutedEventArgs e)
        {            
            TextBox destinationBox = ((sender as Button).Parent as Panel).Children.OfType<TextBox>().FirstOrDefault();
            if (destinationBox == null)
            {
                (sender as Button).IsEnabled = false;
                return;
            }
            string path = destinationBox.Text;            
            if (FileBrowser.BrowseForFile(ref path) == System.Windows.Forms.DialogResult.Cancel)
                return; // cancelled
            destinationBox.Text = path;
        }

        private async Task PatchKrabbyQuest(bool partial)
        {
            string path = null;
            Dispatcher.Invoke(delegate
            {
                path = Path.Combine(WorkspaceDir.Text);
            });
            if (path == null) return;
            GamePatcher patcher = new GamePatcher(path)
            {
                ProgressChanged = (object s, (string m, double p) e) =>
                    InstallationInformationChanged(new InstallationCompletionInfo()
                    {
                        StepName = "Patching",
                        CurrentTask = e.m,
                        PercentComplete = e.p
                    }),
            };
            if (partial)
            {
                isPartialInstall = true;
                partialTitle = "Game Update";
            }
            if (Installation == null)
            {
                if (!partial) partial = true;
                Installation = new AutoInstall()
                {
                    DestinationDirectory = path,
                    OnProgressChanged = (object s, InstallationCompletionInfo info) => InstallationInformationChanged(info)
                };
            }
            await Task.Run(() => patcher.Start(Installation)).ContinueWith((Task<bool> task) =>
            {
                if (task.IsFaulted)
                    SafePushError(task.Exception.InnerException.Message);
                if (partial)
                {
                    try
                    {
                        Installation.Start(AutoInstall.InstallationType.Finish, false);
                    }
                    catch (Exception e)
                    {
                        SafePushError(e.Message);
                    }
                    OnInstallationCompleted(task);
                }
                var settings = Properties.Settings.Default;
                lock(settings)
                {
                    if (!settings.IsSynchronized)
                        ;
                    settings.GameDir = patcher.GameDir;
                    settings.GameExePath = patcher.GameExePath;
                    settings.EditorDir = patcher.EditorDir;
                    settings.EditorExePath = patcher.EditorExePath;
                    settings.Save();
                }
            });
        }

        private void StartInstallStep(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StinkyFile.Installation.AutoInstall.InstallationType type = AutoInstall.InstallationType.Unknown;
            if (sender.Equals(ExtractionStepBorder)) type = AutoInstall.InstallationType.ExtractContent;
            if (sender.Equals(FontBorder)) type = AutoInstall.InstallationType.CreateFont;
            if (sender.Equals(ModelsBorder)) type = AutoInstall.InstallationType.CreateModels;
            if (sender.Equals(PatchingBorder)) // patching step handled outside of StinkyFile
            {
                PatchKrabbyQuest(true);
                return;
            }
            if (type != default)
                StartInstallation(type);
        }        

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentState = InstallerState.AskingToUninstall;
            ShowMessage("Uninstall KrabbyQuestEmu?", 
                "Are you sure you want to uninstall the game? You can reinstall the game again later by " +
                "pressing Start Installation.");            
        }

        private void MsgOKButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentState)
            {
                case InstallerState.AskingToUninstall:
                    StartInstallation(AutoInstall.InstallationType.Uninstallation);
                    break;
                case InstallerState.AskingToInstall3D:
                    GetBlenderPlugin();
                    break;
                case InstallerState.WaitingForCompletion3D:
                    Installation.Paused = false;
                    break;
                case InstallerState.GoToLauncher:
                    (Application.Current.MainWindow as MainWindow).SwitchScreen(MainWindow.ScreenState.Launcher);
                    break;
            }
            CurrentState = InstallerState.NoSpecialState;
            HideMessage();
        }

        private void MsgCancelButton_Click(object sender, RoutedEventArgs e)
        {
            switch (CurrentState) 
            {
                case InstallerState.AskingToInstall3D:
                    WaitForUserCompletion();                    
                    break;
                case InstallerState.WaitingForCompletion3D:
                    Installation.Skip();
                    goto default;
                default:
                    HideMessage();
                    CurrentState = InstallerState.NoSpecialState;
                    break;
            }                                    
        }

        #region Blender Installation
        private void PromptBlenderPlugin()
        {
            Installation.Paused = true;
            ShowMessage("Blender Addon Required",
                "KrabbyQuestEmu requires all models to be converted. " +
                "A Blender addon is required to do this, as Blender cannot do this conversion " +
                "normally. \n \n" +
                "Do you wish to proceed with the installation? (If the plug-in is already installed, you can click 'No')",
                "Yes", "No/Installed Already");
            CurrentState = InstallerState.AskingToInstall3D;
        }

        private void WaitForUserCompletion()
        {
            CurrentState = InstallerState.WaitingForCompletion3D;
            ShowMessage("Waiting for Blender...",
                        "The installation is paused. Once the Blender addon has been \n" +
                        "successfully installed, the 'All Set' button will be available. \n \n" +
                        "If you do not want to install the 3D models right now, " +
                        "click 'Do it later'",
                        "All set", "Do it later");
        }

        private void GetBlenderPlugin()
        {
            string blender = BlenderPath.Text;
            Task.Run(async () =>
            {
                DataReceivedEventHandler action = (object s, DataReceivedEventArgs e) => SafePushError(e.Data, Brushes.Gray);
                //download plugin
                string path = Path.Combine(Environment.CurrentDirectory, "plugin.zip");
                if (!File.Exists(path))
                {
                    using (var client = new WebClient())
                    {                        
                        //progress changed
                        client.DownloadProgressChanged += async (object sender, DownloadProgressChangedEventArgs e) =>
                        {
                            var args = new InstallationCompletionInfo()
                            {
                                StepName = "3D Model Creation",
                                CurrentTask = $"Downloading ({e.BytesReceived / e.TotalBytesToReceive} bytes)",
                                PercentComplete = e.ProgressPercentage / 100.0
                            };
                            InstallationInformationChanged(args);
                        };
                        await client.DownloadFileTaskAsync(
                            new Uri("https://github.com/joric/io_scene_b3d/releases/download/1.0/io_scene_b3d.zip"),
                            "plugin.zip");
                    }
                }
                string pluginPath = Path.Combine(Environment.CurrentDirectory, "InstallPlugin.py");
                PythonParameterEditor.ReplaceParameter(pluginPath, "filepath=", 0, path.Replace('\\', '/'));
                ProcessStartInfo startInfo = new ProcessStartInfo(blender, $"-b -P {pluginPath}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true
                };
                var info = Process.Start(startInfo);
                try
                {                    
                    info.BeginOutputReadLine();
                    info.OutputDataReceived += action;
                }
                catch(Exception e)
                {
                    SafePushError(e.Message);
                }
                Dispatcher.Invoke(delegate
                {
                    InstallationInformationChanged(new InstallationCompletionInfo()
                    {
                        StepName = "3D Model Creation",
                        CurrentTask = $"Installing Blender Addon...",
                        PercentComplete = 0.0
                    });
                    WaitForUserCompletion();
                    MsgOKButton.IsEnabled = false;
                    MsgOKButton.Content = "Blender is open";
                });                
                info.WaitForExit();
                try
                {
                    info.CancelOutputRead();
                    info.OutputDataReceived -= action;
                }
                catch(Exception e) {
                    PushError(e.Message);
                }
                Dispatcher.Invoke(delegate
                {
                    InstallationInformationChanged(new InstallationCompletionInfo()
                    {
                        StepName = "3D Model Creation",
                        CurrentTask = $"Installing Blender Addon...",
                        PercentComplete = 1.0
                    });                    
                    if (info.ExitCode == 0)
                    {
                        MsgTitle.Text = "Good to Go!";
                        MsgOKButton.IsEnabled = true;
                        MsgOKButton.Content = "All set";
                    }
                    else
                    {
                        MsgTitle.Text = "Blender had an error";
                        MsgOKButton.IsEnabled = false;
                        MsgOKButton.Content = "Try again later";
                    }
                });
            }).ContinueWith((Task task) =>
            {
                if (task.IsFaulted)
                {
                    SafePushError(task.Exception.Flatten().InnerException.Message);
                    Installation?.TogglePause();
                }
            });
        }
        #endregion
    }
}
