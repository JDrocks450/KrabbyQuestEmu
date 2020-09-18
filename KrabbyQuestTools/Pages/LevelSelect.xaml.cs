using StinkyFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for LevelSelect.xaml
    /// </summary>
    public partial class LevelSelect : Page
    {
        private StinkyParser Parser => AppResources.Parser;
        string Workspace, DATpath;

        public LevelSelect()
        {
            InitializeComponent();
            DATpath = Properties.Settings.Default.DataPath;
            Workspace = Properties.Settings.Default.DestinationDir;
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
        }

        private void GetLevels(string searchTerm = "")
        {
            LevelButtons.Children.Clear();
            LevelButtons.Children.Add(LevelsLabel);
            LevelButtons.Children.Add(SearchLabel);
            LevelButtons.Children.Add(SearchBox);
            int number = 0;
            string levelDir = System.IO.Path.Combine(Workspace, "levels");
            DirectoryInfo dir = new DirectoryInfo(levelDir);
            if (!dir.Exists)
            {
                MessageBox.Show("That directory does not exist. The levels must be extracted to: " + levelDir);
                return;
            }
            Parser.FindAllLevels(dir.FullName);
            foreach(var level in Parser.LevelInfo)
            {
                var button = new Button() 
                {
                    Height = 25,
                    Margin = new Thickness(0,5,0,5),
                    Content = $"({System.IO.Path.GetFileName(level.LevelFilePath)})" +
                        $": " + level.Name,
                    Tag = level
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
            NavigationService.Navigate(new StinkyUI(level));
        }

        private void FilePathScreen_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (file[0].EndsWith(".lv5")) // directly open level
                {
                    var level = Parser.LevelRead(file[0]);
                    NavigationService.Navigate(new StinkyUI(level));
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

        private void ExportModels_Click(object sender, RoutedEventArgs e)
        {
            KQTDialog dialog = new KQTDialog()
            {
                Content = new ModelExporterOptions()
            };
            dialog.ShowDialog();
        }
    }
}
