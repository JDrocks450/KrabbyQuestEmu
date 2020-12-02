using KrabbyQuestTools.Controls;
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
        public static MainWindow ApplicationMainWindow;
        public MainWindow()
        {
            InitializeComponent();
            ApplicationMainWindow = this;
            TabDeclarationCached = AppResources.XamlToString(TabSwitcher.Items[0] as TabItem);            
        }

        public static void PushTab(Page Content) => ApplicationMainWindow.PushNewTab(Content);

        public void PushNewTab(Page Content)
        {
            TabItem tab = AppResources.CloneXaml<TabItem>(TabDeclarationCached);
            (tab.Content as Frame).Content = Content;
            var closeButton = (((tab.Header as Border).Child as DockPanel).Children[0] as Button);  
            var title = (((tab.Header as Border).Child as DockPanel).Children[1] as TextBlock);
            title.Text = Content.Title;
            (tab.Content as Frame).Tag = tab;
            (tab.Content as Frame).Navigating += MainWindow_Navigating;            
            closeButton.Tag = tab;
            closeButton.Click += CloseButtonClick;
            TabSwitcher.Items.Insert(TabSwitcher.Items.Count - 1,tab);
            int x = TabSwitcher.Items.Count - 1;
            Dispatcher.BeginInvoke((Action)(() => TabSwitcher.SelectedItem = tab));
        }

        private void CreateTab_Selected(object sender, RoutedEventArgs e)
        {
            PushNewTab(new LevelSelect());
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
            if (tab.Content == null) return;
            var stinkyPage = ((tab.Content as Frame).Content) as KQTPage;
            if (stinkyPage != null)
                if (!stinkyPage.OnClosing()) return;
            if (tab.IsSelected)
            {
                if (TabSwitcher.SelectedIndex > 0)
                    TabSwitcher.SelectedIndex--;
                else return;
            }
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

        private void Window_Activated(object sender, EventArgs e)
        {
            foreach(TabItem tab in TabSwitcher.Items)
            {
                if (tab.Content == null) return;
                var page = ((tab.Content as Frame).Content) as KQTPage;
                if (page != null)
                    page.OnActivated();
            }
        }
    }
}
