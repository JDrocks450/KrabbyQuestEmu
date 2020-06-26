using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public class BlockDBRepair
    {
        public static void FixBlockLevel()
        {
            var database = XDocument.Load("blockdb.xml");
            database.Save("blockdb_fix.xml.bak");
            foreach(var element in database.Root.Elements())
            {                
                if (element.Element("Package").Value == "0" && element.Element("ItemId").Value != "0")
                {
                    foreach (var duplicate in element.Elements().Where(x => x.Name == "Level"))
                        duplicate.Remove();
                    element.Add(new XElement("Level", "Decoration"));
                }
            }
            database.Save("blockdb.xml");
        }
    }
}
