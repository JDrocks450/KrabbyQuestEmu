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
using System.Xml.Linq;

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for MapScreenCustomizer.xaml
    /// </summary>
    public partial class MapScreenCustomizer : Page
    {
        private readonly string workspaceDir;
        public static string DBPath => MapWaypointParser.DBPath;
        MapWaypointParser mapParser;
        enum Tool
        {      
            NONE,
            PLACE,
            DELETE,
            MOVE
        }
        Tool Current {
            get
            {
                return _tool;
            }
            set
            {
                _tool = value;
                var color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                PlaceButton.Background = DeleteButton.Background = MoveButton.Background = color;
                switch (value)
                {
                    case Tool.MOVE:
                        MoveButton.Background = Brushes.DeepSkyBlue;
                        break;
                    case Tool.PLACE:
                        PlaceButton.Background = getColor(placing.Context);
                        break;
                    case Tool.DELETE:
                        DeleteButton.Background = Brushes.Red;
                        break;
                }
            }
        }
        Button currentlySelected;
        Border placingMarker;
        StinkyLevel placing;
        bool isMouseOver = false, levelsRefreshed = false;
        SolidColorBrush[] mapColors =
        {
            Brushes.Black,
            Brushes.Black,
            Brushes.Orange,
            Brushes.Red,
            Brushes.Pink,
            Brushes.Green,
            Brushes.Turquoise
        };
        Point placingPosition;
        private Tool _tool = Tool.NONE;

        Dictionary<StinkyLevel, SPoint> levels => mapParser.Levels;

        public MapScreenCustomizer(string WorkspaceDir)
        {
            InitializeComponent();
            if (AppResources.Parser == null)
                AppResources.Parser = new StinkyFile.StinkyParser();
            workspaceDir = WorkspaceDir;
            mapParser = new MapWaypointParser(WorkspaceDir, AppResources.Parser);
            DrawMap();
            GetLevels();            
        }
        
        (StinkyLevel level, Point position) Load(XElement element)
        {
            var info = mapParser.Load(element);
            return (info.level, new Point(info.position.X, info.position.Y));
        }

        void DrawMap()
        {
            for(int i = 0; i < 4; i++)
            {
                Image target = default;
                switch (i)
                {
                    case 0: target = MapTopLeft; break;
                    case 1: target = MapTopRight; break;
                    case 2: target = MapBotLeft; break;
                    case 3: target = MapBotRight; break;
                }
                BitmapSource source = new BitmapImage(new Uri(System.IO.Path.Combine(workspaceDir, "Graphics", $"map{i + 1}.bmp")));
                target.Source = source;
            }
            RefreshMarkers();
        }

        void RefreshMarkers()
        {
            var removeUs = PlaceGrid.Children.OfType<Border>().ToList();
            foreach (var border in removeUs)
                PlaceGrid.Children.Remove(border);
            var removeLines = PlaceGrid.Children.OfType<Line>().ToList();
            foreach (var border in removeLines)
                PlaceGrid.Children.Remove(border);            
            Point lastWaypointLoc = default;
            string lastWorldName = default;
            List<Border> waypoints = new List<Border>();
            List<Line> lines = new List<Line>();
            mapParser.LoadAll();
            foreach (var pair in levels)
            {
                (StinkyLevel level, Point position) info = (pair.Key, new Point(pair.Value.X, pair.Value.Y));
                var waypoint = getMapWaypoint(info.position);
                var color = getColor(info.level.Context);
                waypoint.Background = color;
                waypoint.Child = new TextBlock()
                {
                    Text = info.level.LevelWorldName.Replace(".lv5", ""),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                waypoints.Add(waypoint);
                if (lastWaypointLoc != default)
                {
                    var line = new Line()
                    {
                        StrokeThickness = 5.0,
                        Stroke = color
                    };
                    line.X1 = info.position.X + 15;
                    line.X2 = lastWaypointLoc.X + 15;
                    line.Y1 = info.position.Y + 15;
                    line.Y2 = lastWaypointLoc.Y + 15;
                    Grid.SetColumnSpan(line, 2);
                    Grid.SetRowSpan(line, 2);
                    lines.Add(line);
                    applyTooltip(line, $"{lastWorldName} -> {info.level.Name}", "Transition");
                }
                applyTooltip(waypoint, info.level.Name, "Level Waypoint");
                waypoint.Tag = info.level;
                waypoint.MouseLeftButtonDown += Waypoint_MouseLeftButtonDown;
                lastWaypointLoc = info.position;
                lastWorldName = info.level.Name;
            }
            foreach (var line in lines)
                PlaceGrid.Children.Add(line);
            foreach (var marker in waypoints)
                PlaceGrid.Children.Add(marker);            
        }

        private void Waypoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var level = (sender as Border).Tag as StinkyLevel;
            if (Current == Tool.DELETE)
            {
                Remove(level);
                RefreshMarkers();
                GetLevels();
                SwitchLevelPlacement(null);
            }
            else if (Current == Tool.MOVE)
            {
                MoveWaypoint(sender as Border, level);
                (sender as Border).MouseLeftButtonDown -= Waypoint_MouseLeftButtonDown;
            }
        }

        Border getMapWaypoint(Point position)
        {
            var border = getMapWaypoint();
            border.Margin = new Thickness(position.X, position.Y, 0, 0);
            return border;
        }

        void applyTooltip(FrameworkElement element, string Title, string Content)
        {
            var stack = new StackPanel()
            {
                Margin = new Thickness(10,10,10,10)
            };
            stack.Children.Add(new TextBlock()
            {
                Text = Title,
                FontWeight = FontWeights.Bold,                
            });
            stack.Children.Add(new TextBlock()
            {
                Text = Content,
                Margin = new Thickness(0, 5, 0, 0)
            });
            element.ToolTip = new ToolTip()
            {
                Content = stack,
                Background = Brushes.Black
            };
        }

        Border getMapWaypoint()
        {
            var placingMarker = new Border()
            {
                Width = 30,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,                   
            };
            Grid.SetColumnSpan(placingMarker, 2);
            Grid.SetRowSpan(placingMarker, 2);
            return placingMarker;
        }

        Brush getColor(LevelContext context) => mapColors[(int)context];        

        void GetLevels()
        {
            LevelStack.Children.Clear();
            var levelDir = System.IO.Path.Combine(workspaceDir, "levels");            
            if (!levelsRefreshed)
            {
                AppResources.Parser.FindAllLevels(levelDir);
                levelsRefreshed = true;
            }
            foreach(var level in AppResources.Parser.LevelInfo)
            {
                bool isPlaced = mapParser.LevelMarkerPlaced(level);
                var button = new Button()
                {
                    Content = $"[{level.LevelWorldName}]: {level.Name}" + (isPlaced ? " - Placed" : ""),                    
                    Margin = new Thickness(10, 5, 10, 5),
                    Padding = new Thickness(5,10,5,10),
                    Tag = level
                };
                if (isPlaced)
                    button.Background = getColor(level.Context);
                button.Click += delegate {
                    if (isPlaced) return; //TODO: Enter Movement mode
                    SwitchLevelPlacement(level);
                    currentlySelected = button;
                    currentlySelected.Background = getColor(level.Context); 
                };
                LevelStack.Children.Add(button);
            }
        }

        private void SwitchLevelPlacement(StinkyLevel level)
        {
            placing = level;                        
            if (level == null)
            {
                if (currentlySelected != null)
                    currentlySelected.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"));
                Current = Tool.NONE;
                return;
            }
            Current = Tool.PLACE;
            if (placingMarker == null)
            {
                placingMarker = getMapWaypoint();
                PlaceGrid.Children.Add(placingMarker);
                placingMarker.MouseLeftButtonDown += PlacingMarker_MouseLeftButtonDown;
            }   
            placingMarker.Background = getColor(level.Context); 
        }

        private void MoveWaypoint(Border waypoint, StinkyLevel level)
        {
            placing = level;
            Current = Tool.PLACE;            
            if (level == null)
            {
                Current = Tool.NONE;
                return;
            }
            placingMarker = waypoint;
            placingMarker.MouseLeftButtonDown += PlacingMarker_MouseLeftButtonDown;
        }

        private void PlacingMarker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            placingMarker = null;
            Save(placing, placingPosition);
            RefreshMarkers();
            GetLevels();
            SwitchLevelPlacement(null);
        }

        void Save(StinkyLevel level, Point location) => mapParser.Save(level, new SPoint(location.X, location.Y));
        void Remove(StinkyLevel level) => mapParser.Remove(level);

        private void MapBotRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (placingMarker != null && Current == Tool.PLACE && isMouseOver)
            {
                placingMarker.Visibility = Visibility.Visible;
                var pos = e.GetPosition(PlaceGrid);
                placingPosition = new Point(pos.X - 15, pos.Y - 15);
                placingMarker.Margin = new Thickness(pos.X - 15, pos.Y - 15,0,0);                
            }
        }

        private void MapBotRight_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOver = true;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current != Tool.DELETE)
                Current = Tool.DELETE;
            else Current = Tool.NONE;
        }

        private void PlaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current == Tool.PLACE)
            {
                Current = Tool.NONE;
                placingMarker.Visibility = Visibility.Hidden;
            }
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (Current != Tool.MOVE)
                Current = Tool.MOVE;
            else
            {
                Current = Tool.NONE;
            }
        }

        private void MapBotRight_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOver = false;
        }
    }
}
