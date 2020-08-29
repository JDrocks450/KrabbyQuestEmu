using KrabbyQuestTools;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for StinkyUI.xaml
    /// </summary>
    public partial class StinkyUI : Page
    {
        private StinkyParser Parser => AppResources.Parser;
        private LevelDataBlock OpenDataBlock;
        private BlockLayers _mode;
        private string AssetDir = @"D:\Projects\Krabby Quest\FileDump";
        bool askSave = false;

        private BlockLayers Mode
        {
            get => _mode;
            set
            {
                switch (value)
                {
                    case BlockLayers.Decoration:
                        IntegralModeButton.Background = Brushes.Silver;
                        DecorModeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                        break;
                    case BlockLayers.Integral:                        
                        DecorModeButton.Background = Brushes.Silver;
                        IntegralModeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                        break;
                }
                _mode = value;
            }            
        }
        
        public StinkyUI()
        {            
            InitializeComponent();
            PrepareMapScreen(Parser.OpenLevel);           
            Title = Parser.OpenLevel.Name;
        }                

        private void Refresh()
        {
            try
            {
                Parser.OpenLevel.Columns = int.Parse(ColumnsSelectionBox.Text);
                Parser.OpenLevel.Rows = int.Parse(RowsSelectionBox.Text);
                Parser.BitRead = int.Parse(BitSkipSelection.Text);
            }
            catch(Exception)
            {
                //!
            }            
            Parser.Refresh();
            PrepareMapScreen(Parser.OpenLevel);
            PrepareMapScreen(Parser.OpenLevel, BlockLayers.Decoration);
        }

        private void PrepareMapScreen(StinkyLevel parser, BlockLayers Viewing = BlockLayers.Integral)
        {            
            int columns = parser.Columns;
            int rows = parser.Rows;
            RowsSelectionBox.Text = rows.ToString();
            ColumnsSelectionBox.Text = columns.ToString();
            TotalCellsDisplay.Text = parser.Total.ToString();
            //BitSkipSelection.Text = parser.BitRead.ToString();
            var destination = Viewing == BlockLayers.Integral ? LevelGrid : DecorGrid;
            destination.Children.Clear();
            destination.RowDefinitions.Clear();
            destination.ColumnDefinitions.Clear();
            for (int r = 0; r < rows; r++)
            {
                destination.RowDefinitions.Add(new RowDefinition()
                {
                    Height = new GridLength(1, GridUnitType.Star),
                    MinHeight = 25,
                    MaxHeight = 30
                });
                for (int c = 0; c < columns; c++)
                {
                    destination.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                        MinWidth = 25,
                        MaxWidth = 30
                    });
                    var data = ((Viewing == BlockLayers.Integral) ? parser.IntegralData : parser.DecorationData)[r * columns + c];
                    if (data != null)
                    {
                        var cell = new Border()
                        {
                            Background = new SolidColorBrush(AppResources.S_ColorConvert(data.Color))
                        };
                        var texture = data.GetFirstTextureAsset();
                        if (texture != null)
                        {
                            RotateTransform transform = new RotateTransform(0);
                            if (data.Name.Contains("_N")) // North
                            {

                            }
                            else if (data.Name.Contains("_S")) //south
                            {
                                transform = new RotateTransform(180);
                            }
                            else if (data.Name.Contains("_E")) //east
                            {
                                transform = new RotateTransform(90);
                            }
                            else // west 
                            {
                                transform = new RotateTransform(-90);
                            }
                            cell.Child = new Image()
                            {
                                Source = new BitmapImage(new Uri(System.IO.Path.Combine(AssetDir, texture.FileName))),
                                RenderTransform = transform,
                                RenderTransformOrigin = new Point(.5, .5)
                            };
                        }
                        cell.MouseLeftButtonDown += LevelBlock_Click;
                        cell.Tag = data;
                        Grid.SetRow(cell, r);
                        Grid.SetColumn(cell, c);
                        destination.Children.Add(cell);
                    }
                }
            }
            PrepareToDoList();
        }

        private void PrepareToDoList()
        {
            ToDoListContent.Children.Clear();
            foreach(var block in Parser.UnknownBlocks.Values)
            {
                var border = new Border()
                {
                    Height = 20,
                    Margin = new Thickness(0,5,0,5),
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(1, 1, 1, 1),
                    BorderBrush = Brushes.Gray,
                    Background = new SolidColorBrush(AppResources.S_ColorConvert(block.Color)),
                    Tag = block
                };
                border.MouseLeftButtonDown += ToDoListClicked;
                ToDoListContent.Children.Add(border);
            }
        }

        private void ToDoListClicked(object sender, MouseButtonEventArgs e)
        {
            UpdateToolMenu((sender as FrameworkElement).Tag);
        }        

        private void LevelBlock_Click(object sender, MouseButtonEventArgs e)
        {
            UpdateToolMenu((sender as FrameworkElement).Tag);
        }

        /// <summary>
        /// Signals the tool menu to update its display based on what was selected by the user
        /// </summary>
        /// <param name="SelectedItem"></param>
        private void UpdateToolMenu(object SelectedItem)
        {
            if (SelectedItem is LevelDataBlock)
            {
                askSave = true;
                var dataBlock = SelectedItem as LevelDataBlock;                
                dataBlock = dataBlock.RefreshFromDatabase(Parser);
                RawDataField.Children.Clear();
                BlockSaveButton.IsEnabled = false;
                if (dataBlock == null)
                {
                    NameSelectionField.Text = "EMPTY SPACE";
                    BlockSaveButton.IsEnabled = false;
                    return;
                }
                for(int i = 0; i < LevelDataBlock.RAW_DATA_SIZE; i++)
                {
                    var box = new TextBox()
                    {
                        Text = dataBlock.RawData[i].ToString(),
                        IsEnabled = false,
                        Margin = new Thickness(i == 0 ? 0 : 2, 0, i == 3 ? 0 : 2, 0),
                    };
                    RawDataField.Children.Add(box);
                    Grid.SetColumn(box, i);
                }
                NameSelectionField.Text = dataBlock.Name;
                BlockLayerDisplay.Text = Enum.GetName(typeof(BlockLayers), dataBlock.BlockLayer) + " - " + dataBlock.GUID;
                BlockColorPicker.SelectedColor = AppResources.S_ColorConvert(dataBlock.Color);
                var textureRef = dataBlock.GetFirstTextureAsset();
                try
                {
                    TextureSelectionField.Background = new ImageBrush(new BitmapImage(new Uri(System.IO.Path.Combine(AssetDir, textureRef.FileName))));
                    TextureNameBox.Text = textureRef.DBName;
                }
                catch 
                {
                    TextureSelectionField.Background = null;
                    TextureNameBox.Text = "No Texture";
                }
                RotationField.SelectedIndex = Array.IndexOf(Enum.GetNames(typeof(SRotation)), Enum.GetName(typeof(SRotation), dataBlock.Rotation));
                OpenDataBlock = dataBlock;
                BlockSaveButton.IsEnabled = true;                
            }
        }        

        private void RefreshEditorButton_Click(object sender, RoutedEventArgs e) => Refresh();

        private void ColumnsSelectionBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)            
                RowsSelectionBox.Text = (Parser.OpenLevel.Total / int.Parse(ColumnsSelectionBox.Text)).ToString();            
        }

        private void RowsSelectionBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)            
                ColumnsSelectionBox.Text = (Parser.OpenLevel.Total / int.Parse(RowsSelectionBox.Text)).ToString();            
        }

        private void BlockSaveButton_Click(object sender, RoutedEventArgs e)
        {
            OpenDataBlock.Name = NameSelectionField.Text;
            OpenDataBlock.SaveToDatabase();
            Parser.CacheRefresh(OpenDataBlock.GUID);
            askSave = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Parser.OpenLevel.SaveAll();
            askSave = false;
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (OpenDataBlock != null)
                OpenDataBlock.Color = new S_Color(e.NewValue.Value.R, e.NewValue.Value.G, e.NewValue.Value.B);
        }


        private void IntegralModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode != BlockLayers.Integral)
            {
                PrepareMapScreen(Parser.OpenLevel);
                Mode = BlockLayers.Integral;
                DecorGrid.Visibility = Visibility.Hidden;
                LevelGrid.Opacity = 1;
            }
        }

        private void DecorModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode != BlockLayers.Decoration)
            {
                PrepareMapScreen(Parser.OpenLevel, BlockLayers.Decoration);
                Mode = BlockLayers.Decoration;
                DecorGrid.Visibility = Visibility.Visible;
                LevelGrid.Opacity = .15;
            }
        }

        private void RotationField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OpenDataBlock != null)
                OpenDataBlock.Rotation = (SRotation)Enum.Parse(typeof(SRotation), (RotationField.SelectedItem as ComboBoxItem).Content.ToString());
        }

        private void ParameterButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenDataBlock != null)
            {
                var paramDialog = new ParameterDialog(OpenDataBlock);
                paramDialog.ShowDialog();
            }
        }

        public bool OnClosing()
        {
            if (askSave)
            {
                var result = MessageBox.Show("Do you want to save before closing?", "Unsaved Changes!", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                    Parser.CacheSaveAll();
                else if (result == MessageBoxResult.Cancel)
                    return false;
            }
            return true;
        }
    }
}
