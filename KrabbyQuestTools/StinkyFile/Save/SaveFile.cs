using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Save
{
    /// <summary>
    /// Interfaces with save files to read/write to them.
    /// </summary>
    public class SaveFile
    {
        /// <summary>
        /// The header expected in every genuine save file
        /// </summary>
        static byte[] Header =
        {
            75, 114, 97, 98, 98, 121, 32, 81, 117, 101, 115, 116, 32, 83, 97,
            118, 101, 32, 70, 105, 108, 101, 32, 66, 121, 32, 74 ,101, 114, 101,
            109, 121, 32, 71, 108, 97, 122, 101, 98, 114, 111, 111, 107
        };
        /// <summary>
        /// The length of the header
        /// </summary>
        static int HeaderLength => Header.Length;
        bool initLoad = false;
        /// <summary>
        /// The directory to find save files in
        /// </summary>
        public static string SaveFileDir
        {
            get; set;
        } = "Resources/Saves";
        /// <summary>
        /// Contains information about the save file and its completion status
        /// </summary>
        public SaveFileCompletionInfo SaveFileInfo
        {
            get; private set;
        } = new SaveFileCompletionInfo();
        /// <summary>
        /// The path to this save file (relative or absolute depending on <see cref="SaveFileDir"/>)
        /// </summary>
        public string FilePath
        {
            get; private set;
        }
        /// <summary>
        /// True if <see cref="FullLoad"/> has been called on this save file. Indicates whether level completion status has been loaded
        /// </summary>
        public bool Loaded { get; private set; }
        /// <summary>
        /// The number of unlocked levels
        /// </summary>
        public int UnlockedLevels { get; private set; }
        /// <summary>
        /// The total number of levels this save file contains
        /// </summary>
        public int TotalLevels { get; private set; }
        /// <summary>
        /// A string that says: Unlocked Levels: (x/x) (x.xx%)
        /// </summary>
        public string UnlockedLevelsString => $"Unlocked Levels: " +
            $"{UnlockedLevels}/{TotalLevels} ({(UnlockedLevels/(double)TotalLevels).ToString("P")})";
         /// <summary>
        /// A string that says: Perfected Levels: (x/x) (x.xx%)
        /// </summary>
        public string PerfectLevelsString => $"Perfected Levels: " +
            $"{SaveFileInfo.PerfectLevels}/{TotalLevels} ({(SaveFileInfo.PerfectLevels/(double)TotalLevels).ToString("P")})";
        /// <summary>
        /// A string that says: Completed Levels: (x/x) (x.xx%)
        /// </summary>
        public string CompletedLevelString => $"Completed Levels: " +
            $"{SaveFileInfo.CompletedLevels}/{TotalLevels} ({(SaveFileInfo.CompletedLevels/(double)TotalLevels).ToString("P")})";

        private int ContentIndex;
        /// <summary>
        /// The completion status for every level in this save file. See <see cref="UpdateInfo(LevelCompletionInfo)"/> to write to this
        /// </summary>
        public Dictionary<string, LevelCompletionInfo> LevelInfo = new Dictionary<string, LevelCompletionInfo>();

        private SaveFile()
        {

        }

        /// <summary>
        /// Creates a new save file using the next available save slot 
        /// </summary>
        /// <param name="Name">The PlayerName for this save file</param>
        public SaveFile(string Name)
        {
            int tries = 0;
            while (true)
            {
                Directory.CreateDirectory(SaveFileDir);
                var slot = getLastSaveIndex() + tries;
                var path = Path.Combine(SaveFileDir, slot.ToString());                
                if (File.Exists(path))
                {
                    tries++;
                    continue;
                }
                FilePath = path;
                SaveFileInfo.PlayerName = Name;
                SaveFileInfo.Slot = slot;
                break;
            }
        }

        /// <summary>
        /// Loads the save file at the slot provided
        /// </summary>
        /// <param name="slot">The slot to open</param>
        public SaveFile(int slot)
        {
            Directory.CreateDirectory(SaveFileDir);
            var path = Path.Combine(SaveFileDir, slot.ToString());
            if (!File.Exists(path))
                throw new Exception("Save file doesn't exist!");
            FilePath = path;
            SaveFileInfo.Slot = slot;
            Load();
        }

        /// <summary>
        /// Open a save file from the specified path
        /// </summary>
        /// <param name="filePath">The path to the save file relative/absolute</param>
        public SaveFile(Uri filePath)
        {
            if (!File.Exists(filePath.OriginalString))
                throw new Exception("Save file doesn't exist!");
            FilePath = filePath.OriginalString;
            if (int.TryParse(new string(
                Path.GetFileNameWithoutExtension(FilePath).
                Where(x => char.IsDigit(x)).ToArray()), out int slot)) // try to get slot from filename
                SaveFileInfo.Slot = slot;
            SaveFileInfo.IsBackup = FilePath.Substring(FilePath.Length - 4) == ".bak";
            Load();            
        }

        /// <summary>
        /// Gets all the save files in the directory
        /// </summary>
        /// <param name="SaveDir">The directory to use - defaults to <see cref="SaveFileDir"/>. This will update <see cref="SaveFileDir"/> to the value provided.</param>
        /// <returns></returns>
        public static SaveFile[] GetAllSaves(string SaveDir = default)
        {
            if (SaveDir == default)
                SaveDir = SaveFileDir;
            if (!Directory.Exists(SaveDir)) 
                return new SaveFile[0];
            var dirFiles = Directory.GetFiles(SaveDir);
            SaveFile[] files = new SaveFile[dirFiles.Length];
            int index = 0;
            foreach(var file in dirFiles)
            {
                var saveFile = files[index] = new SaveFile(new Uri(file, UriKind.Relative));
                index++;
            }
            return files;
        }

        /// <summary>
        /// Previews the save file by only getting the necessary preview data: PlayerName
        /// </summary>        
        public void Load()
        {
            if (!File.Exists(FilePath)) return;
            var handle = File.OpenRead(FilePath);
            if (!CheckHeader(handle))
                throw new Exception("The level header is not correct. The file could be corrupt or is not genuine.");
            handle.Position = HeaderLength;
            var nameArr = new byte[4];
            handle.Read(nameArr, 0, 4);
            int nameLength = BitConverter.ToInt32(nameArr, 0);
            nameArr = new byte[nameLength];
            handle.Read(nameArr, 0, nameLength);
            SaveFileInfo.PlayerName = Encoding.ASCII.GetString(nameArr);
            ContentIndex = (int)handle.Position;
            handle.Dispose();
            initLoad = true;
        }

        /// <summary>
        /// The header data must match the preset header data above
        /// </summary>
        /// <param name="stream">The save file stream</param>
        /// <returns></returns>
        private bool CheckHeader(FileStream stream)
        {
            var header = new byte[HeaderLength];
            stream.Read(header, 0, HeaderLength);
            var equalityComparer = new ArrayEqualityComparer<byte>();
            return equalityComparer.Equals(header, Header);
        }

        /// <summary>
        /// Full loads the save file data by loading every level's data
        /// </summary>
        public void FullLoad()
        {
            if (Loaded)
                return;
            if (!initLoad)
                Load();            
            if (!File.Exists(FilePath))
            {
                Loaded = true;
                return;
            }
            var handle = File.OpenRead(FilePath);
            if (!CheckHeader(handle))
                throw new Exception("The level header is not correct. The file could be corrupt or the save file is not genuine. " +
                    "You can recover the save file from a backup using the \"Save File Editor\" tool in the Editor. ");
            handle.Position = ContentIndex;
            LevelInfo.Clear();
            int CURRENT = 0, lastAvailableLevel = 0, totalUnlocked = 0;
            while (handle.Position < handle.Length)
            {
                var sizeArr = new byte[4];
                handle.Read(sizeArr, 0, 4);
                var size = BitConverter.ToInt32(sizeArr, 0);
                var serializedData = new byte[size];
                handle.Read(serializedData, 0, size);
                var info = Deserialize(serializedData);
                LevelInfo.Add(info.LevelWorldName, info);
                bool isAvailable = false;
                if (CURRENT == 0 || CURRENT == 1 || info.WasSuccessful ||
                    lastAvailableLevel == CURRENT - 1)
                {
                    isAvailable = true;
                    totalUnlocked++;
                }
                if (info.WasSuccessful)
                    lastAvailableLevel = CURRENT;
                info.IsAvailable = isAvailable;
                CURRENT++;
            }
            TotalLevels = CURRENT;
            UnlockedLevels = totalUnlocked;
            handle.Dispose();
            RefreshStats();
            Loaded = true;
        }

        /// <summary>
        /// Pushes the <see cref="LevelCompletionInfo"/> to the <see cref="LevelInfo"/> collection. Call <see cref="Save"/> to save changes to file
        /// </summary>
        /// <param name="NewInfo">The data to update in <see cref="LevelInfo"/>. LevelWorldName must be set or else an exception will be thrown. </param>
        public void UpdateInfo(LevelCompletionInfo NewInfo)
        {
            if (NewInfo.LevelWorldName == null)
                throw new Exception("Error in updating save file. LevelWorldName is not set properly.");
            if (LevelInfo.ContainsKey(NewInfo.LevelWorldName))
                LevelInfo.Remove(NewInfo.LevelWorldName);
            LevelInfo.Add(NewInfo.LevelWorldName, NewInfo);
        }

        /// <summary>
        /// Refreshes the level stats for this save file in <see cref="LevelInfo"/>
        /// </summary>
        public void RefreshStats()
        {
            SaveFileInfo.CompletedLevels = 0;
            SaveFileInfo.PerfectLevels = 0;
            SaveFileInfo.Spatulas = 10;
            SaveFileInfo.TotalScore = 0;
            foreach(var level in LevelInfo)
            {
                if (level.Value.WasSuccessful)
                    SaveFileInfo.CompletedLevels++;
                if (level.Value.WasPerfect)
                    SaveFileInfo.PerfectLevels++;
                SaveFileInfo.TotalScore += level.Value.LevelScore;
            }
        }

        /// <summary>
        /// Save the <see cref="LevelInfo"/> to the path: <see cref="SaveFileDir"/> \ Slot.
        /// </summary>
        public void Save()
        {
            if (File.Exists(FilePath))
                File.Copy(FilePath, FilePath + ".bak", true);
            var handle = File.Create(FilePath);
            BinaryFormatter formatter = new BinaryFormatter();
            // write header
            handle.Write(Header, 0, HeaderLength);
            //write name
            var nameArr = Encoding.ASCII.GetBytes(SaveFileInfo.PlayerName);
            handle.Write(BitConverter.GetBytes(nameArr.Length), 0, 4);
            handle.Write(nameArr, 0, nameArr.Length);
            //serialize each level's completion status
            foreach (var level in LevelInfo.Values)
            {
                using (var ms = new MemoryStream()) 
                {
                    formatter.Serialize(ms, level);
                    handle.Write(BitConverter.GetBytes((int)ms.Length), 0, 4);
                    handle.Write(ms.ToArray(), 0, (int)ms.Length);
                }
            }
            handle.Dispose();
            Loaded = false;
            FullLoad(); // refresh the save file to keep perfect sync
        }

        private LevelCompletionInfo Deserialize(byte[] serializedData)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(serializedData))
                return (LevelCompletionInfo)formatter.Deserialize(ms);
        }

        int getLastSaveIndex() => Directory.GetFiles(SaveFileDir).Length + 1;
    }
}
