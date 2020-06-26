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
        public static void FixBlockLevel()
        {
            var database = XDocument.Load(LevelDataBlock.BLOCK_DB_PATH);
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
            database.Save(LevelDataBlock.BLOCK_DB_PATH);
        }

        public static void AssetDB_FixFilePaths()
        {
            var database = XDocument.Load(AssetDBEntry.ASSET_DB_PATH);
            database.Save("Resources/texturedb_fix.xml.bak");
            foreach(var element in database.Root.Elements())
            {
                element.Element("FilePath").Value = Path.GetFileName(element.Element("FilePath").Value);
            }
            database.Save(AssetDBEntry.ASSET_DB_PATH);
        }
    }
}
