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

namespace KrabbyQuestTools.Pages
{
    /// <summary>
    /// Interaction logic for GalleryPage.xaml
    /// </summary>
    public partial class GalleryPage : Page
    {
        public GalleryPage()
        {
            InitializeComponent();
            Gallery.OnDataSelected += Gallery_OnDataSelected;
        }

        private void Gallery_OnDataSelected(object sender, StinkyFile.LevelDataBlock e)
        {
            PopulateInfo(e);
        }

        private void PopulateInfo(LevelDataBlock block)
        {
            AssetInfoStack.Children.Clear();
            AssetInfoStack.Children.Add(assetInfoLabel);
            foreach (var assetRef in block.AssetReferences)
            {
                Button border = new Button()
                {
                    Background = Brushes.DarkCyan,
                    Content = $"Asset GUID: {assetRef.guid} \n" +
                    $"Asset Type: {assetRef.type}",
                    Margin = new Thickness(0,0,0,5),
                    Padding = new Thickness(10,5,10,5),
                    BorderBrush = Brushes.Turquoise
                };
                border.Click += delegate
                {
                    MainWindow.PushTab(new TextureToolPage(Properties.Settings.Default.DestinationDir, assetRef.guid)); 
                };
                AssetInfoStack.Children.Add(border);
            }
        }
    }
}
