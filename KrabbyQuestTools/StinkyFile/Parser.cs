using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public class StinkyParser
    {
        /// <summary>
        /// Always 3 bytes behind level file header
        /// </summary>
        public static byte LEVEL_START_MARKER_0 = 0x01, LEVEL_START_MARKER_1 = 0x1B;
        public Dictionary<string, LevelDataBlock> UnknownBlocks = new Dictionary<string, LevelDataBlock>();
        private byte[] fileData;

        public IEnumerable<StinkyLevel> LevelInfo
        {
            get; private set;
        }

        public int BitRead
        {
            get => _bitRead;
            set
            {
                if (_bitRead != value)
                    bitSkipChanged = true;
                _bitRead = value;
            }
        }
        
        private bool bitSkipChanged = true;
        private bool pathChanged = true;
        private string _filePath;
        private int _bitRead = 4;

        private Dictionary<byte[], LevelDataBlock> DataCache = new Dictionary<byte[], LevelDataBlock>(new ArrayEqualityComparer<byte>());
        private Dictionary<string, byte[]> IDCache = new Dictionary<string, byte[]>();

        public string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                if (_filePath != value)
                    pathChanged = true;
                _filePath = value;
            }
        }

        public static StinkyLevel LoadLevelFile(string FilePath, out StinkyParser parser)
        {
            parser = new StinkyParser();
            return LoadLevel(parser, FilePath);
        }

        public static StinkyLevel LoadLevel(StinkyParser parser, string FilePath)
        {
            return parser.LevelRead(FilePath);
        }

        /// <summary>
        /// Creates a new <see cref="StinkyParser"/> object for reading <see cref="StinkyLevel"/> objects
        /// </summary>
        public StinkyParser()
        {

        }

        /// <summary>
        /// Finds all the levels based off the directory the levels are stored in
        /// </summary>
        /// <param name="LevelDirectory"></param>
        public void FindAllLevels(string LevelDirectory)
        {
            LevelInfo = null;
            var list = new List<StinkyLevel>();
            DirectoryInfo dir = new DirectoryInfo(LevelDirectory);
            if (!dir.Exists)
                throw new IOException("That directory does not exist");
            foreach(var file in dir.GetFiles())
            {
                var levelData = new StinkyLevel(this, file.FullName);
                list.Add(levelData);
            }
            LevelInfo = list.OrderBy(x =>
            {
                if (int.TryParse(x.LevelWorldName, out int levelIndex))
                    return levelIndex;
                else return 1000; // put unknown levels at the very end
            });
        }

        /// <summary>
        /// Load level from the specified filepath
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public StinkyLevel LevelRead(string FilePath)
        {
            return new StinkyLevel(this, FilePath);
        }

        [Obsolete]
        /// <summary>
        /// Reads a level at the specified index in a DAT file -- Sets the level as <see cref="OpenLevel"/> See: <see cref="LevelIndices"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StinkyLevel LevelRead(int index) => GetLevelByIndex(index);
        /// <summary>
        /// Reads a level with the specified Name in a DAT file -- Sets the level as <see cref="OpenLevel"/> See: <see cref="LevelIndices"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //public StinkyLevel LevelRead(string Name) => LevelRead(LevelIndices.First(x => x.Value == Name).Key);

        [Obsolete]
        /// <summary>
        /// Returns the level data for the level at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StinkyLevel GetLevelByIndex(int index) => new StinkyLevel(this, fileData, index);

        /// <summary>
        /// Loads a <see cref="LevelDataBlock"/> if it's already cached, or loads it from DB if it's not
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public LevelDataBlock CachedLoad(string ID, BlockLayers Layer, out bool success)
        {
            success = true;
            if (IDCache.ContainsKey(ID))
                return CachedLoad(IDCache[ID], Layer, out success);
            else
            {
                var block = LevelDataBlock.LoadFromDatabase(ID, out success);
                IDCache.Add(block.GUID, block.RawData);
                DataCache.Add(block.RawData, block);
                return block;
            }
        }

        /// <summary>
        /// Loads a <see cref="LevelDataBlock"/> if it's already cached, or loads it from DB if it's not
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public LevelDataBlock CachedLoad(byte[] data, BlockLayers Layer, out bool success)
        {
            success = true;
            if (DataCache.ContainsKey(data))
                return DataCache[data];
            else
            {
                var block = LevelDataBlock.LoadFromDatabase(data, Layer, out success);
                IDCache.Add(block.GUID, data);
                DataCache.Add(data, block);
                return block;
            }
        }

        public LevelDataBlock CacheRefresh(string ID)
        {
            var block = LevelDataBlock.LoadFromDatabase(ID, out bool success);
            if (block == null) return block;
            DataCache.Remove(block.RawData);
            DataCache.Add(block.RawData, block);
            return block;
        }

        public void CacheSaveAll()
        {
            foreach (var block in DataCache.Values)
                block.SaveToDatabase();
        }
            
        /// <summary>
        /// Clears the <see cref="StinkyLevel"/>'s data and loads/reloads it from file -- call before using the level!
        /// </summary>
        /// <param name="level"></param>
        public void RefreshLevel(StinkyLevel level)
        {
            int skip = BitRead;
            var data = File.ReadAllBytes(level.LevelFilePath).Skip(level.LevelContentIndex-1); // i fucked up early on so if you dont subtract 1 from the index the DB is unreadable. :( ill fix it later
            List<LevelDataBlock> blocks = new List<LevelDataBlock>();
            List<LevelDataBlock> decor = new List<LevelDataBlock>();
            DataCache.Clear();
            IDCache.Clear();
            UnknownBlocks.Clear();
            byte[] current = new byte[4]; // the current data block bytes
            int count = 0, amount = 0, index = -1;
            bool incomplete = true;
            BlockLayers constructing = BlockLayers.Integral; // the layer of the map being read by the parser
            foreach (var d in data)
            {
                index++;
                switch (constructing)
                {
                    case BlockLayers.Integral:
                        if (count == skip)
                        {
                            var block = CachedLoad(current, BlockLayers.Integral, out bool success);
                            blocks.Add(block);
                            if (!success && !UnknownBlocks.ContainsKey(block.Group + block.GroupId.ToString()))
                                UnknownBlocks.Add(block.Group + block.GroupId.ToString(), block);
                            count = 0;
                            if (index == level.ByteSize)
                            {
                                constructing = BlockLayers.Decoration;
                                index = -1;
                                continue;
                            }
                            current = new byte[4];
                            amount++;
                        }
                        current[count] = d;
                        count++;
                        break;
                    case BlockLayers.Decoration:
                        if (count == skip)
                        {                            
                            var block = CachedLoad(current, BlockLayers.Decoration, out bool success);
                            decor.Add(block);
                            //if (!success && !UnknownBlocks.ContainsKey(block.Group + block.GroupId.ToString()))
                              //  UnknownBlocks.Add(block.Group + block.GroupId.ToString(), block);
                            count = 0;
                            if (index >= level.ByteSize)
                            {
                                incomplete = false;
                                break;
                            }
                            current = new byte[4];
                            amount++;
                        }
                        current[count] = d;
                        count++;
                        break;
                }
                if (!incomplete) break;
            }
            level.IntegralData = blocks.ToArray();
            level.DecorationData = decor.ToArray();
            bitSkipChanged = false;
            pathChanged = false;
        }
    }
}
