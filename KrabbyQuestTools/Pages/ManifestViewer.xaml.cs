using Paloma;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
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
    /// Interaction logic for ManifestViewer.xaml
    /// </summary>
    public partial class ManifestViewer : Page
    {
        FileStream ManifestSource, DataSource;
        ManifestParser Parser;
        Color[] FileMapColors;
        Dictionary<ManifestChunk, Border> FileMapBorders = new Dictionary<ManifestChunk, Border>();
        ManifestChunk OpenChunk;
        string Destination;
        bool tgaConvert;

        public ManifestViewer()
        {
            InitializeComponent();
            Parser = new ManifestParser();
            LoadingCover.Visibility = Visibility.Collapsed;
        }

        private void ManifestBox_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void ManifestBox_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void ManifestBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                (sender as TextBox).Text = file[0];
            }            
        }

        private void ManifestBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void RefreshOpenedFile(string ManifestPath, string DataPath)
        {
            PathViewer.Children.Clear();
            Parser.ManifestPath = ManifestPath;
            Parser.DataPath = DataPath;
            DestinationBox.BorderBrush = Brushes.Gray;
            ManifestBox.BorderBrush = Brushes.Gray;
            DataBox.BorderBrush = Brushes.Gray;
            Parser.Refresh();
            bool error = false;
            if (string.IsNullOrWhiteSpace(DestinationBox.Text))
            {
                DestinationBox.BorderBrush = Brushes.Red;
                error = true;
            }
            if (Parser.ManifestPathException != null)
            {                
                PathViewer.Children.Add(
                    new Button()
                    {
                        Content = new TextBlock()
                        {
                            Text = "Manifest Path Error: " + Parser.ManifestPathException.Message,
                            TextWrapping = TextWrapping.Wrap
                        },
                        Padding = new Thickness(10,5,10,5),
                        Background = Brushes.Red,
                        Margin = new Thickness(10)
                    });
                ManifestBox.BorderBrush = Brushes.Red;
                error = true;
            }            
            if (Parser.DataPathException != null)
            {        
                DataBox.BorderBrush = Brushes.Red;
                PathViewer.Children.Add(
                    new Button()
                    {
                        Content = new TextBlock()
                        {
                            Text = "Data Path Error: " + Parser.ManifestPathException.Message,
                            TextWrapping = TextWrapping.Wrap
                        },
                        Padding = new Thickness(10,5,10,5),
                        Background = Brushes.Red,
                        Margin = new Thickness(10)
                    });
                return;
            }
            if (error)
                return;                        
            ManifestSource = Parser.ManifestSource;
            DataSource = Parser.DataSource;
            Properties.Settings.Default.ManifestPath = ManifestPath;
            Properties.Settings.Default.DataPath = DataPath;
            Properties.Settings.Default.DestinationDir = DestinationBox.Text;
            Destination = DestinationBox.Text;
            FileMap.Children.Clear();
            FileMap.ColumnDefinitions.Clear();
            FileMapBorders.Clear();
            FileMapColors = AppResources.BatchRandomColor(Parser.Chunks.Count);
            foreach(var chunk in Parser.Chunks)
            {
                AddToFileMap(chunk);
                var button = new Button()
                {
                    Content = chunk.Name,
                    Margin = new Thickness(10),
                    Height = 25,
                    Tag = chunk
                };
                button.Click += delegate
                {
                    DisplayInformation(button.Tag as ManifestChunk);
                };
                PathViewer.Children.Add(button);
            }
        }

        private void AddToFileMap(ManifestChunk chunk)
        {
            double size = DataSource.Length;
            FileMap.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = new GridLength(chunk.Size / size, GridUnitType.Star)
            });
            var border = new Border()
            {
                Background = new SolidColorBrush(FileMapColors[FileMap.Children.Count]),
                Tag = chunk
            };
            Grid.SetColumn(border, FileMap.Children.Count);
            border.MouseLeftButtonDown += delegate
            {
                DisplayInformation(border.Tag as ManifestChunk);
            };
            FileMapBorders.Add(chunk, border);
            FileMap.Children.Add(border);
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshOpenedFile(ManifestBox.Text, DataBox.Text);            
        }

        private void ExtractButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingCover.Visibility = Visibility.Visible; //open loading cover
            tgaConvert = TGACheckBox.IsChecked.Value;
            try
            {
                ExtractAll().ContinueWith(delegate
                {
                    Dispatcher.Invoke(() => LoadingCover.Visibility = Visibility.Collapsed); //close loading cover
                });
            }
            catch(Exception ex)
            {
                Dispatcher.Invoke(() => MessageBox.Show("An error has occured: " + ex));
            }
        }

        private async Task ExtractAll()
        {           
            await Task.Run(delegate
            {
                foreach (var chunk in Parser.Chunks)
                {
                    string path = System.IO.Path.Combine(Destination, chunk.Name);
                    if (File.Exists(path))
                        continue;
                    Dispatcher.Invoke(delegate
                    {
                        LoadingBody.Text = path;
                        LoadingProgress.Maximum = Parser.Chunks.Count;
                        LoadingProgress.Value++;
                    });
                    Parser.ExtractFile(Destination, chunk);
                    if (tgaConvert == true)
                    {
                        if (System.IO.Path.GetExtension(chunk.Name) == ".tga")
                        {
                            using (var targa = TargaImage.LoadTargaImage(path))
                            {
                                using (var fileStream = File.OpenWrite(
                                    System.IO.Path.Combine(Destination,
                                    chunk.Name.Remove(chunk.Name.Length - 4, 4) + ".png")))
                                {
                                    targa.Save(fileStream, ImageFormat.Png);
                                }
                            }
                            File.Delete(path);
                        }
                    }
                }
            });
        }

        private void ExtractFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenChunk == null) return;
            Parser.ExtractFile(DestinationBox.Text, OpenChunk);
            DisplayInformation(OpenChunk);
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            HexEditor.CloseProvider();  
            Process.Start(System.IO.Path.Combine(DestinationBox.Text, OpenChunk.Name));
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ManifestBox.Text = Properties.Settings.Default.ManifestPath;
            DataBox.Text = Properties.Settings.Default.DataPath;
            DestinationBox.Text = Properties.Settings.Default.DestinationDir;
        }

        private void DisplayInformation(ManifestChunk chunk)
        {
            ByteStartOffsetBox.Text = chunk.Offset.ToString();
            ByteEndOffsetBox.Text = chunk.EndOffset.ToString();
            ByteSizeBox.Text = chunk.Size.ToString();
            AnonByteBox.Text = chunk.Extra.ToString();
            NameLabel.Text = chunk.Name;
            ColorDisplay.Background = FileMapBorders[chunk].Background;
            OpenChunk = chunk;
            var path = System.IO.Path.Combine(DestinationBox.Text, chunk.Name);
            if (File.Exists(path)) 
            {
                OpenFileButton.IsEnabled = true;
                HexEditor.FileName = path;
            } 
            else
            {
                OpenFileButton.IsEnabled = false;
                HexEditor.CloseProvider();               
            }
        }

        /// <summary>
        /// WARNING can cause massive changes to AssetDB use carefully!!!
        /// </summary>
        private void AssetDBFill()
        {
            var doc = XDocument.Load(AssetDBEntry.AssetDatabasePath);
            var source = doc.Root.Elements();
            int docIndex = 0;
            for(var index = 0; index < Parser.Chunks.Count; index++)
            {
                var element = source.ElementAt(docIndex);
                var name = element.Element("FilePath");
                var newVal = Parser.Chunks[index].Name;
                if (System.IO.Path.GetExtension(name.Value) == System.IO.Path.GetExtension(newVal))
                {
                    name.Value = newVal;
                    docIndex++;
                }
            }
            doc.Save(AssetDBEntry.AssetDatabasePath + ".new");
        }
    }
}
