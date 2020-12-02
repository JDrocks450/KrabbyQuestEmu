using StinkyFile.Primitive;
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
        /// <summary>
        /// The amount of bytes the level data takes up
        /// </summary>
        public int ByteSize => Rows * Columns * 4;
        /// <summary>
        /// The header, is expected to always be level file v5
        /// </summary>
        public string Header { get; private set; }
        [EditorVisible("The version of the level loaded")]
        /// <summary>
        /// The version of the level loaded
        /// </summary>
        public int LevelFileVersion
        {
            get; private set;
        }
        [EditorVisible("The name of the Level, included in the header data")]
        /// <summary>
        /// The name of the Level, included in the header data
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The amount of signs supported by the level file version
        /// </summary>
        public int SupportedSignAmount { get; private set; } = 20;

        private StinkyParser Parent;

        /// <summary>
        /// The tile data that is Integral to the level layout
        /// </summary>
        public LevelDataBlock[] IntegralData;

        /// <summary>
        /// Any objects that are placed above Integral tiles
        /// </summary>
        public LevelDataBlock[] DecorationData;

        /// <summary>
        /// The sign data in the world.
        /// <para>Stinky and Loof v5+ have 20 signs</para>
        /// </summary>
        public string[] Messages;

        [EditorVisible("The FileName of the level")]
        /// <summary>
        /// The FileName of the level
        /// </summary>
        public string LevelFilePath;

        [EditorVisible("Each level has a unique ID")]
        /// <summary>
        /// Each level has a unique ID
        /// </summary>
        public int ID
        {
            get; set;
        }
        [EditorVisible("The developer name for worlds, usually the ordered index of the level in the world. \n" +
            "For all intents and purposes, this is identical to the level's FileName")]
        /// <summary>
        /// The developer name for worlds, usually the ordered index of the level in the world.
        /// <para>For all intents and purposes, this is identical to the level's FileName</para>
        /// </summary>
        public string LevelWorldName
        {
            get; set;
        }

        /// <summary>
        /// The point in the file data where the object data begins
        /// </summary>
        internal int LevelContentIndex;

        [EditorVisible("The Height, in Tiles, of the World")]
        /// <summary>
        /// The Height, in Tiles, of the World
        /// </summary>
        public int Rows { get; set; }
        [EditorVisible("The Width, in Tiles, of the World")]
        /// <summary>
        /// The Width, in Tiles, of the World
        /// </summary>
        public int Columns { get; set; }  
        [EditorVisible]
        /// <summary>
        /// The total amount of tiles on either level of the World.
        /// <para><code>Rows * Columns</code></para>
        /// </summary>
        public int Total => IntegralData?.Length ?? 0;
        [EditorVisible]
        /// <summary>
        /// The context this level uses (LevelTexture) 
        /// <para>This dictates the style of the objects in the world.</para>
        /// </summary>
        public LevelContext Context => (LevelContext)LevelParameters[(int)ParameterDefinitions.CONTEXT_LEVEL_TEXTURE];
        [EditorVisible]
        /// <summary>
        /// The context this level uses. 
        /// <para>This dictates the style of the objects in the world.</para>
        /// </summary>
        public int LevelBackground => (int)LevelParameters[(int)ParameterDefinitions.LEVEL_BG];
        [EditorVisible("The time you have to complete the level")]
        /// <summary>
        /// The time you have to complete the level
        /// </summary>
        public int LevelTime
        {
            get; set;
        }
        [EditorVisible(3)]
        /// <summary>
        /// The music the level is intended to use        
        /// </summary>
        public int LevelMusic => (int)LevelParameters[(int)ParameterDefinitions.LEVEL_MUSIC];
        [EditorVisible("The level's custom model, if applicable", 5)]
        /// <summary>
        /// The level's custom model, if applicable
        /// </summary>
        public string CustomModel => (string)LevelParameters[(int)ParameterDefinitions.CUSTOM_MODEL];
        [EditorVisible("The level's custom house, if applicable", 5)]
        /// <summary>
        /// The level's custom house, if applicable
        /// </summary>
        public string CustomHouse => (string)LevelParameters[(int)ParameterDefinitions.CUSTOM_HOUSE];
        [EditorVisible(6)]
        /// <summary>
        /// LV6 compatibility
        /// </summary>
        public string CustomTexture
        {
            get; private set;
        }
        [EditorVisible(6)]
        /// <summary>
        /// LV6 compatibility
        /// </summary>
        public string CustomBackground
        {
            get; private set;
        }

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
        public StinkyLevel(StinkyParser parent, string LevelPath) : this(parent, File.ReadAllBytes(LevelPath), 0)
        {            
            LevelFilePath = LevelPath;         
        }

        internal StinkyLevel()
        {

        }
        
        /// <summary>
        /// Load from a file at a specific index
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="FileData"></param>
        /// <param name="index"></param>
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
            Header = FileTools.ReadString(data, Position, out Position); // header data
            LevelWorldName = FileTools.ReadString(data, Position, out Position); // level filename
            LevelParameters[8] = LevelWorldName; // enables the editor to display this data field
            ID = FileTools.ReadInt(data, Position, out Position);
            Name = FileTools.ReadString(data, Position, out Position); // the name of the level
            SupportedSignAmount = 5;
            switch (Header)
            {
                case "Stinky & Loof Level File v3": // LV3 [UNSUPPORTED]
                    LevelFileVersion = 3;
                    LevelTime = FileTools.ReadInt(data, Position, out Position); // level time
                    break;
                case "Stinky & Loof Level File v5": // LV5 [SUPPORTED]
                    PopulateLV5(data, ref Position);
                    LevelFileVersion = 5;
                    break;
                case "Stinky & Loof Level File v6": // LV6 [SUPPORTED]
                    PopulateLV5(data, ref Position);
                    _ = FileTools.ReadInt(data, Position, out Position); // dummy
                    CustomTexture = FileTools.ReadString(data, Position, out Position);
                    _ = FileTools.ReadInt(data, Position, out Position); // dummy
                    CustomBackground = FileTools.ReadString(data, Position, out Position);
                    LevelFileVersion = 6;
                    break;
            }                        
            LevelParameters[6] = FileTools.ReadInt(data, Position, out Position); // level texture
            LevelParameters[7] = FileTools.ReadInt(data, Position, out Position); // level background
            Columns = FileTools.ReadInt(data, Position, out Position); 
            Rows = FileTools.ReadInt(data, Position, out Position); 
            LevelContentIndex = Position;
            Position += (ByteSize * 2); // jump to file footer data where messages are stored
            var messages = new string[SupportedSignAmount];
            for (int i = 0; i < SupportedSignAmount; i++)
            {
                string message = FileTools.ReadString(data, Position, out Position);
                message = message.Replace("#", "\n"); // replace # with new line
                messages[i] = message;
            }
            Messages = messages.ToArray();
            LevelParameters[1] = FileTools.ReadInt(data, Position, out Position); // level music
        }

        private void PopulateLV5(byte[] data, ref int Position)
        {
            SupportedSignAmount = 20;
            LevelParameters[2] = FileTools.ReadInt(data, Position, out Position); // likely nothing
            LevelParameters[3] = FileTools.ReadString(data, Position, out Position); // custom house
            LevelParameters[4] = FileTools.ReadInt(data, Position, out Position); // likely nothing
            LevelParameters[5] = FileTools.ReadString(data, Position, out Position); // custom model
            LevelTime = FileTools.ReadInt(data, Position, out Position); // level time
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
                TimeRemaining = 0
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
