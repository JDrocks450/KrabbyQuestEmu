using StinkyFile;
using StinkyFile.Save;
using System;
using System.Collections.Generic;
using System.IO;
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
            LevelSaves.Clear();
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
            UnlockedLevels.Text = $"{saveFile.UnlockedLevels}/{saveFile.TotalLevels} ({(saveFile.UnlockedLevels/(double)saveFile.TotalLevels).ToString("P")})";
            TotalScore.Text = saveFile.SaveFileInfo.TotalScore.ToString();
            PerfectLevels.Text = $"{saveFile.SaveFileInfo.PerfectLevels}/{saveFile.TotalLevels} ({(saveFile.SaveFileInfo.PerfectLevels/(double)saveFile.TotalLevels).ToString("P")})"; 
            SpatulasLeft.Text = saveFile.SaveFileInfo.Spatulas.ToString();
            PassedLevels.Text = $"{saveFile.SaveFileInfo.CompletedLevels}/{saveFile.TotalLevels} ({(saveFile.SaveFileInfo.CompletedLevels/(double)saveFile.TotalLevels).ToString("P")})";
            Title = "SFV - Viewing " + saveFile.SaveFileInfo.PlayerName;
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
                Rating.Text = "Completed";
            }
            else if (info.IsAvailable)
                Rating.Text = "Unlocked";
            else Rating.Text = "Locked";
            TimeCompleted.Text = info.TimeRemaining.ToString();
            SaveLevel.IsEnabled = !IsProtectedViewing;
            SaveLevel.Content = "Apply Changes";
            if (!info.IsAvailable)
                SaveLevel.Content = "Unlock Level and Apply Changes";
            else if (!info.WasSuccessful)
                SaveLevel.Content = "Complete Level and Apply Changes";
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
            SaveFileStack.Children.Add(SaveTitle);
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
            Title = "SFV - Selecting A Save...";
            FindAllSaves();
            SaveFileDialog.Visibility = Visibility.Visible;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => AnotherSaveFileButton_Click(null, null);

        private void RecoverSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will restore the current backed-up save file. " +
                "The save file being used in this save-slot will be replaced by the backup opened now. " +
                "You should only use this recovery tool if your save file is corrupt or otherwise lost! " +
                "Do you wish to replace your current save file with this backup?",
                "WARNING", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var path = saveFile.FilePath.Substring(0, saveFile.FilePath.Length - 4);
                File.Copy(saveFile.FilePath, path, true);
                File.Delete(saveFile.FilePath);
                SelectSaveFile(new SaveFile(new Uri(path, UriKind.Relative)));
                MessageBox.Show("The backup save file has been restored. It has been automatically opened and is being viewed right now.");
            }
        }

        private void Rectangle_DragEnter(object sender, DragEventArgs e)
        {
            DragAndDropTarget.Stroke = Brushes.DeepSkyBlue;
            var background = Brushes.DeepSkyBlue;
            background = new SolidColorBrush(Color.FromArgb((byte)(255 / 4.0), background.Color.R, background.Color.G, background.Color.B));
            DragAndDropTarget.Fill = background;
        }

        private void DragAndDropTarget_DragLeave(object sender, DragEventArgs e)
        {
            var color = (Color)ColorConverter.ConvertFromString("#FF009580");
            var background = new SolidColorBrush(color);
            DragAndDropTarget.Stroke = background;
            color.A = (byte)(255 / 4.0);
            DragAndDropTarget.Fill = new SolidColorBrush(color);
        }

        private void DragAndDropTarget_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] file = (string[])e.Data.GetData(DataFormats.FileDrop);
                try
                {
                    SelectSaveFile(new SaveFile(new Uri(file[0], UriKind.Absolute)));
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            DragAndDropTarget_DragLeave(null, null);
        }
    }
}
