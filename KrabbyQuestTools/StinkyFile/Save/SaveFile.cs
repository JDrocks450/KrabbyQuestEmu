using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Save
{

    public class SaveFile
    {
        static byte[] Header =
        {
            75, 114, 97, 98, 98, 121, 32, 81, 117, 101, 115, 116, 32, 83, 97,
            118, 101, 32, 70, 105, 108, 101, 32, 66, 121, 32, 74 ,101, 114, 101,
            109, 121, 32, 71, 108, 97, 122, 101, 98, 114, 111, 111, 107
        };
        static int HeaderLength => Header.Length;
        public static string SaveFileDir
        {
            get; set;
        } = "Resources/Saves";

        public SaveFileCompletionInfo SaveFileInfo
        {
            get; private set;
        } = new SaveFileCompletionInfo();

        public string FilePath
        {
            get; private set;
        }
        public bool Loaded { get; private set; }

        private int ContentIndex;

        public Dictionary<string, LevelCompletionInfo> LevelInfo = new Dictionary<string, LevelCompletionInfo>();

        private SaveFile()
        {

        }

        /// <summary>
        /// Creates a new save file using the next available save slot 
        /// </summary>
        /// <param name="Name"></param>
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
        /// <param name="slot"></param>
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
        /// <param name="filePath"></param>
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
        /// <param name="SaveDir"></param>
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
        /// Previews the save file by only getting the necessary data: PlayerName
        /// </summary>        
        public void Load()
        {
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
        }

        /// <summary>
        /// The header data must match the preset header data above
        /// </summary>
        /// <param name="stream"></param>
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
            Loaded = true;
            var handle = File.OpenRead(FilePath);
            if (!CheckHeader(handle))
                throw new Exception("The level header is not correct. The file could be corrupt or is not genuine.");
            handle.Position = ContentIndex;
            while (handle.Position < handle.Length)
            {
                var sizeArr = new byte[4];
                handle.Read(sizeArr, 0, 4);
                var size = BitConverter.ToInt32(sizeArr, 0);
                var serializedData = new byte[size];
                handle.Read(serializedData, 0, size);
                var info = Deserialize(serializedData);
                LevelInfo.Add(info.LevelWorldName, info);
            }
            handle.Dispose();
            RefreshStats();
        }

        /// <summary>
        /// Pushes the <see cref="LevelCompletionInfo"/> to the <see cref="LevelInfo"/> collection. Call <see cref="Save"/> to save changes to file
        /// </summary>
        /// <param name="NewInfo"></param>
        public void UpdateInfo(LevelCompletionInfo NewInfo)
        {
            if (LevelInfo.ContainsKey(NewInfo.LevelWorldName))
                LevelInfo.Remove(NewInfo.LevelWorldName);
            LevelInfo.Add(NewInfo.LevelWorldName, NewInfo);
        }

        /// <summary>
        /// Refreshes the level stats for this save file in <see cref="LevelInfo"/>
        /// </summary>
        public void RefreshStats()
        {
            SaveFileInfo = new SaveFileCompletionInfo()
            {
                PlayerName = SaveFileInfo.PlayerName,
                Slot = SaveFileInfo.Slot
            };
            foreach(var level in LevelInfo)
            {
                if (level.Value.WasSuccessful)
                    SaveFileInfo.CompletedLevels++;
                if (level.Value.WasPerfect)
                    SaveFileInfo.PerfectLevels++;
                SaveFileInfo.TotalScore += level.Value.LevelScore;
                SaveFileInfo.Spatulas = 10;
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
