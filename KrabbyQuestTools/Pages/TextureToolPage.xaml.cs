using StinkyFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Xml.Linq;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for TextureToolPage.xaml
    /// </summary>
    public partial class TextureToolPage : Page
    {
        private string DirPath;
        private AssetDBEntry OpenEntry;
        private List<string> DirFiles = new List<string>();
        private bool unsavedChanges = false;
        Dictionary<string, Button> FileBrowserSource = new Dictionary<string, Button>();
        public TextureToolPage()
        {
            InitializeComponent();
            Title = "Asset Database Editor";
        }
        public TextureToolPage(string FilePath) : this()
        {
            DirPath = FilePath;
            PopulateFileBrowser();
            AssetTypeSwitcher.ItemsSource = Enum.GetNames(typeof(AssetType));
            GalleryView.OnDataSelected += (object sender, LevelDataBlock d) =>
            {
                if (OpenEntry.ReferencedDataBlocks.Where(x => x.GUID == d.GUID).Any())
                    return;
                OpenEntry.ReferencedDataBlocks.Add(d);
                RefreshReferences(OpenEntry);
                unsavedChanges = true;
            };
            AssetDBEntry.PushWorkspaceDir(FilePath);
        }
        public TextureToolPage(string FilePath, string AssetGuid) : this(FilePath)
        {
            var dbe = AssetDBEntry.Load(AssetGuid, true);
            if (dbe == null)
            {
                MessageBox.Show($"The AssetDBEntry using GUID: {AssetGuid} could not be found. Verify that the path: {FilePath} is " +
                    $"the correct Workspace Directory. If not, you can change it on the opening screen of the editor.");
                return;
            }
            PopulateFileInfo(dbe, false);
        }

        private void PopulateFileBrowser()
        {
            try
            {
                foreach (var directory in Directory.GetDirectories(DirPath))
                {
                    var files = Directory.GetFiles(directory);
                    foreach (var file in files)
                    {
                        var fileName = System.IO.Path.Combine(System.IO.Path.GetFileName(directory), System.IO.Path.GetFileName(file));
                        var button = new Button()
                        {
                            Content = AssetDBEntry.GetDBNameFromFileName(fileName),
                            Margin = new Thickness(10),
                            Height = 25,
                            Tag = fileName
                        };
                        button.Click += FileSelected_Click;
                        FileBrowser.Children.Add(button);
                        DirFiles.Add(fileName);
                        FileBrowserSource.Add(fileName, button);
                    }                    
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("An error occured viewing this directory... " + e);                
            }
        }

        private void FileSelected_Click(object sender, RoutedEventArgs e)
        {
            PopulateFileInfo((sender as Button).Tag as string);
        }

        private void PopulateFileInfo(string filePath)
        {
            if (unsavedChanges)
            {
                var result = MessageBox.Show("Opening a new file will delete all unsaved changes. Would you like to save?",
                    "Changes Unsaved", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result== MessageBoxResult.Cancel)                    
                    return;
                if (result == MessageBoxResult.Yes)                
                    Save();    
                unsavedChanges = false;            
            }
            var file = new FileInfo(System.IO.Path.Combine(DirPath, filePath));
            try
            {
                FilePreview.Source = new BitmapImage(new Uri(System.IO.Path.Combine(DirPath, filePath)));
                FileCorruptBlock.Visibility = Visibility.Collapsed;
            }
            catch
            {
                FilePreview.Source = null;
                FileCorruptBlock.Visibility = Visibility.Visible;
            }
            var dbe = AssetDBEntry.LoadFromFileName(filePath, out bool created);
            PopulateFileInfo(dbe, created);   
        }

        public void PopulateFileInfo(AssetDBEntry dbe, bool newlyCreated)
        {
            if (dbe == null)
            {
                return;
            }
            OpenEntry = dbe;
            FileNameBox.Text = dbe.FileName;
            var name = dbe.DBName ?? "UNKNOWN";
            DBNameBox.Text = name;
            AssetGuidBox.Text = dbe.GUID;
            if (!newlyCreated)
                AssetTypeSwitcher.SelectedItem = Enum.GetName(typeof(AssetType), dbe.Type);
            else
            {
                AssetTypeSwitcher.SelectedItem = "Texture";
                if (dbe.FileName.EndsWith(".wav"))
                    AssetTypeSwitcher.SelectedItem = "Sound";
                if (dbe.FileName.EndsWith(".obj"))
                    AssetTypeSwitcher.SelectedItem = "Model";
            }
            RefreshReferences(dbe);    
            Title = "ADE - Viewing " + name; 
        }

        private void RefreshReferences(AssetDBEntry dbe)
        {
            GUIDBox.Children.Clear();
            foreach(var guid in dbe.ReferencedDataBlocks)
            {
                var button = new Button()
                {
                    Content = guid.GUID + " " + guid.Name,
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 5, 0),
                    Background = new SolidColorBrush(AppResources.S_ColorConvert(guid.Color)),
                    Tag = guid
                };
                button.Click += delegate
                {
                    dbe.ReferencedDataBlocks.Remove((LevelDataBlock)button.Tag);
                    unsavedChanges = true;
                    RefreshReferences(dbe);                    
                };
                GUIDBox.Children.Add(button);
            }
        }

        private void Save()
        {
            OpenEntry.DBName = DBNameBox.Text;
            OpenEntry.Type = (AssetType)Enum.Parse(typeof(AssetType), (string)AssetTypeSwitcher.SelectedItem);
            OpenEntry.Save();
            unsavedChanges = false;
        }

        private void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(System.IO.Path.Combine(DirPath, OpenEntry.FileName));
        }

        private void NextFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int newIndex = DirFiles.IndexOf(System.IO.Path.Combine(DirPath, OpenEntry.FileName)) + 1;
                if (DirFiles.Count != newIndex)
                    PopulateFileInfo(DirFiles[newIndex]);
            }
            catch { }
        }

        private void PrevFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int newIndex = DirFiles.IndexOf(System.IO.Path.Combine(DirPath, OpenEntry.FileName)) + 1;
                if (newIndex >= 0)
                    PopulateFileInfo(DirFiles[newIndex]);
            }
            catch { }
        }

        private void DBNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (OpenEntry == null) return;
            if (DBNameBox.Text != OpenEntry?.DBName)
                unsavedChanges = true;
        }

        private void AssetTypeSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpenEntry == null) return;
            if ((string)AssetTypeSwitcher.SelectedItem != Enum.GetName(typeof(AssetType), OpenEntry.Type))
                unsavedChanges = true;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileBrowser.Children.Clear();
            if (string.IsNullOrWhiteSpace(SearchBox.Text)) {
                foreach (var button in FileBrowserSource.Values)
                    FileBrowser.Children.Add(button);
                return;
            }
            foreach(var button in FileBrowserSource.Where(x => x.Key.Contains(SearchBox.Text)))
                FileBrowser.Children.Add(button.Value);
        }
    }
}
