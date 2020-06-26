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

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for TextureToolPage.xaml
    /// </summary>
    public partial class TextureToolPage : Page
    {
        private string DirPath;
        private AssetDBEntry OpenEntry;
        private string[] DirFiles;
        private bool unsavedChanges = false;
        public TextureToolPage()
        {
            InitializeComponent();
            WindowTitle = "Asset Database Editor";
        }
        public TextureToolPage(string FilePath) : this()
        {
            DirPath = FilePath;
            PopulateFileBrowser();
            AssetTypeSwitcher.ItemsSource = Enum.GetNames(typeof(AssetType));
            GalleryView.OnDataSelected += (object sender, LevelDataBlock d) =>
            {
                OpenEntry.ReferencedDataBlockGuids.Add(d);
                RefreshReferences(OpenEntry);
                unsavedChanges = true;
            };
        }

        private void PopulateFileBrowser()
        {
            try
            {
                DirFiles = Directory.GetFiles(DirPath);
                foreach (var file in DirFiles)
                {
                    var button = new Button()
                    {
                        Content = AssetDBEntry.GetDBNameFromFileName(file),
                        Margin = new Thickness(10),
                        Height = 25,
                        Tag = file
                    };
                    button.Click += FileSelected_Click;
                    FileBrowser.Children.Add(button);
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
            }
            var file = new FileInfo(filePath);
            try
            {
                FilePreview.Source = new BitmapImage(new Uri(filePath));
                FileCorruptBlock.Visibility = Visibility.Collapsed;
            }
            catch
            {
                FilePreview.Source = null;
                FileCorruptBlock.Visibility = Visibility.Visible;
            }
            var dbe = AssetDBEntry.LoadFromFilePath(filePath);
            OpenEntry = dbe;
            FileNameBox.Text = filePath;
            DBNameBox.Text = dbe.DBName;
            AssetTypeSwitcher.SelectedItem = Enum.GetName(typeof(AssetType), dbe.Type);
            RefreshReferences(dbe);    
            WindowTitle = "Asset Database Editor - " + AssetDBEntry.GetDBNameFromFileName(filePath);
        }

        private void RefreshReferences(AssetDBEntry dbe)
        {
            GUIDBox.Children.Clear();
            foreach(var guid in dbe.ReferencedDataBlockGuids)
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
                    dbe.ReferencedDataBlockGuids.Remove((LevelDataBlock)button.Tag);
                    RefreshReferences(dbe);
                    unsavedChanges = true;
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
            Process.Start(OpenEntry.FilePath);
        }

        private void NextFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int newIndex = Array.IndexOf(DirFiles, OpenEntry.FilePath) + 1;
                if (DirFiles.Length != newIndex)
                    PopulateFileInfo(DirFiles[newIndex]);
            }
            catch { }
        }

        private void PrevFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int newIndex = Array.IndexOf(DirFiles, OpenEntry.FilePath) - 1;
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
    }
}
