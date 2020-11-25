using KrabbyQuestTools.Common;
using Paloma;
using StinkyFile.Installation;
using StinkyFile.Primitive;
using System;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace KrabbyQuestInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum ScreenState
        {
            None,
            Installer,
            Launcher
        }

        public Page CurrentPage { get; private set; }
        public readonly Page InstallerScreen, LauncherScreen;
        public ScreenState CurrentState { get; private set; }

        private double ScreenExpectedWidth;
        private ThicknessAnimation ScreenAnimation;
        private bool AnimationRunning = false;

        private void SetContentHostWidth(double Width)
        {
            if (!double.IsNaN(Width))
            {
                double width = Width * 2 - 20;
                ContentHost.Width = width;
                ScreenExpectedWidth = width;
            }
            if (!AnimationRunning)
            {
                switch (CurrentState)
                {
                    case ScreenState.Installer:
                        ContentHost.Margin = new Thickness();
                        break;
                    case ScreenState.Launcher:
                        ContentHost.Margin = new Thickness(-ScreenExpectedWidth / 2, 0, 0, 0);
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InstallerScreen = new InstallerPage();
            LauncherScreen = new LauncherScreen();
            WindowContentLeft.Content = InstallerScreen;
            WindowContentRight.Content = LauncherScreen;
            SetContentHostWidth(Width);
            SwitchScreen(ScreenState.Installer);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(CurrentPage is InstallerPage)) return;
            var installPage = CurrentPage as InstallerPage;
            if (installPage.Installing)
            {
                installPage.Installation.Paused = true;
                if (MessageBox.Show("The installation has not completed yet. Are you sure you want to " +
                    "close the installer now? ", "Installation In Progress", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    e.Cancel = false;
                else
                {
                    installPage.Installation.Paused = false;
                    e.Cancel = true;
                }
            }
        }

        public void SwitchScreen(ScreenState screenState)
        {
            if (screenState == CurrentState) return;
            Brush highlight = Brushes.DarkCyan;
            Brush shadow = Brushes.Gray;
            double timespanSeconds = .25;
            switch (screenState)
            {
                case ScreenState.Installer:
                    InstallerButton.Background = highlight;
                    LauncherButton.Background = shadow;
                    CurrentPage = InstallerScreen;
                    LauncherScreen.IsEnabled = false;
                    WindowContentLeft.Content = InstallerScreen;
                    InstallerScreen.IsEnabled = true;
                    ScreenAnimation = new ThicknessAnimation(
                        new Thickness(0, 0, 0, 0), TimeSpan.FromSeconds(timespanSeconds));
                    break;
                case ScreenState.Launcher:
                    InstallerButton.Background = shadow;
                    LauncherButton.Background = highlight;
                    CurrentPage = LauncherScreen;
                    InstallerScreen.IsEnabled = false;
                    WindowContentRight.Content = LauncherScreen;
                    LauncherScreen.IsEnabled = true;
                    ScreenAnimation = new ThicknessAnimation(
                        new Thickness(-ScreenExpectedWidth/2, 0, 0, 0), TimeSpan.FromSeconds(timespanSeconds));
                    break;
            }
            AnimationRunning = true;
            ScreenAnimation.Completed += ScreenAnimation_Completed;
            ScreenAnimation.FillBehavior = FillBehavior.Stop;
            ContentHost.BeginAnimation(MarginProperty, ScreenAnimation);
            CurrentState = screenState;
        }

        private void ScreenAnimation_Completed(object sender, EventArgs e)
        {
            AnimationRunning = false;
            switch (CurrentState)
            {
                case ScreenState.Launcher:
                    WindowContentLeft.Content = null;
                    break;
                case ScreenState.Installer:
                    WindowContentRight.Content = null;
                    break;
            }
            SetContentHostWidth(double.NaN);
        }

        private void InstallerButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchScreen(ScreenState.Installer);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged)
                SetContentHostWidth(e.NewSize.Width);
        }

        private void LauncherButton_Click(object sender, RoutedEventArgs e)
        {
            SwitchScreen(ScreenState.Launcher);
        }
    }
}
