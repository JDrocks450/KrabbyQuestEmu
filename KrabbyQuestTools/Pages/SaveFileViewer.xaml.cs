using StinkyFile;
using StinkyFile.Save;
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
    /// Interaction logic for SaveFileViewer.xaml
    /// </summary>
    public partial class SaveFileViewer : Page
    {
        private StinkyParser Parser;
        public Dictionary<StinkyLevel, LevelCompletionInfo> LevelSaves = new Dictionary<StinkyLevel, LevelCompletionInfo>();
        SaveFile saveFile;
        bool IsProtectedViewing;

        public LevelCompletionInfo OpenInfo { get; private set; }

        public SaveFileViewer(string LevelDir)
        {
            InitializeComponent();
            Parser = new StinkyParser();
            Parser.FindAllLevels(LevelDir);
            AnotherSaveFileButton_Click(null, null);
        }

        private void GetLevels()
        {
            foreach(var level in Parser.LevelInfo)
            {
                LevelSaves.Add(level, level.GetSaveFileInfo(saveFile));
                var button = new Button()
                {
                    Content = level.Name,
                    Margin = new Thickness(10, 10, 10, 0),
                    Padding = new Thickness(10, 5, 10, 5),
                    Tag = level
                };
                button.Click += LevelSelected;
                LevelButtons.Children.Add(button);
            }
        }

        private void LevelSelected(object sender, RoutedEventArgs e)
        {
            var level = (StinkyLevel)(sender as Button).Tag;
            ShowInfo(LevelSaves[level]);
        }

        private void ShowFileInfo()
        {
            FileTitleText.Text = saveFile.SaveFileInfo.PlayerName + "'s Krabby Quest Save File - Slot: #" + saveFile.SaveFileInfo.Slot;
            if (IsProtectedViewing)
            {
                ProtectedViewing.Visibility = Visibility.Visible;
                FullEdit.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProtectedViewing.Visibility = Visibility.Collapsed;
                FullEdit.Visibility = Visibility.Visible;
            }
            TotalScore.Text = saveFile.SaveFileInfo.TotalScore.ToString();
            PerfectLevels.Text = saveFile.SaveFileInfo.PerfectLevels.ToString();
            SpatulasLeft.Text = saveFile.SaveFileInfo.Spatulas.ToString();
            PassedLevels.Text = saveFile.SaveFileInfo.CompletedLevels.ToString();
        }

        private void ShowInfo(LevelCompletionInfo info)
        {
            LevelFileTitle.Text = info.LevelName + ", ID: " + info.LevelWorldName;
            LevelScore.Text = info.LevelScore.ToString();
            PattiesCollected.Text = info.PattiesCollected.ToString();
            BonusesCollected.Text = info.BonusesCollected.ToString();
            if (info.WasSuccessful)
            {
                if (info.WasPerfect)
                    Rating.Text = "Perfected";
                Rating.Text = "Beaten";
            }
            else
                Rating.Text = "Unbeaten";
                TimeCompleted.Text = info.TimeRemaining.ToString();
            SaveLevel.IsEnabled = !IsProtectedViewing;
            SaveToFile.IsEnabled = !IsProtectedViewing;
            OpenInfo = info;
        }

        private void SaveLevel_Click(object sender, RoutedEventArgs e)
        {
            var info = OpenInfo;
            info.PattiesCollected = int.Parse(PattiesCollected.Text);
            info.BonusesCollected = int.Parse(BonusesCollected.Text);
            info.TimeRemaining = int.Parse(TimeCompleted.Text);
            info.WasSuccessful = true;
            saveFile.RefreshStats();
            ShowInfo(info);
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            saveFile.Save();
        }

        private void CreateNewSave_Click(object sender, RoutedEventArgs e)
        {
            SelectSaveFile(new SaveFile("TEST FILE"));
        }

        private void SelectSaveFile(SaveFile Selected)
        {
            SaveFileDialog.Visibility = Visibility.Collapsed;
            saveFile = Selected;
            Selected.FullLoad();
            IsProtectedViewing = Selected.SaveFileInfo.IsBackup;
            ShowFileInfo();
            GetLevels();
            if (LevelSaves.Count > 0)
                ShowInfo(LevelSaves.Values.First());
        }

        private void FindAllSaves()
        {
            SaveFileStack.Children.Clear();
            SaveFileStack.Children.Add(Title);
            SaveFileStack.Children.Add(CreateNewSave);
            SaveFileStack.Children.Add(Separator);
            SaveFileStack.Children.Add(RefreshButton);
            var saveFiles = StinkyFile.Save.SaveFile.GetAllSaves();
            foreach (var save in saveFiles)
            {
                var button = new Button()
                {
                    Content = (save.SaveFileInfo.IsBackup ? "[BACKUP] " : "") + $"[{save.SaveFileInfo.Slot}]: " + save.SaveFileInfo.PlayerName,
                    Padding = new Thickness(10, 5, 10, 5),
                    Margin = new Thickness(0, 0, 0, 5)
                };
                button.Click += delegate { SelectSaveFile(save); };
                SaveFileStack.Children.Add(button);
            }
        }

        private void AnotherSaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            FindAllSaves();
            SaveFileDialog.Visibility = Visibility.Visible;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => AnotherSaveFileButton_Click(null, null);
    }
}
