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
        public TreeVisualizer(string Workspace)
        {
            InitializeComponent();
            var obj = StinkyFile.Blitz3D.B3D.B3D_Loader.Load(System.IO.Path.Combine(Workspace, "Graphics", "seaweedgate.b3d"));
            var visual = new BlitzTreeVisualizer(obj.LoadedObjects, obj.RootObject);
            TextContent.Text = visual.GetTextTree() + "\nLoaded Objects: " + obj.LoadedObjects.Count;
        }

    }
}
