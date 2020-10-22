using StinkyFile.Save;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace StinkyFile
{
    public partial class StinkyLevel
    {
        public int ByteSize => Rows * Columns * 4;
        public string Header { get; private set; }
        public string Name { get; private set; }

        private StinkyParser Parent;

        /// <summary>
        /// The tile data that is Integral to the level layout
        /// </summary>
        public LevelDataBlock[] IntegralData;
        /// <summary>
        /// Any objects that are placed above Integral tiles
        /// </summary>
        public LevelDataBlock[] DecorationData;
        public string[] Messages;

        public string LevelFilePath;

        /// <summary>
        /// The developer name for worlds, usually the ordered index of the level in the world.
        /// </summary>
        public string LevelWorldName;
        public int LevelContentIndex;
        public int Rows { get; set; }
        public int Columns { get; set; }        
        public int Total => IntegralData?.Length ?? 0;
        public LevelContext Context { get; private set; }

        /// <summary>
        /// Is this level currently loaded
        /// </summary>
        public bool IsLoaded
        {
            get; set;
        } = false;

        /// <summary>
        /// Level parameters stored in the header information
        /// </summary>
        public object[] LevelParameters = new object[9];

        /// <summary>
        /// Loads a <see cref="StinkyLevel"/> from the file data provided
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="FileData"></param>
        public StinkyLevel(StinkyParser parent, string LevelPath)
        {
            LevelFilePath = LevelPath;
            var data = File.ReadAllBytes(LevelPath);
            LoadFromDat(data, 0);            
        }

        internal StinkyLevel()
        {

        }

        [Obsolete]
        public StinkyLevel(StinkyParser parent, byte[] FileData, int index)
        {
            Parent = parent;      
            LoadFromDat(FileData, index);
        }        

        /// <summary>
        /// Loads the level specific data before <see cref="StinkyParser"/> refreshes the level object data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        internal void LoadFromDat(byte[] data, int index)
        {                      
            int Position = index; // keeps track of the current file index
            int headerLength = BitConverter.ToInt32(data, Position); // the length of the header... should be 27!            
            Position += 4;
            Header = Encoding.ASCII.GetString(data, Position, headerLength); // extra debugging information
            Position += headerLength;
            int LevelIndexLength = BitConverter.ToInt32(data, Position);
            Position += 4;
            LevelWorldName = Encoding.ASCII.GetString(data, Position, LevelIndexLength);
            LevelParameters[8] = LevelWorldName; // enables the editor to display this data field
            Position += LevelIndexLength;
            LevelParameters[1] = BitConverter.ToInt32(data, Position);
            Position += 4;
            int TitleLength = BitConverter.ToInt32(data, Position);
            Position += 4;
            Name = Encoding.ASCII.GetString(data, Position, TitleLength); // the name of the level
            Position += TitleLength;
            LevelParameters[2] = BitConverter.ToInt32(data, Position);
            Position += 4;
            LevelParameters[3] = BitConverter.ToInt32(data, Position);
            Position += 4;
            LevelParameters[4] = BitConverter.ToInt32(data, Position);
            Position += 4;
            LevelParameters[5] = BitConverter.ToInt32(data, Position);
            Position += 4;
            Position += 4; // comma separator
            LevelParameters[6] = BitConverter.ToInt32(data, Position); // Big endian conversion
            Position += 4;
            LevelParameters[7] = BitConverter.ToInt32(data, Position); // level parameter?
            Position += 4;
            Columns = BitConverter.ToInt32(data, Position);
            Position += 4;
            Rows = BitConverter.ToInt32(data, Position);
            Position += 4;
            LevelContentIndex = Position;
            Context = (LevelContext)LevelParameters[(int)ParameterDefinitions.CONTEXT];
            Position += (ByteSize * 2); // jump to file footer data where messages are stored
            var messages = new List<string>();
            while (true)
            {
                if (Position >= data.Length - 1)
                    break;
                int length = BitConverter.ToInt32(data, Position);
                Position += 4;
                if (data.Length <= Position + length)
                    break;                
                if (length < 1000 && length > 0)
                {
                    string message = Encoding.ASCII.GetString(data, Position, length);
                    message = message.Replace("#", "\n"); // replace # with new line
                    messages.Add(message);                    
                }
                else
                {
                    messages.Add(null);
                    continue;
                }
                Position += length;
                
            }
            Messages = messages.ToArray();
        }

        /// <summary>
        /// Returns the <see cref="LevelCompletionInfo"/> for this level, or creates one if one does not exist
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public LevelCompletionInfo GetSaveFileInfo(SaveFile file)
        {
            if (file.LevelInfo.TryGetValue(LevelWorldName, out var value))
                return value;
            var newInfo = new LevelCompletionInfo()
            {
                LevelWorldName = LevelWorldName,
                LevelName = Name,
                TimeRemaining = 300000
            };
            file.UpdateInfo(newInfo);
            return newInfo;
        }

        public void SaveAll()
        {
            Parent.CacheSaveAll();
        }
    }
}

#if false
var headerData = data.Skip(index + headerLength).Take(0x1E).ToArray();
            var text = Encoding.ASCII.GetString(headerData).Trim(); // get level name    
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
#endif
