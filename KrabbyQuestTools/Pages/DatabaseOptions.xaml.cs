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
    public partial class DatabaseOptions : Page
    {
        public DatabaseOptions()
        {
            InitializeComponent();
            GamePathBox.Text = Properties.Settings.Default.GameResourcesPath;
        }

        private void PushButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CommandBox.Text))
            {
                try
                {
                    MethodInfo method = typeof(DBRepair).GetMethod(CommandBox.Text, BindingFlags.Static | BindingFlags.Public);
                    method.Invoke(null, null);
                }
                catch
                {
                    MessageBox.Show("The function: " + CommandBox.Text + " was not found. Press LIST to see available functions");
                    return;
                }
                if (MessageBox.Show("The function completed successfully! PUSH new changes?", "PUSH", MessageBoxButton.YesNo) == MessageBoxResult.No)
                    return;
            }
            //Process.Start("Resources//push.bat");
            File.Copy(AssetDBEntry.AssetDatabasePath, System.IO.Path.Combine(GamePathBox.Text, "texturedb.xml"), true);
            File.Copy(LevelDataBlock.BlockDatabasePath, System.IO.Path.Combine(GamePathBox.Text, "blockdb.xml"), true);
            Properties.Settings.Default.GameResourcesPath = GamePathBox.Text;
            Properties.Settings.Default.Save();
            (Parent as Window).Close();
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(string.Join("\n", FindFunctions()), "Repair Functions");
        }

        private IEnumerable<string> FindFunctions()
        {
            foreach (var method in typeof(DBRepair).GetMethods(BindingFlags.Public | BindingFlags.Static))
                yield return method.Name;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Window).Close();
        }
    }
}
