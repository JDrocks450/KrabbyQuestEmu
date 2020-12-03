using KrabbyQuestTools.Common;
using KrabbyQuestTools.Controls;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for LevelSelect.xaml
    /// </summary>
    public partial class LevelSelect : KQTPage
    {
        /// <summary>
        /// Is the message prompt hidden by default
        /// </summary>
        static bool IsPromptHidden = false;

        private StinkyParser Parser => AppResources.Parser;
        string Workspace, DATpath;
        const double scrollTimerInterval = .1, scrollTime = 3.5;
        double currentScrollTime = 0, scrollFrom, scrollTo;

        public LevelSelect()
        {
            InitializeComponent();
            DATpath = Properties.Settings.Default.DataPath;
            Workspace = Properties.Settings.Default.DestinationDir;
            string gameResources = Properties.Settings.Default.GameResourcesPath;
            if (!string.IsNullOrWhiteSpace(gameResources))            
                GamePathBox.Text = gameResources;            
            else GamePathBox.BorderBrush = Brushes.Red;
            if (AppResources.Parser == null)
                AppResources.Parser = new StinkyParser();
            if (!File.Exists(LevelDataBlock.BlockDatabasePath))
            {
                MessageBox.Show("The BlockDB (blockdb.xml) and AssetDB (texturedb.xml) are expected here: "
                    + System.IO.Path.Combine(Environment.CurrentDirectory, LevelDataBlock.BlockDatabasePath) +
                    ", make sure to move them from <SolutionFolder>/Resources to this path on first run!");
                Environment.Exit(0);
            }
            if (!string.IsNullOrWhiteSpace(Workspace))
            {
                GetLevels();
                WorkspacePath.Text = Workspace;
            }
            Title = "Editor Homepage";
            if (IsPromptHidden)
                MessagePrompt.Visibility = Visibility.Collapsed;
        }

        private void GetLevels(string searchTerm = "")
        {
            LevelButtons.Children.Clear();
            int number = 0;
            string levelDir = System.IO.Path.Combine(Workspace, "levels");
            DirectoryInfo dir = new DirectoryInfo(levelDir);
            if (!dir.Exists)
            {
                MessageBox.Show("That directory does not exist. The levels must be extracted to: " + levelDir);
                return;
            }
            Parser.FindAllLevels(dir.FullName);
            foreach (var level in Parser.LevelInfo)
            {
                bool allow = false;
                if (searchTerm != "")
                {
                    if (level.Name.Contains(searchTerm) || level.LevelWorldName.Contains(searchTerm))
                        allow = true;
                }
                else
                    allow = true;
                if (!allow) continue;
                var button = new Button()
                {
                    Height = 75,
                    Width = 100,
                    Margin = new Thickness(10),
                    Content = new TextBlock()
                    {
                        Text = $"({System.IO.Path.GetFileName(level.LevelFilePath)})" +
                        $": " + level.Name,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    },
                    Tag = level,
                    Background = Brushes.DarkCyan,
                    BorderBrush = Brushes.Cyan
                };
                button.Click += Level_Click;
                LevelButtons.Children.Add(button);
                number++;
            }
        }

        private void Level_Click(object sender, RoutedEventArgs e)
        {
            StinkyLevel level = (StinkyLevel)(sender as Button).Tag;
            Parser.RefreshLevel(level);
            NavigationService.Navigate(new StinkyUI(level, Workspace));
        }

        private void FilePathScreen_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (file[0].EndsWith(".lv5")) // directly open level
                {
                    var level = Parser.LevelRead(file[0]);
                    NavigationService.Navigate(new StinkyUI(level, Workspace));
                    return;
                }                
                WorkspacePath.Text = file[0];
            }
        }

        private void FilePathSubmit_Click(object sender, RoutedEventArgs e)
        {
            Workspace = WorkspacePath.Text;
            GetLevels();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GetLevels(SearchBox.Text);
        }

        private void AssetEditorButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new TextureToolPage(WorkspacePath.Text));
        }

        private void DatabaseOptions_Click(object sender, RoutedEventArgs e)
        {
            KQTDialog dialog = new KQTDialog()
            {
                Content = new DatabaseOptions()
            };
            dialog.ShowDialog();
        }

        private void GalleryViewerButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GalleryPage());
        }

        private void ManifestViewerButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ManifestViewer());
        }

        private void FontMakerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var path = System.IO.Path.Combine(Workspace, "Graphics");
                var converter = new FontConverter(path);
                converter.Convert(System.IO.Path.Combine(path, "font.bmp"));
                (sender as Button).Content = "Converted";
            }
            catch
            {
                MessageBox.Show("An error has occured, make sure the Workspace is set correctly and that assets have been dumped using the ManifestViewer.");
            }
        }

        private void MapScreenEditor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new MapScreenCustomizer(Workspace));
        }

        private void SaveEditor_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new SaveFileViewer(System.IO.Path.Combine(Workspace, "levels")));
        }

        private void OpenLevelButton_Click(object sender, RoutedEventArgs e)
        {
            LevelCategory.Visibility = Visibility.Visible;
            //scroll animation
            Timer timer = new Timer(scrollTimerInterval);
            currentScrollTime = 0;
            scrollFrom = CategoryViewer.VerticalOffset;
            scrollTo = LevelCategory.TranslatePoint(new Point(0,0), CategoryViewer).Y;
            timer.Elapsed += delegate
            {
                double percentage = currentScrollTime / scrollTime;
                double offset = scrollFrom + ((scrollTo - scrollFrom) * percentage);
                percentage += scrollTimerInterval;
                if (percentage >= 1)
                {
                    offset = scrollTo;
                    timer.Stop();
                }
                Dispatcher.Invoke(() =>
                {
                    CategoryViewer.ScrollToVerticalOffset(offset);
                });
                currentScrollTime += scrollTimerInterval;
            };
            timer.Start();
        }

        private void PushAllChanges_Click(object sender, RoutedEventArgs e)
        {
            string blockDBpath1 = LevelDataBlock.BlockDatabasePath, blockDBpath2 = System.IO.Path.Combine(GamePathBox.Text, "blockdb.xml");
            string assetDBpath1 = AssetDBEntry.AssetDatabasePath, assetDBpath2 = System.IO.Path.Combine(GamePathBox.Text, "texturedb.xml");
            File.Copy(blockDBpath1, blockDBpath2, true);
            File.Copy(assetDBpath1, assetDBpath2, true);
            Properties.Settings.Default.GameResourcesPath = GamePathBox.Text;
            Properties.Settings.Default.Save();
            RefreshAllChanges();
        }

        private void GamePathBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GamePathBox.Text))
                GamePathBox.BorderBrush = Brushes.Red;
            else GamePathBox.BorderBrush = Brushes.Gray;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshAllChanges();
        }

        private void RefreshChanges_Click(object sender, RoutedEventArgs e)
        {
            RefreshAllChanges();
        }

        private void MessagePromptOKButton_Click(object sender, RoutedEventArgs e)
        {
            MessagePrompt.Visibility = Visibility.Collapsed;
            IsPromptHidden = true;
        }

        private void OpenInstallerButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void DiffChecker_Click(object sender, RoutedEventArgs e)
        {
            string blockDBpath1 = LevelDataBlock.BlockDatabasePath, blockDBpath2 = System.IO.Path.Combine(GamePathBox.Text, "blockdb.xml");
            NavigationService.Navigate(new DiffPage(blockDBpath2, blockDBpath1));
        }

        private void ExportModels_Click(object sender, RoutedEventArgs e)
        {
            KQTDialog dialog = new KQTDialog()
            {
                Content = new ModelExporterOptions()
            };
            dialog.ShowDialog();
        }
        public override void OnActivated()
        {
            RefreshAllChanges();
            base.OnActivated();
        }
        string editorDefinition, gameDefinition;
        private void RefreshAllChanges()
        {
            bool changes = false;
            EditorStack.Children.Clear();
            GameStack.Children.Clear();
            if (editorDefinition == null)
            {
                editorDefinition = AppResources.XamlToString(SampleEditor);
                gameDefinition = AppResources.XamlToString(SampleGame);
            }
            DockPanel getRow(DockPanel dockPanel,string fileName, string dateModified, long byteChange, RoutedEventHandler onPush)
            {
                Brush brush = Brushes.Orange;
                if (byteChange > 0)
                    brush = Brushes.Green;
                else if (byteChange < 0) brush = Brushes.Red;
                (dockPanel.Children[2] as TextBlock).Text = fileName;
                (dockPanel.Children[3] as TextBlock).Foreground = brush;
                (dockPanel.Children[3] as TextBlock).Text = (byteChange > 0 ? "+" : "") + byteChange.ToString() + " bytes";
                (dockPanel.Children[4] as TextBlock).Text = dateModified;
                (dockPanel.Children[5] as Button).Click += onPush;
                return dockPanel;
            }
            DockPanel getEditorRow(string fileName, string dateModified, long byteChange, RoutedEventHandler onPush) => 
                getRow(AppResources.CloneXaml<DockPanel>(editorDefinition), fileName, dateModified, byteChange, onPush);                    
            DockPanel getGameRow(string fileName, string dateModified, long byteChange, RoutedEventHandler onPush)=>
                getRow(AppResources.CloneXaml<DockPanel>(gameDefinition), fileName, dateModified, byteChange, onPush);
            void Push(FileInfo fInfo, string destination)
            {
                fInfo.CopyTo(destination, true);
                Properties.Settings.Default.GameResourcesPath = GamePathBox.Text;
                Properties.Settings.Default.Save();
            }
            void Revert(FileInfo fInfo, string destination)
            {
                if (MessageBox.Show("This will delete any changes currently unsaved and restore the selected file to the Game's current version. " +
                    "This cannot be undone.", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    fInfo.CopyTo(destination, true);
                    Properties.Settings.Default.GameResourcesPath = GamePathBox.Text;
                    Properties.Settings.Default.Save();
                }
            }
            string blockDBpath1 = LevelDataBlock.BlockDatabasePath, blockDBpath2 = System.IO.Path.Combine(GamePathBox.Text, "blockdb.xml");
            string assetDBpath1 = AssetDBEntry.AssetDatabasePath, assetDBpath2 = System.IO.Path.Combine(GamePathBox.Text, "texturedb.xml");
            FileInfo info1 = new FileInfo(blockDBpath1), info2 = new FileInfo(blockDBpath2);
            if (info1.Exists && info2.Exists)
            {
                if (info1.LastWriteTime.ToFileTime() != info2.LastWriteTime.ToFileTime()) // one is modified
                {
                    changes = true;
                    string time = info1.LastWriteTime.ToShortDateString() + " " + info1.LastWriteTime.ToShortTimeString();
                    EditorStack.Children.Add(
                        getEditorRow("Block Database",
                        time,
                        info1.Length - info2.Length,
                        delegate
                        {
                            Push(info1, blockDBpath2);
                            RefreshAllChanges();
                        }));
                    time = info2.LastWriteTime.ToShortDateString() + " " + info2.LastWriteTime.ToShortTimeString();
                    GameStack.Children.Add(
                        getGameRow("Block Database",
                        time,
                        info2.Length - info1.Length,
                        delegate
                        {
                            Revert(info2, blockDBpath1);
                            RefreshAllChanges();
                        }));
                }
            }
            FileInfo ainfo1 = new FileInfo(assetDBpath1), ainfo2 = new FileInfo(assetDBpath2);
            if (ainfo1.Exists && ainfo2.Exists)
            {
                if (ainfo1.LastWriteTime.ToFileTime() != ainfo2.LastWriteTime.ToFileTime()) // one is modified
                {
                    changes = true;
                    string time = ainfo1.LastWriteTime.ToShortDateString() + " " + ainfo1.LastWriteTime.ToShortTimeString();
                    EditorStack.Children.Add(
                        getEditorRow("Asset Database",
                        time,
                        ainfo1.Length - ainfo2.Length,
                        delegate
                        {
                            Push(ainfo1, assetDBpath2);
                            RefreshAllChanges();
                        }));
                    time = ainfo2.LastWriteTime.ToShortDateString() + " " + ainfo2.LastWriteTime.ToShortTimeString();
                    GameStack.Children.Add(
                        getGameRow("Asset Database",
                        time,
                        ainfo2.Length - ainfo1.Length,
                        delegate
                        {
                            Revert(ainfo2, assetDBpath1);
                            RefreshAllChanges();
                        }));
                }
            }
            PushAllChanges.IsEnabled = changes;
        }
    }
}
