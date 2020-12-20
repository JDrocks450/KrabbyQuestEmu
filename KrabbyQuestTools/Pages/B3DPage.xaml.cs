using LiveCharts;
using LiveCharts.Wpf;
using StinkyFile.Blitz3D;
using StinkyFile.Blitz3D.Prim;
using StinkyFile.Blitz3D.Visualizer;
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
    /// Interaction logic for TreeVisualizer.xaml
    /// </summary>
    public partial class TreeVisualizer : Page
    {
        public enum ChartData
        {
            None, Pos, Rot, Scl
        }
        ChartData AutoChart = ChartData.Pos;

        BlitzTreeVisualizer TreeHost;
        Color color1 = Colors.LightSkyBlue, color2 = Colors.DarkBlue;
        int maxColorDepth = 10;
        int marginTop = 20, marginLeft = 50;

        Anim selectedAnim;
        BlitzObject selectedObject;
        string Workspace;

        public TreeVisualizer(string Workspace)
        {
            InitializeComponent();            
            DataChart.Visibility = Visibility.Collapsed;
            DataGrid.Visibility = Visibility.Collapsed;
            this.Workspace = Workspace;
            Load(System.IO.Path.Combine(Workspace, "Graphics", "jellyfish01.b3d"));
        }        

        private void Load(string fileName)
        {
            PathBlock.Text = fileName;
            var obj = StinkyFile.Blitz3D.B3D.B3D_Loader.Load(fileName);
            TreeHost = new BlitzTreeVisualizer(obj.LoadedObjects, obj.RootObject);
            LoadedObjectsText.Text = "\nLoaded Objects: " + obj.LoadedObjects.Count;
            DrawTree();
        }

        private void DrawTree()
        {
            TreeViewerCanvas.Children.Clear();
            lastYCoordinate = marginTop;
            mostXCoordinate = 0;
            DrawTreeItem(TreeHost.Tree.First().Value);
        }

        private Color GetColor(float weight)
        {
            Color startColor = color1;
            Color endColor = color2;

            if (weight > 1)
                weight = 1;

            return Color.FromRgb(
                (byte)Math.Round(startColor.R * (1 - weight) + endColor.R * weight),
                (byte)Math.Round(startColor.G * (1 - weight) + endColor.G * weight),
                (byte)Math.Round(startColor.B * (1 - weight) + endColor.B * weight));
        }

        static int lastYCoordinate = 0;
        static int mostXCoordinate = 0;

        private Border DrawTreeItem(BlitzTreeVisualizer.BlitzTreeItem Item, int depth = 1)
        {
            //get tree gradient color
            var percent = depth / (float)maxColorDepth;
            Color newColor = GetColor(percent);
            var grid = new StackPanel();
            //title
            var title = new TextBlock()
            {
                Text = BlitzTreeVisualizer.BlitzTreeItem.DisplayTypeName(Item.Focus) + " " + Item.Focus.Name,
                FontWeight = FontWeights.Bold,
                FontSize = 14.0,
                Margin = new Thickness(0, 0, 0, 10)
            };
            grid.Children.Add(title);

            //show properties
            if (!(Item.Focus is StinkyFile.Blitz3D.Prim.FileRoot))
                foreach (var prop in Item.GetPropertyValues())
                {
                    grid.Children.Add(new TextBlock()
                    {
                        Text = prop.Key + " " + prop.Value,
                        Margin = new Thickness(5, 0, 0, 0)
                    });
                }
            var brush = new SolidColorBrush(newColor);
            var border = new Border()
            {
                Background = brush,
                Padding = new Thickness(10, 10, 10, 10),
                Child = grid,
                Tag = Item.Focus
            };

            TreeViewerCanvas.Children.Add(border);

            int thisX = depth * marginLeft;
            Canvas.SetLeft(border, thisX);
            int thisY = lastYCoordinate;
            Canvas.SetTop(border, thisY);

            border.Measure(new Size(9999, 9999));

            lastYCoordinate += marginTop + (int)border.DesiredSize.Height;
            TreeViewerCanvas.Height = lastYCoordinate + marginTop;
            var x = (depth * marginLeft) + (int)border.DesiredSize.Width + marginLeft;
            if (x > mostXCoordinate)
                mostXCoordinate = x;
            TreeViewerCanvas.Width = mostXCoordinate;
            
            List<Border> borders = new List<Border>();
            foreach (var child in Item.Children)
                borders.Add(DrawTreeItem(TreeHost.Tree[child], depth + 1));
            Border last = borders.LastOrDefault();

            foreach(var b in borders)
            {
                double y = Canvas.GetTop(b) + b.DesiredSize.Height / 2;
                TreeViewerCanvas.Children.Add(new Line()
                {
                    X1 = thisX + 10,
                    X2 = Canvas.GetLeft(b) - 10,
                    Y1 = y,
                    Y2 = y,
                    Stroke = brush
                });
            }

            if (last != null)
                TreeViewerCanvas.Children.Add(new Line()
                {
                    X1 = thisX + 10,
                    X2 = thisX + 10,
                    Y1 = thisY + border.DesiredSize.Height,
                    Y2 = Canvas.GetTop(last) + last.DesiredSize.Height / 2,
                    Stroke = brush
                });

            if (Item.HasAnimator)
            {
                border.Cursor = Cursors.Hand;
                border.MouseLeftButtonDown += TreeItemPicked;
            }

            return border;
        }

        private void TreeItemPicked(object sender, MouseButtonEventArgs e)
        {
            AnimationStack.Children.Clear();
            var blitzObject = selectedObject = (sender as Border).Tag as BlitzObject;
            int index = 0;
            foreach(var anim in blitzObject.Animator.Animations)
            {
                bool nullAnim = anim?.keys[0]?.AnimRep?.pos_anim == null;
                var button = new Button()
                {
                    Content = anim == null ? "Not found" : (nullAnim ? "Error: " : "") + selectedObject.Animator.Objects.ElementAtOrDefault(index)?.Name ?? "Unknown Bone",
                    Width=150,
                    Height=50,
                    Margin=new Thickness(10),
                    Background = !nullAnim ? Brushes.DarkCyan : Brushes.Gray,
                    BorderBrush = Brushes.White,
                    IsEnabled = !nullAnim,
                    Tag = anim
                };
                button.Click += AnimationPicked;
                AnimationStack.Children.Add(button);
                index++;
            }
        }        

        private void AnimationPicked(object sender, RoutedEventArgs e)
        {           
            Anim selected = (sender as Button).Tag as Anim;
            selectedAnim = selected;                     
            DataGrid.Visibility = Visibility.Visible;
            //DataGrid.Height = (AnimationDockPanel.ActualHeight) / 3;
            ObjectSwitcher.Items.Clear();
            for (int i = 0; i < selectedAnim.keys.Count; i++)
                ObjectSwitcher.Items.Add(i);
            ObjectSwitcher.SelectedIndex = 0;
            LoadAnim(0);
        }

        private void LoadAnim(int key = 0)
        {
            var selected = selectedAnim;
            int selectedObjectKey = key;
            StringBuilder posBuilder = new StringBuilder(),
                rotBuilder = new StringBuilder(),
                sclBuilder = new StringBuilder();            
            var anim = selected.keys.ElementAtOrDefault(selectedObjectKey);
            if (anim == null)
            {
                //MessageBox.Show("Animation not found!");
                return;
            }
            posBuilder.AppendLine(string.Join("\n", anim.AnimRep.pos_anim));
            sclBuilder.AppendLine(string.Join("\n", anim.AnimRep.scale_anim));
            rotBuilder.AppendLine(string.Join("\n", anim.AnimRep.rot_anim));               
            PositionBlock.Text = posBuilder.ToString();
            RotationBlock.Text = rotBuilder.ToString();
            ScaleBlock.Text = sclBuilder.ToString();
            switch (AutoChart)
            {
                case ChartData.Pos:
                    PopulatePosition();
                    break;
                case ChartData.Rot:
                    PopulateRotation();
                    break;
            }
        }

        private void ChartPos_Click(object sender, RoutedEventArgs e)
        {
            AutoChart = ChartData.Pos;
            PopulatePosition();    
        }

        private void OpenNewButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Workspace;
            if (Common.FileBrowser.BrowseForFile(ref path, "Open *.B3D File") == System.Windows.Forms.DialogResult.Cancel) return;
            Load(path);
        }

        private void ObjectSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadAnim(ObjectSwitcher.SelectedIndex);
        }

        private void PopulatePosition()
        {            
            PopulateChart("Time", "Position",
                ("X", selectedAnim.keys[0].AnimRep.pos_anim.Select(x => x.Value.V.X)),
                ("Y", selectedAnim.keys[0].AnimRep.pos_anim.Select(x => x.Value.V.Y)),
                ("Z", selectedAnim.keys[0].AnimRep.pos_anim.Select(x => x.Value.V.Z)));
        }

        private void ChartRot_Click(object sender, RoutedEventArgs e)
        {
            AutoChart = ChartData.Rot;
            PopulateRotation(); 
        }

        private void PopulateRotation()
        {
            PopulateChart("Time", "Rotation",
                ("W", selectedAnim.keys[0].AnimRep.rot_anim.Select(x=> x.Value.W)),
                ("X", selectedAnim.keys[0].AnimRep.rot_anim.Select(x => x.Value.V.X)),
                ("Y", selectedAnim.keys[0].AnimRep.rot_anim.Select(x => x.Value.V.Y)),
                ("Z", selectedAnim.keys[0].AnimRep.rot_anim.Select(x => x.Value.V.Z)));
        }

        private void PopulateChart(string AxisXTitle, string AxisYTitle, params (string title, IEnumerable<float> series)[] data)
        {
            DataChart.Visibility = Visibility.Visible;
            DataChart.Height = (AnimationDockPanel.ActualHeight) / 3;
            var axisX = DataChart.AxisX.First();
            axisX.Title = AxisXTitle;
            var axisY = DataChart.AxisY.First();
            axisY.Title = AxisYTitle;
            DataChart.Series.Clear();
            foreach (var tuple in data) {
                var creatingSeries = new ChartValues<float>();
                creatingSeries.AddRange(tuple.series);
                DataChart.Series.Add(new LineSeries
                {
                    Title = tuple.title,
                    Values = creatingSeries
                });
            }
        }
    }
}
