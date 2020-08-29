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
        public const String ParameterDBPath = "Resources/paramdb.xml";

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
        public static IEnumerable<BlockParameter> LoadParams(XElement DataElement)
        {
            if (DataElement.Element("Parameters") == null) yield break;
            foreach (var element in DataElement.Element("Parameters").Elements())
            {
                var name = element.Element("Name").Value;
                var value = element.Element("Value").Value;
                yield return new BlockParameter() { Name = name, Value = value };
            }
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
