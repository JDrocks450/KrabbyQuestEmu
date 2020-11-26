using KrabbyQuestTools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace KrabbyQuestInstaller
{
    /// <summary>
    /// Interaction logic for LauncherScreen.xaml
    /// </summary>
    public partial class LauncherScreen : Page
    {

        public LauncherScreen()
        {
            InitializeComponent();
        }

        public Process LaunchComponent(ProjectComponent Component)
        {
            string path = "";
            var settings = Properties.Settings.Default;
            switch (Component)
            {
                case ProjectComponent.Editor:
                    path = settings.EditorExePath;
                    break;
                case ProjectComponent.Game:
                    path = settings.GameExePath;
                    break;
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("The application has not been found. Try installing again, or manually looking in the " +
                    "installation directory for Krabby Quest Game.exe/KrabbyQuestTools.exe", "Game not found");
                return null;
            }
            ProcessStartInfo info = new ProcessStartInfo(path)
            {
                UseShellExecute = false,
                WorkingDirectory = System.IO.Path.GetDirectoryName(path)
            };
            Process p = new Process()
            {
                StartInfo = info,                
            };
            p.Start();
            return p;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var process = LaunchComponent(ProjectComponent.Game);
            if (process == null) return;
            var state = Application.Current.MainWindow.WindowState;
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
            Task.Run(() =>
            {
                process.WaitForExit();
                Dispatcher.Invoke(delegate
                {
                    Application.Current.MainWindow.WindowState = state;
                });
            });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditorButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchComponent(ProjectComponent.Editor);
        }
    }
}
