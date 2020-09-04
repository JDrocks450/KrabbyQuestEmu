using StinkyFile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for DatabaseOptions.xaml
    /// </summary>
    public partial class ModelExporterOptions : Page
    {
        public ModelExporterOptions()
        {
            InitializeComponent();
            GamePathBox.Text = System.IO.Path.Combine(Properties.Settings.Default.DestinationDir);
        }

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Command-Windows will rapidly open and close. This is Blender" +
                " exporting the models in the background. Please be patient!");
            var exporter = new ModelExporter(BlenderPathBox.Text,
                System.IO.Path.Combine(GamePathBox.Text, "Graphics"), System.IO.Path.Combine(GamePathBox.Text, "Export"));
            PushButton.IsEnabled = false;
            PushButton.Content = "Exporting... Do Not Close Window";
            CancelButton.IsEnabled = false;
            Task.Run(() => exporter.ExportAll())
                .ContinueWith(delegate
                {                    
                    Dispatcher.Invoke(delegate
                    {
                        if (exporter.ExceptionObject != null)
                            MessageBox.Show("Error extracting, check your paths and try again. " + exporter.ExceptionObject.Message);
                        else
                        {
                            MessageBox.Show("Success! The models were sent to: " + GamePathBox.Text);
                            (Parent as Window).Close();
                        }
                    });
                });            
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Close();
        }
    }
}
