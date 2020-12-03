using KrabbyQuestTools.Controls;
using StinkyFile;
using StinkyFile.Util;
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
    /// Interaction logic for DiffPage.xaml
    /// </summary>
    public partial class DiffPage : Page
    {
        DBDiffUtility diffHost;
        public DiffPage(string Path1, string Path2)
        {
            InitializeComponent();
            PathSubtextOriginal.Text = Path1;
            PathSubtextNew.Text = Path2;
            diffHost = new DBDiffUtility(DBDiffUtility.DBSelection.BlockDB, Path1, Path2);     
            diffHost.PercentUpdated += async (object s, double percent) => await Dispatcher.InvokeAsync(() => Progress.Value = percent * 100);
            RefreshDiff();
        }

        private async Task PopulateScreen()
        {
            async Task CreateTile(Panel destination, LevelDataBlock block, bool isModifiedDB)
            {
                await Dispatcher.InvokeAsync(delegate
                {
                    var cell = new LevelEditorTile(LevelContext.BIKINI, block)
                    {
                        Width = 20,
                        Height = 20
                    };
                    cell.MouseDown += delegate
                    {
                        if (!isModifiedDB)
                            OriginalSelected.Text = block.GUID + " " + block.Name;
                        else ModifiedSelected.Text = block.GUID + " " + block.Name;
                    };
                    destination.Children.Add(cell);
                });
            }
            Dispatcher.Invoke(() =>
            {
                AllBlocksWrap.Children.Clear();
                AllBlocksWrapModified.Children.Clear();
                AddedBlocksWrap.Children.Clear();
                RemovedBlocksWrap.Children.Clear();
            });
            foreach (var entry in diffHost.DB1_BlockData)
                await CreateTile(AllBlocksWrap, entry, false);
            foreach (var entry in diffHost.DB2_BlockData)
                await CreateTile(AllBlocksWrapModified, entry, true);
            foreach (var entry in diffHost.AddedBlocks.Values)
                await CreateTile(AddedBlocksWrap, entry, true);
            foreach (var entry in diffHost.RemovedBlocks.Values)
                await CreateTile(AddedBlocksWrap, entry, true);
        }

        private void RefreshDiff()
        {            
            diffHost.FindDifferences().ContinueWith(async delegate
            {
                await PopulateScreen();
            });
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshDiff();
        }
    }
}
