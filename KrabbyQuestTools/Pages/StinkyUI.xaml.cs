using KrabbyQuestTools;
using KrabbyQuestTools.Controls;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    public partial class StinkyUI : KQTPage
    {
        private StinkyParser Parser => AppResources.Parser;
        private StinkyLevel OpenLevel;
        private LevelDataBlock OpenDataBlock;
        private BlockLayers _mode;
        private string AssetDir;
        bool askSave = false, decorPrepared = false;
        LevelEditorTile[,] LevelTileMap, ObjTileMap;
        private List<LevelDataBlock> UnsavedChanges = new List<LevelDataBlock>();
        private int _cellSize = 50;
        LevelEditorTile currentlySelected;

        private int CellSize
        {
            get => _cellSize;
            set
            {
                if (_cellSize == value) return;
                _cellSize = value;
                CellSizeBlock.Text = value.ToString();
                if (OpenLevel == null) return;
                SetupGridLength(LevelGrid, OpenLevel, value).ContinueWith(async delegate
                {
                    await SetupGridLength(DecorGrid, OpenLevel, value);
                });
            }
        }

        private BlockLayers Mode
        {
            get => _mode;
            set
            {
                switch (value)
                {
                    case BlockLayers.Decoration:
                        IntegralModeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                        DecorModeButton.Background = Brushes.DarkCyan;                        
                        break;
                    case BlockLayers.Integral:                        
                        DecorModeButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                        IntegralModeButton.Background = Brushes.DarkCyan;
                        break;
                }
                _mode = value;
            }            
        }

        public BlockLayers CurrentLayer => Mode;

        public StinkyUI(StinkyLevel Level, string AssetDirectory)
        {            
            InitializeComponent();
            LoadingPanel.Visibility = Visibility.Hidden;
            AssetDir = AssetDirectory;
            LevelEditorTile.AssetDir = AssetDirectory;
            OpenLevel = Level;
            PrepareMapScreen(Level);           
            Title = Level.Name;
            Mode = BlockLayers.Integral;
            GetMessages();
            GetHeaderData();
            LevelTitleText.Text = Level.Name;
        }

        private void Refresh()
        {
            OpenLevel.Columns = int.Parse(ColumnsSelectionBox.Text);
            OpenLevel.Rows = int.Parse(RowsSelectionBox.Text);
            Parser.BitRead = int.Parse(BitSkipSelection.Text);
            Parser.RefreshLevel(OpenLevel);
            PrepareMapScreen(OpenLevel);
            decorPrepared = false;
            //PrepareMapScreen(OpenLevel, BlockLayers.Decoration);
        }

        private void GetHeaderData()
        {
            int index = 0;
            if (OpenLevel.LevelFileVersion < 5)
            {
                LevelWarningLabel.Text = "LV3 levels not supported.";
                LevelWarningLabel.Background = Brushes.Red;
            }
            else
                LevelWarningLabel.Text = "Viewing Level File Version: " + OpenLevel.LevelFileVersion;
            foreach(var param in typeof(StinkyLevel).GetProperties(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance))
            {
                var att = param.CustomAttributes.FirstOrDefault();
                if (att == default) continue;
                if (att.AttributeType != typeof(StinkyFile.Primitive.EditorVisible)) continue;
                var paramAtt = (param.GetCustomAttributes().ElementAt(0) as StinkyFile.Primitive.EditorVisible);
                if (paramAtt.LevelVersion > OpenLevel.LevelFileVersion) continue;
                var dock = new DockPanel()
                {
                    Margin = new Thickness(0,0,0,5)
                };
                string name = param.Name,
                    value = param.GetValue(OpenLevel)?.ToString() ?? "not found";
                var textBlock = new TextBlock()
                {
                    Text = name,
                    ToolTip = new Border()
                    {
                        Style = null,
                        BorderThickness = new Thickness(1),
                        Padding = new Thickness(10,5,10,5),
                        Background = Brushes.Gray,
                        BorderBrush = Brushes.Silver,
                        Child = new TextBlock()
                        {
                            Text = paramAtt.Description
                        }
                    }
                };
                var textbox = new TextBox()
                {
                    Text = value,
                    IsReadOnly = true,
                    Margin = new Thickness(10,0,0,0),
                    Height = 18
                };
                DockPanel.SetDock(textbox, Dock.Right);
                dock.Children.Add(textBlock);
                dock.Children.Add(textbox);
                HeaderData.Children.Add(dock);
                index++;
            }
        }

        private void GetMessages()
        {
            int index = 0;
            foreach(var message in OpenLevel.Messages)
            {
                if (string.IsNullOrWhiteSpace(message)) { index++; continue; } // dont display intentionally null messages
                var name = "Message #" + (index + 1);
                var block = LevelDataBlock.LoadFromDatabase(name);
                var button = new Button()
                {
                    Content = name,
                    Padding = new Thickness(10,5,10,5),
                    Margin = new Thickness(0,0,0,5),
                    Tag = index
                };
                if (block != null)
                {
                    button.Click += delegate
                    {
                        UpdateToolMenu(block);
                    };
                    button.Background = new SolidColorBrush(AppResources.S_ColorConvert(block.Color));
                    button.Content = block.GUID + " " + block.Name;
                }
                MessageButtons.Children.Add(button);
                index++;
            }
        }

        private async Task SetupGridLength(Grid destination, StinkyLevel level, int cellSize)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                int columns = level.Columns;
                int rows = level.Rows;
                LevelGridHost.MaxWidth = columns * CellSize;
                destination.RowDefinitions.Clear();
                destination.ColumnDefinitions.Clear();
                for (int r = 0; r < rows; r++)
                {
                    destination.RowDefinitions.Add(new RowDefinition()
                    {
                        Height = new GridLength(cellSize, GridUnitType.Pixel),
                    });
                    for (int c = 0; c < columns; c++)
                    {
                        destination.ColumnDefinitions.Add(new ColumnDefinition()
                        {
                            Width = new GridLength(cellSize, GridUnitType.Pixel),
                        });
                    }
                }
            });
        }

        private void PrepareMapScreen(StinkyLevel level, BlockLayers Viewing = BlockLayers.Integral)
        {  
            int columns = level.Columns;
            int rows = level.Rows;              
                                         
            RowsSelectionBox.Text = rows.ToString();
            ColumnsSelectionBox.Text = columns.ToString();
            TotalCellsDisplay.Text = level.Total.ToString();
            //BitSkipSelection.Text = parser.BitRead.ToString();
            var destination = Viewing == BlockLayers.Integral ? LevelGrid : DecorGrid;
            _ = SetupGridLength(destination, level, CellSize);
            _ = LoadMapAsync(destination, level, Viewing);
            PrepareToDoList();
        }

        private async Task LoadMapAsync(Panel destination, StinkyLevel level, BlockLayers Viewing)
        {
            int columns = level.Columns;
            int rows = level.Rows;    
            destination.Children.Clear();  
            if (Viewing == BlockLayers.Integral)
                LevelTileMap = new LevelEditorTile[columns, rows];      
            else ObjTileMap = new LevelEditorTile[columns, rows]; 
            var tileMap = Viewing == BlockLayers.Integral ? LevelTileMap : ObjTileMap;
            LoadingPanel.Visibility = Visibility.Visible;
            LoadingBar.Maximum = level.Total;
            LoadingBar.Value = 0;
            await Task.Run(async delegate
            {
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        var data = ((Viewing == BlockLayers.Integral) ? level.IntegralData : level.DecorationData)[r * columns + c];
                        if (data != null)
                        {
                            int row = r;
                            int column = c;
                            await Dispatcher.InvokeAsync(delegate
                            {        
                                LoadingBar.Value++;
                                var cell = new LevelEditorTile(OpenLevel.Context, data)
                                {
                                    BorderThickness = new Thickness(2, 2, 2, 2)
                                };
                                cell.MouseLeftButtonDown += LevelBlock_Click;
                                cell.Tag = data;
                                Grid.SetRow(cell, row);
                                Grid.SetColumn(cell, column);
                                destination.Children.Add(cell);
                                cell.MouseEnter += delegate
                                {
                                    UpdateHoverText(cell);
                                };
                                lock (tileMap)
                                {
                                    tileMap[column, row] = cell;
                                }                                
                            }, DispatcherPriority.Normal);
                        }
                    }
                }
                for (int r = 0; r < OpenLevel.Rows; r++)
                {
                    for (int c = 0; c < OpenLevel.Columns; c++)
                    {
                        int row = r;
                        int column = c;
                        _ = Dispatcher.InvokeAsync(() => tileMap[column, row].SetupBorder(tileMap, new Point(column, row)));
                    }
                }
            });
            LoadingPanel.Visibility = Visibility.Hidden;
        }

        private void UpdateHoverText(LevelEditorTile cell, bool forceSelected = false)
        {
            var data = cell.BlockData;
            HoverTooltipText.Inlines.Clear();
            if (cell.Selected || forceSelected)
                HoverTooltipText.Inlines.Add("*");
            HoverTooltipText.Inlines.Add(new Run(data.GUID) { FontStyle = FontStyles.Italic });
            HoverTooltipText.Inlines.Add(" " + data.Name);
        }

        private void PrepareToDoList()
        {
            ToDoListContent.Children.Clear();
            foreach(var block in Parser.UnknownBlocks.Values)
            {
                var border = new Button()
                {
                    Height = 20,
                    Margin = new Thickness(0,5,0,5),
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderBrush = Brushes.White,
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
            UpdateHoverText(sender as LevelEditorTile, true);
            UpdateToolMenu((sender as FrameworkElement).Tag);
            if (sender is LevelEditorTile)
                currentlySelected = sender as LevelEditorTile;
        }

        /// <summary>
        /// Signals the tool menu to update its display based on what was selected by the user
        /// </summary>
        /// <param name="SelectedItem"></param>
        private void UpdateToolMenu(object SelectedItem)
        {
            if (SelectedItem is LevelDataBlock && (OpenDataBlock?.GUID ?? null) != ((SelectedItem as LevelDataBlock)?.GUID ?? null))
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
                for (int i = 0; i < LevelDataBlock.RAW_DATA_SIZE; i++)
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
                if (string.IsNullOrWhiteSpace(NameSelectionField.Text))
                    NameSelectionField.Text = "Untitled object";
                BlockLayerDisplay.Text = Enum.GetName(typeof(BlockLayers), dataBlock.BlockLayer) + " - " + dataBlock.GUID;
                BlockColorPicker.SelectedColor = AppResources.S_ColorConvert(dataBlock.Color);
                ContextWarningLabel.Text = "Using Context: " + Enum.GetName(typeof(LevelContext), OpenLevel.Context);
                var textureRef = dataBlock.GetEditorPreview(OpenLevel.Context);
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
                ParameterMenu.Load(dataBlock);
                RotationField.SelectedIndex = Array.IndexOf(Enum.GetNames(typeof(SRotation)), Enum.GetName(typeof(SRotation), dataBlock.Rotation));
                OpenDataBlock = dataBlock;
                BlockSaveButton.IsEnabled = true;
                if (OpenDataBlock.HasMessageContent)
                    MessageTextEditor.Text = OpenDataBlock.GetMessageContent(OpenLevel);
                //refresh updated state
                LevelEditorTile.SetSelectedGUID(dataBlock.GUID);
            }
            else LevelEditorTile.SetSelectedGUID(null);
            //undim the map
            var tileMap = CurrentLayer == BlockLayers.Integral ? LevelTileMap : ObjTileMap;     
            for (int r = 0; r < OpenLevel.Rows; r++)
                for (int c = 0; c < OpenLevel.Columns; c++)
                    tileMap[c, r].UpdateSelectedState();
        }        

        private void RefreshEditorButton_Click(object sender, RoutedEventArgs e) => Refresh();

        private void ColumnsSelectionBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)            
                RowsSelectionBox.Text = (OpenLevel.Total / int.Parse(ColumnsSelectionBox.Text)).ToString();            
        }

        private void RowsSelectionBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)            
                ColumnsSelectionBox.Text = (OpenLevel.Total / int.Parse(RowsSelectionBox.Text)).ToString();            
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
            OpenLevel.SaveAll();
            UnsavedChanges = new List<LevelDataBlock>();
            askSave = false;
            Refresh();
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
                //PrepareMapScreen(OpenLevel);
                Mode = BlockLayers.Integral;
                DecorGrid.Visibility = Visibility.Hidden;
                LevelGrid.Opacity = 1;
            }
        }

        private void DecorModeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode != BlockLayers.Decoration)
            {
                if (!decorPrepared)
                {
                    PrepareMapScreen(OpenLevel, BlockLayers.Decoration);
                    decorPrepared = true;
                }
                Mode = BlockLayers.Decoration;
                DecorGrid.Visibility = Visibility.Visible;
                LevelGrid.Opacity = .15;
            }
        }

        public override void OnActivated()
        {
            UpdateToolMenu(OpenDataBlock);
            base.OnActivated();
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
                var paramDialog = new KQTDialog()
                {
                    Content = new ParameterDialog(OpenDataBlock),
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF2C2C2C"))
                };
                paramDialog.Show();
            }
        }

        private void FitButton_Click(object sender, RoutedEventArgs e)
        {
            var usableSpace = ContentGrid.ActualHeight - 30;
            var heightWise = (int)(usableSpace / OpenLevel.Rows);
            usableSpace = ContentGrid.ActualWidth - 10;
            var widthWise = (int)(usableSpace / OpenLevel.Columns);
            CellSize = Math.Min(heightWise, widthWise);
        }

        private void GridBorder_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(sender as Panel);
            if (point.X + HoverTooltipText.ActualWidth < LevelGridHost.ActualWidth)
                HoverTooltipText.Margin = new Thickness(point.X + 10, point.Y + 10,0,0);
            else 
                HoverTooltipText.Margin = new Thickness(
                    point.X - 10 - HoverTooltipText.ActualWidth,
                    point.Y + 10,0,0);
        }

        private void GridBorder_MouseEnter(object sender, MouseEventArgs e)
        {            
            HoverTooltipText.Visibility = Visibility.Visible;
        }

        private void GridBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            HoverTooltipText.Visibility = Visibility.Hidden;
        }

        private void CellSizeBlock_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(CellSizeBlock.Text, out int cellSize))
                CellSize = cellSize;
        }

        public override bool OnClosing()
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
