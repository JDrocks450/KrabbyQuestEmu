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
    /// Interaction logic for Gallery.xaml
    /// </summary>
    public partial class Gallery : UserControl
    {
        public event EventHandler<LevelDataBlock> OnDataSelected;
        public Gallery()
        {
            InitializeComponent();
            Populate();
        }

        private void Populate(string searchTerm = "")
        {
            GalleryWrapper.Children.Clear();
            foreach(var block in LevelDataBlock.LoadAllFromDB())
            {
                if (searchTerm == "" || block.Name.ToLower().Contains(searchTerm.ToLower()))
                {
                    var stack = new StackPanel();
                    stack.Children.Add(new Image()
                    {

                    });
                    stack.Children.Add(new TextBlock()
                    {
                        Text = block.Name,
                        TextWrapping = TextWrapping.Wrap,
                        //Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF343434"))
                    });
                    var button = new Button()
                    {
                        Content = stack,
                        Tag = block,
                        Padding = new Thickness(10),
                        Margin = new Thickness(5, 5, 0, 0),
                        Background = new SolidColorBrush(AppResources.S_ColorConvert(block.Color))
                    };
                    button.Click += delegate
                    {
                        OnDataSelected?.Invoke(this, button.Tag as LevelDataBlock);
                    };
                    GalleryWrapper.Children.Add(button);
                }
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Populate(SearchBox.Text);
        }
    }
}
