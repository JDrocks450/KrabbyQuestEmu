using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public class StinkyLevel
    {
        public int ByteSize => Rows * Columns * 4;
        public string Name { get; set; }
        internal int LevelContentIndex = 0;

        private StinkyParser Parent;
        public int LevelHeaderIndex { get; private set; }
        public LevelDataBlock[] IntegralData;
        public LevelDataBlock[] DecorationData;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Total => IntegralData?.Length ?? 0;
        public StinkyLevel(StinkyParser parent, byte[] FileData, int index)
        {
            Parent = parent;
            LevelHeaderIndex = index;
            LoadFromDat(FileData, index);
        }

        internal void LoadFromDat(byte[] data, int index)
        {            
            var name = data.Skip(index + 0x28).Take(0x1E).ToArray();
            var text = Encoding.ASCII.GetString(name).Trim(); // get level name    
            HashSet<byte> secretBytes = new HashSet<byte>();
            for(int i = 0; i < 10; i++) // levels have a "secret" in the header
            {
                byte b = data[index + 0x1F + i];
                if (b != 0)
                    secretBytes.Add(b);
                else break;
            }
            int sizeHeaderIndex = 0;
            if (text.Contains(',')) //the comma was already found -- just use the index of it in the title string
            {
                sizeHeaderIndex = index + 0x28 + text.IndexOf(',');
            }
            for (int i = 0; i < 40; i++) // find the comma that marks the size header after the end of the title string, if possible to reduce the chance of error
            {
                var b = data[index + 0x28 + 0x1E + i];
                if (b == 0x2C)
                {
                    sizeHeaderIndex = index + 0x28 + 0x1E + i;
                    break;
                }
            }
            sizeHeaderIndex++;
            LevelContentIndex = sizeHeaderIndex + 0x12;
            Columns = data[sizeHeaderIndex + 0xB];
            Rows = data[sizeHeaderIndex + 0xF];
            Parent.RefreshLevel(this);            
        }

        public void SaveAll()
        {
            List<string> savedIds = new List<string>();
            foreach (var block in IntegralData)
                if (block != null)
                    if (!savedIds.Contains(block.GUID))
                    {
                        block?.SaveToDatabase();
                        savedIds.Add(block.GUID);
                    }
        }
    }
    public class StinkyParser
    {
        /// <summary>
        /// Always 3 bytes behind level file header
        /// </summary>
        public static byte LEVEL_START_MARKER_0 = 0x01, LEVEL_START_MARKER_1 = 0x1B;
        public Dictionary<int, string> LevelIndices = new Dictionary<int, string>();
        public Dictionary<string, LevelDataBlock> UnknownBlocks = new Dictionary<string, LevelDataBlock>();
        private byte[] fileData;

        public StinkyLevel OpenLevel { get; private set; }

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
        public StinkyParser(string FilePath)
        {
            this.FilePath = FilePath;
            FindAllLevels();
            //Refresh(true);
        }

        private void FindAllLevels()
        {
            fileData = File.ReadAllBytes(FilePath);
            var root = new XElement("root");
            for (int index = 0; index < fileData.Length; index++)
            {
                var Byte = fileData[index];
                if (Byte == 0x53 && fileData.Skip(index + 0xF).Take(1).First() == 0x65 && fileData.Skip(index + 0x1A).Take(1).First() == 0x35)
                {
                    var name = fileData.Skip(index + 0x28).Take(0x1E).ToArray();
                    var text = Encoding.ASCII.GetString(name).Trim();
                    LevelIndices.Add(index, text);
                    if (LevelIndices.Count == 65) break;
                    root.Add(new XElement("Level",
                        new XElement("Location", index),
                        new XElement("Name", text)));
                    index += 0x56;
                }
            }
            var doc = new XDocument();
            doc.Add(root);
            //doc.Save("leveldb.xml");
        }

        /// <summary>
        /// Reads a level at the specified index in a DAT file -- Sets the level as <see cref="OpenLevel"/> See: <see cref="LevelIndices"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StinkyLevel LevelRead(int index) => OpenLevel = GetLevelByIndex(index);

        /// <summary>
        /// Returns the level data for the level at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public StinkyLevel GetLevelByIndex(int index) => new StinkyLevel(this, fileData, index);

        /// <summary>
        /// Refreshes the parser
        /// </summary>
        public void Refresh()
        {
            RefreshLevel(OpenLevel);
        }

        internal void RefreshLevel(StinkyLevel level)
        {
            int skip = BitRead;
            var data = fileData.Skip(level.LevelContentIndex).ToArray();
            List<LevelDataBlock> blocks = new List<LevelDataBlock>();
            List<LevelDataBlock> decor = new List<LevelDataBlock>();
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
                            var block = LevelDataBlock.LoadFromDatabase(current, BlockLayers.Integral, out bool success);
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
                            var block = LevelDataBlock.LoadFromDatabase(current, BlockLayers.Decoration, out bool success);
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
            using (var stream = File.OpenWrite("leveldump.dat"))
                stream.Write(fileData, level.LevelHeaderIndex, level.LevelHeaderIndex + index);
            bitSkipChanged = false;
            pathChanged = false;
        }
    }
}
