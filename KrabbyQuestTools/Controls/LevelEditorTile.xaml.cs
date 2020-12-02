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

namespace KrabbyQuestTools.Controls
{
    /// <summary>
    /// Interaction logic for LevelEditorTile.xaml
    /// </summary>
    public partial class LevelEditorTile : UserControl
    {
        private static string _sGUID;
        private static BitmapSource EditorImage;
        public static string AssetDir { get; set; }

        public LevelDataBlock BlockData
        {
            get; private set;
        }
        public string GUID => BlockData.GUID;

        public static string SelectedGUID
        {
            get => _sGUID;
            set
            {
                _sGUID = value;                
            }
        }

        public bool Selected => GUID == SelectedGUID;
        public bool ShouldDim => SelectedGUID != null;
        public new Thickness BorderThickness { get; set; }

        public static void SetSelectedGUID(string GUID)
        {
            SelectedGUID = GUID;
        }

        public LevelEditorTile()
        {
            InitializeComponent();
            EditorSelectionImage.Source = BitmapSourceProvider.Load(new Uri("Resources/Editor/selection.png", UriKind.Relative));
        }

        public LevelEditorTile(StinkyLevel level, LevelDataBlock block) : this()
        {
            Reflect(level, block);
        }

        public void Reflect(StinkyLevel level, LevelDataBlock block)
        {
            BlockData = block;
            HostGrid.Background = new SolidColorBrush(AppResources.S_ColorConvert(block.Color));
            UpdateSelectedState();
            UpdateImage(level);
        }

        public Task SetupBorder(LevelEditorTile[,] Map, Point MapLocation)
        {
            return Task.Run(delegate
            {
                bool bLeft = true, bRight = true, bTop = true, bBottom = true;
                if (MapLocation.X > Map.GetLowerBound(0))
                    bLeft = Map[(int)MapLocation.X - 1, (int)MapLocation.Y].GUID != GUID;
                if (MapLocation.X < Map.GetUpperBound(0))
                    bRight = Map[(int)MapLocation.X + 1, (int)MapLocation.Y].GUID != GUID;
                if (MapLocation.Y > Map.GetLowerBound(1))
                    bTop = Map[(int)MapLocation.X, (int)MapLocation.Y - 1].GUID != GUID;
                if (MapLocation.Y < Map.GetUpperBound(1))
                    bBottom = Map[(int)MapLocation.X, (int)MapLocation.Y + 1].GUID != GUID;
                Dispatcher.InvokeAsync(delegate
                {
                    SelectionBorder.BorderThickness = new Thickness(
                                bLeft ? BorderThickness.Left : 0,
                                bTop ? BorderThickness.Top : 0,
                                bRight ? BorderThickness.Right : 0,
                                bBottom ? BorderThickness.Bottom : 0);
                });
            });
        }

        public void UpdateImage(StinkyLevel level)
        {
            var data = BlockData;
            var texture = data.GetEditorPreview(level.Context);
            if (texture != null)
            {
                RotateTransform transform = new RotateTransform(0);
                if (data.Rotation == SRotation.NORTH) // North
                {

                }
                else if (data.Rotation == SRotation.SOUTH) //south
                {
                    transform = new RotateTransform(180);
                }
                else if (data.Rotation == SRotation.EAST) //east
                {
                    transform = new RotateTransform(90);
                }
                else // west 
                {
                    transform = new RotateTransform(-90);
                }
                try
                {
                    var image = BitmapSourceProvider.Load(new Uri(System.IO.Path.Combine(AssetDir, texture.FileName)));
                    EditorPreview.Source = image;
                    EditorPreview.RenderTransform = transform;
                    EditorPreview.RenderTransformOrigin = new Point(.5, .5);                    
                }
                catch (Exception e)
                {

                }
            }
        }

        public void UpdateSelectedState()
        {
            if (Selected)
            {
                SelectionBorder.Visibility = Visibility.Visible;
                DimBorder.Visibility = Visibility.Hidden;
            }
            else
            {
                SelectionBorder.Visibility = Visibility.Hidden;
                if (ShouldDim)
                    DimBorder.Visibility = Visibility.Visible;
                else DimBorder.Visibility = Visibility.Hidden;
            }
        }
    }
}
