using KrabbyQuestTools;
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
            WindowTitle = "Krabby Quest Level Viewer - " + Parser.OpenLevel.Name;
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
                    MinHeight = 20
                });
                for (int c = 0; c < columns; c++)
                {
                    destination.ColumnDefinitions.Add(new ColumnDefinition()
                    {
                        Width = new GridLength(1, GridUnitType.Star),
                        MinWidth = 20
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
                            /* cell.Child = new Image()
                            {
                                Source = new BitmapImage(new Uri(texture.FileName)),
                                RenderTransform = transform,
                                RenderTransformOrigin = new Point(.5,.5)
                            }; */
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
                var dataBlock = SelectedItem as LevelDataBlock;
                dataBlock.RefreshFromDatabase();
                RawDataField.Children.Clear();
                for(int i = 0; i < LevelDataBlock.RAW_DATA_SIZE; i++)
                {
                    RawDataField.Children.Add(new TextBox()
                    {
                        Text = dataBlock.RawData[i].ToString(),
                        IsEnabled = false,
                        Margin = new Thickness(0, 0, 5, 0),
                        Width = 31
                    });
                }
                NameSelectionField.Text = dataBlock.Name;
                BlockLayerDisplay.Text = Enum.GetName(typeof(BlockLayers), dataBlock.BlockLayer);
                BlockColorPicker.SelectedColor = AppResources.S_ColorConvert(dataBlock.Color);
                OpenDataBlock = dataBlock;
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
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Parser.OpenLevel.SaveAll();
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
    }
}
