using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{    
    public class BlockParameter
    {
        public static String ParameterDBPath => LevelDataBlock.ParameterDatabasePath;
        public static Dictionary<string, string> ParameterDBDescriptions = new Dictionary<string, string>(); 
        public static void LoadParameterDB()
        {
            if (!ParameterDBDescriptions.Any())
            {
                var database = XDocument.Load(LevelDataBlock.ParameterDatabasePath);
                foreach (var name in database.Root.Elements())
                {
                    ParameterDBDescriptions.Add(name.Element("Name").Value, name.Element("Summary").Value ?? "None given");
                }
            }
        }

        public BlockParameter()
        {
            
        }

        public string Value { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// The element in the BlockDB where the Block's data lives
        /// </summary>
        /// <param name="DataElement"></param>
        /// <returns></returns>
        public static Dictionary<string, BlockParameter> LoadParams(XElement DataElement)
        {
            Dictionary<string, BlockParameter> source = new Dictionary<string, BlockParameter>();
            if (DataElement.Element("Parameters") == null) return source;
            foreach (var element in DataElement.Element("Parameters").Elements())
            {
                var name = element.Element("Name").Value;
                var value = element.Element("Value").Value;
                source.Add(name, new BlockParameter() { Name = name, Value = value });
            }
            return source;
        }

        public void Save(XElement DataElement)
        {
            if (DataElement.Element("Parameters") == null)
                DataElement.Add(new XElement("Parameters"));
            var home = DataElement.Element("Parameters");
            home.Add(new XElement("Parameter",
                        new XElement("Name", Name),
                        new XElement("Value", Value)));
        }
    }
}
