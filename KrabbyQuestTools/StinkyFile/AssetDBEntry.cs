using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    /// <summary>
    /// Types of assets the game can reference
    /// </summary>
    public enum AssetType
    {
        /// <summary>
        /// PNG, JPG, BMP files fall into this category
        /// </summary>
        Texture, 
        /// <summary>
        /// WAV files 
        /// </summary>
        Sound, 
        /// <summary>
        /// B3D files cannot be used, use the converted OBJ/FBX models instead.
        /// </summary>
        Model
    }
    /// <summary>
    /// Interfaces with the AssetDB to manipulate and create entries.
    /// </summary>
    public class AssetDBEntry
    {
        /// <summary>
        /// The path to the AssetDB. Make sure this is accurate by setting it to a correct file before using this class.
        /// </summary>
        public static string AssetDatabasePath { get; set; } = "Resources/texturedb.xml";
        private string _guid;

        /// <summary>
        /// The path to the file -- relative path from the WorkspaceDirectory set in the AssetDB. 
        /// <para>Failing to follow this procedure will cause issues loading content in KrabbyQuestEmu</para>
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// The DB-Friendly name this asset uses
        /// </summary>
        public string DBName { get; set; } = null;

        /// <summary>
        /// The unique GUID to this db entry
        /// </summary>
        public string GUID { get => _guid; 
            private set
            {
                if (GUID != value)
                    GuidMaker.Free(GUID);
                _guid = value;
                GuidMaker.Reserve(value);
            }
        }

        /// <summary>
        /// The type of asset the game recognizes this to be
        /// </summary>
        public AssetType Type { get; set; }

        /// <summary>
        /// The <see cref="LevelDataBlock"/> that reference this <see cref="AssetDBEntry"/>
        /// </summary>
        public List<LevelDataBlock> ReferencedDataBlocks
        {
            get; private set;
        } = new List<LevelDataBlock>();

        /// <summary>
        /// Creates a new <see cref="AssetDBEntry"/> with a random, unused GUID.
        /// <para>See: <see cref="GuidMaker"/></para>
        /// </summary>
        public AssetDBEntry() : this(GuidMaker.GetNextGuid('A'))
        {

        }

        /// <summary>
        /// Creates a new <see cref="AssetDBEntry"/> with the specified GUID
        /// </summary>
        /// <param name="GUID"></param>
        public AssetDBEntry(string GUID)
        {
            this.GUID = GUID;
        }

        /// <summary>
        /// Adds/Edits the <c>WorkspaceDirectory</c> key in the AssetDB
        /// </summary>
        /// <param name="WorkspaceDir">The path to the Workspace directory</param>
        public static void PushWorkspaceDir(string WorkspaceDir, string AssetDatabasePath = default)
        {
            if (AssetDatabasePath == default)
                AssetDatabasePath = AssetDBEntry.AssetDatabasePath;
            var database = XDocument.Load(AssetDatabasePath);
            if (database.Root.Element("WorkspaceDirectory") != null)
                database.Root.Element("WorkspaceDirectory").Remove();
            database.Root.Add(new XElement("WorkspaceDirectory", WorkspaceDir));
            database.Save(AssetDatabasePath);
        }

        /// <summary>
        /// Gets the DB-Friendly name of the asset from it's referenced filename.
        /// </summary>
        /// <param name="FileName">The file name to use, see <see cref="FileName"/> for usage.</param>
        /// <returns></returns>
        public static string GetDBNameFromFileName(string FileName)
        {
            var database = XDocument.Load(AssetDatabasePath);
            var element = database.Root.Elements().Where(
                x => x?.Element("FilePath")?.Value == FileName).FirstOrDefault();
            if (element != null)
                return element.Element("Name").Value;
            else
                return Path.GetFileName(FileName);
        }

        /// <summary>
        /// Saves this <see cref="AssetDBEntry"/> to the AssetDB referenced by <see cref="AssetDatabasePath"/>
        /// </summary>
        public void Save()
        {
            var database = XDocument.Load(AssetDatabasePath);
            database.Save(AssetDatabasePath + ".bak");
            database.Root.Element(GUID)?.Remove();
            database.Root.Add(new XElement(GUID,
                new XElement("FilePath", FileName),
                new XElement("Name", DBName),
                new XElement("AssetType", Enum.GetName(typeof(AssetType),Type)),
                new XElement("References", string.Join(",", ReferencedDataBlocks.Select(x => x.GUID)))));
            database.Save(AssetDatabasePath);
            foreach (var block in ReferencedDataBlocks)
            {
                block.AssetReferences.Add((GUID, Type));
                block.SaveToDatabase();
            }
        }

        /// <summary>
        /// Loads the AssetDB from the filepath provided
        /// </summary>
        /// <param name="Filename">The relative file name of the file to open</param>
        /// <param name="Created">Represents whether the AssetDB did not contain an entry for this file and thus has created one for you to edit.</param>
        /// <returns></returns>
        public static AssetDBEntry LoadFromFileName(string Filename, out bool Created)
        {
            var database = XDocument.Load(AssetDatabasePath);
            var element = database.Root.Elements().Where(
                x => x?.Element("FilePath")?.Value == Filename).FirstOrDefault()?.Name.LocalName;
            Created = false;
            if (element != null)
                return Load(element);
            else
            {
                Created = true;
                return new AssetDBEntry()
                {
                    FileName = Filename
                };                
            }
        }

        /// <summary>
        /// Loads the <see cref="AssetDBEntry"/> from the GUID provided
        /// </summary>
        /// <param name="Guid">The GUID of the asset db entry</param>
        /// <param name="LoadReferences">Specifes whether to load the <see cref="ReferencedDataBlocks"/> property -- default: true</param>
        /// <returns></returns>
        public static AssetDBEntry Load(string Guid, bool LoadReferences = true)
        {            
            var database = XDocument.Load(AssetDatabasePath);
            var element = database.Root.Element(Guid);
            if (element != null)
            {
                var dbe = new AssetDBEntry(Guid)
                {
                    FileName = element.Element("FilePath").Value,
                    DBName = element.Element("Name").Value,    
                    Type = (AssetType)Enum.Parse(typeof(AssetType), element.Element("AssetType").Value)
                };
                if (LoadReferences)
                    foreach (var guid in element.Element("References").Value.Split(','))
                    {
                        if (string.IsNullOrWhiteSpace(guid))
                            continue;
                        dbe.ReferencedDataBlocks.Add(LevelDataBlock.LoadFromDatabase(guid, out _));
                    }
                return dbe;
            }
            else return null;
        }
    }
}
