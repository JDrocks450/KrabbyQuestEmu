using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StinkyFile
{
    /// <summary>
    /// The layer the objects exist on based on location in file
    /// </summary>
    public enum BlockLayers
    {
        /// <summary>
        /// The base layer of the level -- Walls, Floors, Gates, etc.
        /// </summary>
        Integral,
        /// <summary>
        /// Patties? Flowers, Rocks, etc.
        /// </summary>
        Decoration
    }
    public class LevelDataBlock
    {
        public static string BlockDatabasePath { get; set; } = "Resources/blockdb.xml";
        public static string ParameterDatabasePath { get; set; } = "Resources/parameterdb.xml";

        private static Dictionary<byte, S_Color> KnownColors = new Dictionary<byte, S_Color>();
       
        private static Random rand = new Random();

        public const int RAW_DATA_SIZE = 4;
        /// <summary>
        /// Always <see cref="RAW_DATA_SIZE"/> bytes in size -- the data stored to file representing this grid square
        /// </summary>
        public byte[] RawData;
        private string _guid;
        public BlockLayers BlockLayer { get; 
            set; }
        public byte ItemId => RawData[0];
        public byte Group => RawData[1];
        public byte GroupId => RawData[2];
        public string GUID { get => _guid; 
            private set
            {
                if (GUID != value)
                    GuidMaker.Free(GUID);
                _guid = value;
                GuidMaker.Reserve(value);
            }
        }
        public string Name { get; set; }
        public S_Color Color { get; set; }

        public HashSet<(string guid, AssetType type)> AssetReferences = new HashSet<(string guid, AssetType type)>();

        public bool HasTexture => AssetReferences.FirstOrDefault(x => x.type == AssetType.Texture) != default;
        public bool HasModel => AssetReferences.FirstOrDefault(x => x.type == AssetType.Model) != default;
        public bool HasSound => AssetReferences.FirstOrDefault(x => x.type == AssetType.Sound) != default;
        public SRotation Rotation { get; set; }
        public IEnumerable<BlockParameter> Parameters
        {
            get; set;
        } = new List<BlockParameter>();        

        public LevelDataBlock(byte[] RawData, BlockLayers Layer = BlockLayers.Integral)
        {
            BlockLayer = Layer;
            this.RawData = RawData;
            _guid = GuidMaker.GetNextGuid();
            GuidMaker.Reserve(GUID);
            var id = (BlockLayer == BlockLayers.Integral) ? Group : ItemId;
            if (id == 0 && BlockLayer == BlockLayers.Decoration)
            {
                Color = new S_Color(0, 0, 0, 0);
            }
            else
            {
                if (!KnownColors.ContainsKey(id))
                    KnownColors.Add(id, new S_Color((byte)rand.Next(0, 256), (byte)rand.Next(0, 256), (byte)rand.Next(0, 256)));
                Color = KnownColors[id];
            }
        }

        public bool SaveToDatabase()
        {
            var database = XDocument.Load(BlockDatabasePath);
            database.Save(BlockDatabasePath + ".bak");
            database.Root.Element(GUID)?.Remove();
            var element = new XElement(GUID,
                new XElement("Name", Name),
                new XElement("Package", Group),
                new XElement("PackId", GroupId),
                new XElement("ItemId", RawData[0]),
                new XElement("Dat2", RawData[3]),
                new XElement("Color", Color.ToString()),
                new XElement("Rotation", Enum.GetName(typeof(SRotation), Rotation)),
                new XElement("Level", Enum.GetName(typeof(BlockLayers), BlockLayer)));
            foreach (var param in Parameters)
                param.Save(element);
            database.Root.Add(element);
            var assetNode = new XElement("AssetReferences");
            element.Add(assetNode);
            foreach (var asset in AssetReferences)
                assetNode.Add(new XElement(asset.guid, Enum.GetName(typeof(AssetType), asset.type)));                                
            database.Save(BlockDatabasePath);
            return true;
        }

        public LevelDataBlock RefreshFromDatabase(StinkyParser Parser)
        {
            var dbVersion = Parser.CacheRefresh(GUID);
            if (dbVersion == null) //doesnt exist yet!
                return this;
            return dbVersion;
        }

        public bool GetParameterByName(string Name, out BlockParameter Data)
        {
            var value = Parameters.FirstOrDefault(x => x.Name.ToLower() == Name.ToLower());
            Data = value;
            if (value != default) return true;
                return false;
        }

        public AssetDBEntry GetFirstTextureAsset() => GetReference(AssetReferences.FirstOrDefault(x => x.type == AssetType.Texture).guid);

        public AssetDBEntry GetReference(string guid)
        {
            if (guid == null) return null;
            return AssetDBEntry.Load(guid, false);
        }

        public IEnumerable<AssetDBEntry> GetReferences(AssetType Constraint)
        {
            List<AssetDBEntry> list = new List<AssetDBEntry>();
            foreach (var guid in AssetReferences.Where(x => x.type == Constraint))
                list.Add(GetReference(guid.guid));
            return list;
        }

        /// <summary>
        /// Loads the LevelDataBlock from the Database file with the specified GUID
        /// </summary>
        /// <param name="Guid">The GUID to search for</param>
        /// <param name="success">Whether the block was found or not</param>
        /// <returns></returns>
        public static LevelDataBlock LoadFromDatabase(string Guid, out bool success)
        {
            var database = XDocument.Load(BlockDatabasePath);
            var element = database.Root.Element(Guid);
            if (element == null)
            {
                success = false;
                return default;
            }
            var idElement = element.Element("ItemId"); // sometimes saved as Dat1
            if (idElement == null)
                idElement = element.Element("Dat1");
            var itemId = byte.Parse(idElement.Value);
            var package = byte.Parse(element.Element("Package").Value);
            var packId = byte.Parse(element.Element("PackId").Value);
            var dat2 = byte.Parse(element.Element("Dat2").Value);
            var rotate = element.Element("Rotation")?.Value ?? "NORTH";            
            HashSet<(string guid, AssetType type)> assets = new HashSet<(string guid, AssetType type)>();
            var assetNode = element.Element("AssetReferences");            
            if (assetNode != null)
                foreach (var asset in assetNode.Elements())
                    assets.Add((asset.Name.LocalName, (AssetType)Enum.Parse(typeof(AssetType), asset.Value)));            
            success = true;
            return new LevelDataBlock(new byte[] { itemId, package, packId, dat2 })
            {
                Name = element.Element("Name").Value,
                GUID = element.Name.LocalName,
                Color = S_Color.Parse(element.Element("Color").Value),
                BlockLayer = element.Element("Level")?.Value == "Decoration" ? BlockLayers.Decoration : BlockLayers.Integral,
                AssetReferences = assets,
                Rotation = (SRotation)Enum.Parse(typeof(SRotation), rotate),
                Parameters = BlockParameter.LoadParams(element)
            };
        }

        public static LevelDataBlock[] LoadAllFromDB()
        {
            var database = XDocument.Load(BlockDatabasePath);
            List<LevelDataBlock> blocks = new List<LevelDataBlock>();
            foreach(var element in database.Root.Elements())
            {
                blocks.Add(LoadFromDatabase(element.Name.LocalName, out bool success));
            }
            return blocks.ToArray();
        }

        public static LevelDataBlock LoadFromDatabase(byte[] RawData, BlockLayers Layer, out bool success)
        {
            success = false;
            var database = XDocument.Load(BlockDatabasePath);
            foreach (var element in database.Root.Elements())
            {
                switch (Layer)
                {
                    case BlockLayers.Integral:
                        if (element.Element("Package").Value == RawData[1].ToString()
                            && element.Element("PackId").Value == RawData[2].ToString())
                        {
                            success = true;
                            var block = LevelDataBlock.LoadFromDatabase(element.Name.LocalName, out _);
                            return block;
                        }
                        break;
                    case BlockLayers.Decoration:
                        if (element.Element("ItemId")?.Value == RawData[0].ToString() && element.Element("Level")?.Value == "Decoration")
                        {
                            success = true;
                            var block = LevelDataBlock.LoadFromDatabase(element.Name.LocalName, out _);
                            return block;
                        }
                        break;
                }
                
            }            
            return new LevelDataBlock(RawData, Layer);
        }
    }
}
