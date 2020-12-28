using LiveCharts;
using LiveCharts.Wpf;
using StinkyFile.Blitz3D.Prim;
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
    /// Interaction logic for AnimKeyFrameGraph.xaml
    /// </summary>
    public partial class AnimKeyFrameGraph : CartesianChart
    {
        public Animation ViewingAnim { get; private set; }

        public enum GraphingData
        {
            Position,
            Rotation,
            Scale
        }

        public GraphingData CurrentlyShowing
        {
            get; private set;
        }

        public AnimKeyFrameGraph() 
        {
            InitializeComponent();
        }
        
        public AnimKeyFrameGraph(Animation ViewingAnim) : this()
        {
            DisplayAnimation(ViewingAnim);         
        }

        public void DisplayData(GraphingData data)
        {
            CurrentlyShowing = data;
            Invalidate();
        }

        public void DisplayAnimation(Animation ViewingAnim)
        {
            this.ViewingAnim = ViewingAnim;
            Invalidate();
        }

        public void Invalidate()
        {
            switch (CurrentlyShowing)
            {
                case GraphingData.Position:
                    PopulatePosition();
                    break;
                case GraphingData.Rotation:
                    PopulateRotation();
                    break;
            }
        }

        private void PopulatePosition()
        {
            var selectedAnim = ViewingAnim;
            PopulateChart("Time", "Position",
                ("X", selectedAnim.AnimRep.pos_anim.Select(x => x.Value.V.X)),
                ("Y", selectedAnim.AnimRep.pos_anim.Select(x => x.Value.V.Y)),
                ("Z", selectedAnim.AnimRep.pos_anim.Select(x => x.Value.V.Z)));
        }

        private void PopulateRotation()
        {
            var selectedAnim = ViewingAnim;
            PopulateChart("Time", "Rotation",
                ("W", selectedAnim.AnimRep.rot_anim.Select(x => x.Value.W)),
                ("X", selectedAnim.AnimRep.rot_anim.Select(x => x.Value.V.X)),
                ("Y", selectedAnim.AnimRep.rot_anim.Select(x => x.Value.V.Y)),
                ("Z", selectedAnim.AnimRep.rot_anim.Select(x => x.Value.V.Z)));
        }

        private void PopulateChart(string AxisXTitle, string AxisYTitle, params (string title, IEnumerable<float> series)[] data)
        {
            var DataChart = this;
            Axis axisX = null;
            for (int i = 0; i < 5; i++)
            {
                axisX = DataChart.AxisX.FirstOrDefault();
                if (axisX == null) continue;
                axisX.Title = AxisXTitle;
                break;
            }
            Axis axisY = null;
            for (int i = 0; i < 5; i++)
            {
                axisY = DataChart.AxisY.FirstOrDefault();
                if (axisY == null) continue;
                axisY.Title = AxisYTitle;
                break;
            }
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
