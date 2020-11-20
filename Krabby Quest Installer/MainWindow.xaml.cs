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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StringBuilder StandardOutput = new StringBuilder();
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
            WaitingForCompletion3D
        }
        private InstallerState CurrentState;
        public InstallationCompletionInfo[] StepInfo = new InstallationCompletionInfo[5];
        public AutoInstall Installation;
        public MainWindow()
        {
            InitializeComponent();
            CheckingBar.Value = 0;
            ExtractBar.Value = 0;
            FinalBar.Value = 0;
            FontBar.Value = 0;
            ModelBar.Value = 0;
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
            //Save user prefs
            SaveUserPrefs();
            //UI enable/disable
            PreferencesGrid.IsEnabled = false;            
            //clear errors
            ErrorStack.Children.Clear();
            //started text
            var textBlock = new TextBlock()
            {
                Text = $"{(Type == default ? "Complete" : Enum.GetName(typeof(StinkyFile.Installation.AutoInstall.InstallationType), Type))}" +
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
                OnStepStarted = StepStarted
            };
            //run in async context
            Task.Run(() =>
            {
                if (Type == default)
                    return Installation.Start();
                else return Installation.Start(Type);
            }).ContinueWith((Task<bool> task) => OnInstallationCompleted(task));
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

        void OnInstallationCompleted(Task<bool> task)
        {
            bool success = task.Result;
            var exception = task.Exception;
            var errors = Installation.Errors.ToList();
            if (exception != null)
                errors.Add(exception.Message);
            Dispatcher.Invoke(delegate
            {
                PreferencesGrid.IsEnabled = true;
                InstallationGrid.Visibility = Visibility.Hidden;
                if (!success)
                    PushError(StandardOutput.ToString(), Brushes.Gray);
                foreach (var error in Installation.Errors)                
                    PushError(error);                
                var textBlock2 = new TextBlock()
                {
                    Text = $"=== FILES INSTALLED ======== \n{Installation.Manifest}",
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
                if (!success)
                    ; // TODO: Non-successful installation help
                ErrorScroller.ScrollToBottom();
                LoadUserPrefs();
            });
            
        }

        void SavePushError(string error) => Dispatcher.Invoke(() => PushError(error));

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
        }

        void StepStarted(object s, StinkyFile.Installation.AutoInstall.InstallationType Type)
        {
            if (Type == AutoInstall.InstallationType.CreateModels)
                Dispatcher.Invoke(PromptBlenderPlugin);                                                    
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
            System.Windows.Forms.FolderBrowserDialog fBrowserdiag = new System.Windows.Forms.FolderBrowserDialog()
            {
                Description = "Select a folder to save files to",
                SelectedPath = destinationBox.Text,
                ShowNewFolderButton = true
            };
            if (fBrowserdiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return; // cancelled
            destinationBox.Text = fBrowserdiag.SelectedPath;
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
            System.Windows.Forms.OpenFileDialog fBrowserdiag = new System.Windows.Forms.OpenFileDialog()
            {
                InitialDirectory = @"C:\Program Files",
                Title = "Find Blender.exe on your PC",
                FileName = path,
                Multiselect = false,
                AutoUpgradeEnabled = true,
                AddExtension = true,
                DereferenceLinks = true,
                DefaultExt = "exe",
                ValidateNames = true,                
            };
            if (fBrowserdiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return; // cancelled
            destinationBox.Text = fBrowserdiag.FileName;
        }

        private void StartInstallStep(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StinkyFile.Installation.AutoInstall.InstallationType type = AutoInstall.InstallationType.Unknown;
            if (sender.Equals(ExtractionStepBorder)) type = AutoInstall.InstallationType.ExtractContent;
            if (sender.Equals(FontBorder)) type = AutoInstall.InstallationType.CreateFont;
            if (sender.Equals(ModelsBorder)) type = AutoInstall.InstallationType.CreateModels;
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
                        "The model installation will open command-windows one after another. \n" +
                        "This is normal, but can be distracting. This process should not take long. \n \n" +
                        "If you do not want to install the 3D models right now, " +
                        "click 'Do it later'",
                        "All set", "Do it later");
        }

        private void GetBlenderPlugin()
        {
            string blender = BlenderPath.Text;
            Task.Run(delegate
            {
                //download plugin
                string path = Path.Combine(Environment.CurrentDirectory, "plugin.zip");
                if (!File.Exists(path))
                {
                    using (var client = new WebClient())
                    {
                        client.DownloadFileAsync(
                            new Uri("https://github.com/joric/io_scene_b3d/releases/download/1.0/io_scene_b3d.zip"),
                            "plugin.zip");
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
                        bool completed = false;
                        client.DownloadFileCompleted += delegate { completed = true; };
                        while (!completed) { } // wait for completion
                    }
                }
                string pluginPath = Path.Combine(Environment.CurrentDirectory, "InstallPlugin.py");
                PythonParameterEditor.ReplaceParameter(pluginPath, "filepath=", 0, path);
                var info = Process.Start(blender, $"-b -P {path}");
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
                Dispatcher.Invoke(delegate
                {
                    InstallationInformationChanged(new InstallationCompletionInfo()
                    {
                        StepName = "3D Model Creation",
                        CurrentTask = $"Installing Blender Addon...",
                        PercentComplete = 1.0
                    });
                    MsgTitle.Text = "Good to Go!";
                    MsgOKButton.IsEnabled = true;
                    MsgOKButton.Content = "All set";
                });
            });
        }
        #endregion
    }
}
