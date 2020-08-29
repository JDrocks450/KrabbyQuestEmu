using KrabbyQuestTools.Pages;
using StinkyFile;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace KrabbyQuestTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string TabDeclarationCached;
        public MainWindow()
        {
            InitializeComponent();
            TabDeclarationCached = AppResources.XamlToString(TabSwitcher.Items[0] as TabItem);            
        }

        private void CreateTab_Selected(object sender, RoutedEventArgs e)
        {
            TabItem tab = AppResources.CloneXaml<TabItem>(TabDeclarationCached);
            (tab.Content as Frame).Content = new LevelSelect();
            var closeButton = (((tab.Header as Border).Child as DockPanel).Children[0] as Button);            
            (tab.Content as Frame).Tag = tab;
            (tab.Content as Frame).Navigating += MainWindow_Navigating;            
            closeButton.Tag = tab;
            closeButton.Click += CloseButtonClick;
            TabSwitcher.Items.Insert(TabSwitcher.Items.Count - 1,tab);
            int x = TabSwitcher.Items.Count - 1;
            Dispatcher.BeginInvoke((Action)(() => TabSwitcher.SelectedItem = tab));
        }

        Dictionary<TabItem, EventHandler> currentHandlers = new Dictionary<TabItem, EventHandler>();

        private void MainWindow_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            TabItem tab = (sender as Frame).Tag as TabItem;
            var title = ((tab.Header as Border).Child as DockPanel).Children[1] as TextBlock;
            var page = (Page)e.Content;
            title.Text = page.Title;
            if (currentHandlers.TryGetValue(tab, out var handler))
            {
                DependencyPropertyDescriptor
                    .FromProperty(Page.TitleProperty, typeof(Page))
                    .RemoveValueChanged(page, handler);
                currentHandlers.Remove(tab);
            }
            currentHandlers.Add(tab, (s, ea) =>
            {
                title.Text = page.Title;
            });
            DependencyPropertyDescriptor
                .FromProperty(Page.TitleProperty, typeof(Page))
                .AddValueChanged(page, currentHandlers[tab]);
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            var tab = (TabItem)(sender as Button).Tag;
            var stinkyPage = ((tab.Content as Frame).Content) as StinkyUI;
            if (stinkyPage != null)
                if (!stinkyPage.OnClosing()) return;
            TabSwitcher.Items.Remove(tab);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TabSwitcher.Items.RemoveAt(0);
        }

        private void TabSwitcher_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (e.AddedItems.Count == 0) return;            
            try
            {
                var tab = (TabItem)e.AddedItems[0];
                var oldSelection = default(TabItem);
                if (e.RemovedItems.Count > 0) oldSelection = (TabItem)e.RemovedItems[0];
                if (tab != CreateTab)
                    ((Border)tab.Header).Background = Brushes.DarkCyan;
                if (oldSelection != null && oldSelection != CreateTab)
                    ((Border)oldSelection.Header).Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF323232"));
            }
            catch
            {

            }
        }
    }
}
