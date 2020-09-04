using StinkyFile;
using System;
using System.Collections.Generic;
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
        public LevelSelect()
        {
            InitializeComponent();
            var DATpath = Properties.Settings.Default.DataPath;
            var Workspace = Properties.Settings.Default.DestinationDir;
            if (!string.IsNullOrWhiteSpace(DATpath))
            {
                FilePathBox.Text = DATpath;
                GetLevels();
            }
            WorkspacePath.Text = Workspace;
            Title = "Editor Homepage";
        }

        private void GetLevels(string searchTerm = "")
        {
            LevelButtons.Children.Clear();
            LevelButtons.Children.Add(LevelsLabel);
            LevelButtons.Children.Add(SearchLabel);
            LevelButtons.Children.Add(SearchBox);
            try
            {
                if (AppResources.Parser == null)
                    AppResources.Parser = new StinkyParser(FilePathBox.Text);
            }
            catch (Exception e)
            {
                MessageBox.Show("There was an error loading from that directory. " + e);
            }
            var source = (searchTerm != "") ? Parser.LevelIndices.Where(x => x.Value.Contains(SearchBox.Text)) : Parser.LevelIndices;
            int number = 0;
            foreach(var level in source)
            {
                var button = new Button() 
                {
                    Height = 25,
                    Margin = new Thickness(0,5,0,5),
                    Content = $"({number}) [{level.Key}]: " + level.Value,
                    Tag = level.Key 
                };
                button.Click += Level_Click;
                LevelButtons.Children.Add(button);
                number++;
            }
        }

        private void Level_Click(object sender, RoutedEventArgs e)
        {
            int index = (int)(sender as Button).Tag;
            Parser.LevelRead(index);
            NavigationService.Navigate(new StinkyUI());
        }

        private void FilePathScreen_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                FilePathBox.Text = file[0];
            }
        }

        private void FilePathSubmit_Click(object sender, RoutedEventArgs e)
        {
            AppResources.Parser = new StinkyParser(FilePathBox.Text);
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
