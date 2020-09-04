using StinkyFile;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace KrabbyQuestTools
{
    public static class AppResources
    {
        [Obsolete]
        public static StinkyParser Parser { get; set; }
        public static Random Random { get; } = new Random();

        public static Color[] BatchRandomColor(int amount, byte A = 255)
        {
            Color[] data = new Color[amount];
            for (int i = 0; i < amount; i++)
            {
                Color color = new Color();
                color.R = (byte)Random.Next(0, 255);
                color.G = (byte)Random.Next(0, 255);
                color.B = (byte)Random.Next(0, 255);
                color.A = A;
                data[i] = color;
            }
            return data;
        }

        public static Color S_ColorConvert(S_Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);

        public static string XamlToString<T>(T Object) where T : FrameworkElement
        {
            var sb = new StringBuilder();
            var writer = XmlWriter.Create(sb, new XmlWriterSettings
            {
                Indent = true,
                ConformanceLevel = ConformanceLevel.Fragment,
                OmitXmlDeclaration = true,
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
            });
            var mgr = new XamlDesignerSerializationManager(writer);

            // HERE BE MAGIC!!!
            mgr.XamlWriterMode = XamlWriterMode.Expression;
            // THERE WERE MAGIC!!!

            System.Windows.Markup.XamlWriter.Save(Object, mgr);
            return sb.ToString();
        }        

        /// <summary>
        /// Clones an XAML Element and creates a new one
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Object"></param>
        /// <returns></returns>
        public static T CloneXaml<T>(T Object) where T : FrameworkElement
        {
            string str = XamlToString(Object);
            StringReader stringReader = new StringReader(str);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T)XamlReader.Load(xmlReader);
        }
        public static T CloneXaml<T>(string Declaration) where T : FrameworkElement
        {
            StringReader stringReader = new StringReader(Declaration);
            XmlReader xmlReader = XmlReader.Create(stringReader);
            return (T)XamlReader.Load(xmlReader);
        }
    }
}
