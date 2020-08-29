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
        public static void BlockDB_ForceFixRotations() => BlockDB_FixRotations(true);
    }
}
