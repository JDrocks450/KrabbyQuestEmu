using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public enum AssetType
    {
        Texture, Sound
    }
    public class AssetDBEntry
    {
        private const string ASSET_DB_PATH = "Resources/texturedb.xml";
        private string _guid;
        public string FilePath { get; private set; }
        public string DBName { get; set; } = "Untitled";
        public string GUID { get => _guid; 
            private set
            {
                if (GUID != value)
                    GuidMaker.Free(GUID);
                _guid = value;
                GuidMaker.Reserve(value);
            }
        }
        public AssetType Type { get; set; }
        public List<LevelDataBlock> ReferencedDataBlockGuids
        {
            get; private set;
        } = new List<LevelDataBlock>();

        public AssetDBEntry() : this(GuidMaker.GetNextGuid('A'))
        {

        }
        public AssetDBEntry(string GUID)
        {
            this.GUID = GUID;
        }

        public static string GetDBNameFromFileName(string FileName)
        {
            var database = XDocument.Load(ASSET_DB_PATH);
            var element = database.Root.Elements().Where(
                x => x.Element("FilePath").Value == FileName).FirstOrDefault();
            if (element != null)
                return element.Element("Name").Value;
            else
                return Path.GetFileName(FileName);
        }

        public void Save()
        {
            var database = XDocument.Load(ASSET_DB_PATH);
            database.Save(ASSET_DB_PATH + ".bak");
            database.Root.Element(GUID)?.Remove();
            database.Root.Add(new XElement(GUID,
                new XElement("FilePath", FilePath),
                new XElement("Name", DBName),
                new XElement("AssetType", Enum.GetName(typeof(AssetType),Type)),
                new XElement("References", string.Join(",", ReferencedDataBlockGuids.Select(x => x.GUID)))));
            database.Save("texturedb.xml");
            foreach (var block in ReferencedDataBlockGuids)
            {
                block.AssetReferences.Add((GUID, Type));
                block.SaveToDatabase();
            }
        }

        public static AssetDBEntry LoadFromFilePath(string FilePath)
        {
            var database = XDocument.Load(ASSET_DB_PATH);
            var element = database.Root.Elements().Where(
                x => x.Element("FilePath").Value == FilePath).FirstOrDefault()?.Name.LocalName;
            if (element != null)
                return Load(element);
            else
                return new AssetDBEntry()
                {
                    FilePath = FilePath
                };
        }

        public static AssetDBEntry Load(string Guid, bool LoadReferences = true)
        {            
            var database = XDocument.Load(ASSET_DB_PATH);
            var element = database.Root.Element(Guid);
            if (element != null)
            {
                var dbe = new AssetDBEntry(Guid)
                {
                    FilePath = element.Element("FilePath").Value,
                    DBName = element.Element("Name").Value,    
                    Type = (AssetType)Enum.Parse(typeof(AssetType), element.Element("AssetType").Value)
                };
                if (LoadReferences)
                    foreach (var guid in element.Element("References").Value.Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(guid))
                            continue;
                        dbe.ReferencedDataBlockGuids.Add(LevelDataBlock.LoadFromDatabase(guid, out _));
                    }
                return dbe;
            }
            else return null;
        }
    }
}
