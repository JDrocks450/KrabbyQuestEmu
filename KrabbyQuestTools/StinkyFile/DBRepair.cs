using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public class DBRepair
    {
        [Obsolete]
        /// <summary>
        /// An Obsolete fix that has little usage anymore -- Don't use it.
        /// </summary>
        public static void BlockDB_FixBlockLevel()
        {
            var database = XDocument.Load(LevelDataBlock.BlockDatabasePath);
            database.Save("Resources/blockdb_fix.xml.bak");
            foreach(var element in database.Root.Elements())
            {                
                if (element.Element("Package").Value == "0" && element.Element("ItemId").Value != "0")
                {
                    foreach (var duplicate in element.Elements().Where(x => x.Name == "Level"))
                        duplicate.Remove();
                    element.Add(new XElement("Level", "Decoration"));
                }
            }
            database.Save(LevelDataBlock.BlockDatabasePath);
        }

        /// <summary>
        /// Populates the Parameter Database with all applied parameters in the BlockDB -- Clears existing elements!
        /// </summary>
        public static void ParameterDB_Populate()
        {
            HashSet<string> ParamNames = new HashSet<string>();
            var database = XDocument.Load(LevelDataBlock.BlockDatabasePath);
            foreach(var element in database.Root.Elements())
            {
                var thisElement = element.Element("Parameters");
                if (thisElement == null) continue;
                foreach(var param in thisElement.Elements())
                {
                    var paramName = param.Element("Name").Value;
                    if (!ParamNames.Contains(paramName))
                        ParamNames.Add(paramName);
                }
            }
            database = XDocument.Load(LevelDataBlock.ParameterDatabasePath);
            database.Root.RemoveAll();
            foreach(var name in ParamNames)
            {
                database.Root.Add(new XElement("Parameter",
                    new XElement("Name", name),
                    new XElement("Summary", null)));
            }
            database.Save(LevelDataBlock.ParameterDatabasePath);
        }

        /// <summary>
        /// Deletes all AssetReferences from the BlockDB
        /// </summary>
        public static void BlockDB_ClearReferences()
        {
            var database = XDocument.Load(LevelDataBlock.BlockDatabasePath);
            database.Save("Resources/blockdb_fix.xml.bak");
            foreach (var element in database.Root.Elements())
            {
                foreach (var duplicate in element.Elements().Where(x => x.Name == "AssetReferences"))
                    duplicate.RemoveAll();
            }
            database.Save(LevelDataBlock.BlockDatabasePath);
        }

        [Obsolete]
        /// <summary>
        /// An Obsolete fix that trims AssetDB filepath variables to just the FileName -- Don't use it.
        /// </summary>
        public static void AssetDB_FixFilePaths()
        {
            var database = XDocument.Load(AssetDBEntry.AssetDatabasePath);
            database.Save("Resources/texturedb_fix.xml.bak");
            foreach(var element in database.Root.Elements())
            {
                element.Element("FilePath").Value = Path.GetFileName(element.Element("FilePath").Value);
            }
            database.Save(AssetDBEntry.AssetDatabasePath);
        }

        /// <summary>
        /// Applies Rotation = NORTH to all BlockDB entries if they have no preset Rotation
        /// </summary>
        /// <param name="force"></param>
        public static void BlockDB_FixRotations(bool force = false)
        {
            var database = XDocument.Load(LevelDataBlock.BlockDatabasePath);
            database.Save("Resources/blockdb_fix.xml.bak");
            foreach (var element in database.Root.Elements())
            {
                if (force)
                {
                    var array = element.Elements("Rotation").ToArray();
                    foreach (var delete in array)
                        delete.Remove();
                }
                if (element.Element("Rotation") == null)
                {
                    var rotation = "NORTH";
                    var Name = element.Element("Name").Value;
                    if (Name.EndsWith("_N")) // North
                    {

                    }
                    else if (Name.EndsWith("_S")) //south
                    {
                        rotation = "SOUTH";
                    }
                    else if (Name.EndsWith("_E")) //east
                    {
                        rotation = "EAST";
                    }
                    else if (Name.EndsWith("_W")) // west 
                    {
                        rotation = "WEST";
                    }
                    element.Add(new XElement("Rotation", rotation));
                }
            }
            database.Save(LevelDataBlock.BlockDatabasePath);
        }
        
        /// <summary>
        /// Forces all BlockDB entries to have Rotation = NORTH -- Dangerous!
        /// </summary>
        public static void BlockDB_ForceFixRotations() => BlockDB_FixRotations(true);        
    }
}
