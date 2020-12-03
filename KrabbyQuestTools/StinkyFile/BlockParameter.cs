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
    /// <summary>
    /// Attempts to convert the string value into type T
    /// <para><c>enum, string, int, double</c></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypedBlockParameter<T> : BlockParameter
    {
        /// <summary>
        /// The value of the parameter, converted to the type parameter
        /// </summary>
        public new T Value { get => _value; set => _value = value; }
        private T _value;
        /// <summary>
        /// Gets whether the conversion should be considered successful. 
        /// </summary>
        public bool ConversionSuccessful
        {
            get; private set;
        }

        public TypedBlockParameter() { }
        public TypedBlockParameter(string Value) : this() { ConversionSuccessful = Parse<T>(Value, out _value); }
        public TypedBlockParameter(T value) : this() => Value = value;

        private bool Parse<T>(string Value, out T pValue)
        {
            Type dType = typeof(T);
            if (dType.IsClass)
            {
                if (dType == typeof(string))
                {
                    pValue = (T)(object)Value;
                    return true;
                }
            }
            else if (dType.IsEnum)
            {
                try
                {
                    pValue = (T)Enum.Parse(dType, Value, true);
                    return true;
                }
                catch (Exception)
                {

                }
                pValue = default;
                return false; // if it's an enum and the string wasn't found, there is no sense in trying any other types, this is a spelling mistake.
            }
            try
            {
                pValue = (T)Convert.ChangeType(Value, typeof(T));
                return true;
            }
            catch (InvalidCastException)
            {

            }
            pValue = default;
            return false;
        }

        public static TypedBlockParameter<T> Create(string Value)
        {
            return new TypedBlockParameter<T>(Value);
        }
    }
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
