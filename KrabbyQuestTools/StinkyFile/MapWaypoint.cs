using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace StinkyFile
{
    public class MapWaypointParser
    {
        private readonly string workspaceDir;
        private readonly StinkyParser Parser;
        public const string DBPath = "Resources/mapdb.xml";
        public Dictionary<StinkyLevel, SPoint> Levels = new Dictionary<StinkyLevel, SPoint>();

        public MapWaypointParser(string WorkspaceDir, StinkyParser parser)
        {
            if (!File.Exists(DBPath))
            {
                var doc = new XDocument(new XElement("root"));
                doc.Save(DBPath);
            }

            workspaceDir = WorkspaceDir;
            this.Parser = parser;
        }

        public bool LevelMarkerPlaced(StinkyLevel level) => Levels.Keys.FirstOrDefault(x => x.LevelFilePath == level.LevelFilePath) != null;

        public void LoadAll()
        {
            XDocument doc = XDocument.Load(DBPath);
            Levels.Clear();
            var source = doc.Root.Elements("point").OrderBy(x => // order by index
            {
                var value = x.Element("LevelFileName").Value.Replace(".lv5", "");
                var numbers = value.Where(c => char.IsDigit(c));
                if (numbers.Count() != value.Length)
                    return 1000;
                else
                    return int.Parse(new string(numbers.ToArray()));
            });
            foreach (var element in source)
            {
                var info = Load(element);
                Levels.Add(info.level, info.position);
            }
        }

        /// <summary>
        /// Loads the waypoint reference from the DB from the specified DB element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public (StinkyLevel level, SPoint position) Load(XElement element)
        {
            var levelName = element.Element("LevelFileName").Value;
            var level = Parser.LevelRead(System.IO.Path.Combine(workspaceDir, "levels", levelName));
            var positionStrings = element.Element("Position").Value.Split(',');
            var x = double.Parse(positionStrings[0]);
            var y = double.Parse(positionStrings[1]);
            return (level, new SPoint(x, y));
        }

        /// <summary>
        /// Loads the waypoint reference from the DB from the specified level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public (StinkyLevel level, SPoint position) Load(StinkyLevel level)
        {
            var element = GetDataElement(level).FirstOrDefault();
            if (element == null)
                return (level, default);
            return Load(element);
        }

        /// <summary>
        /// Loads the waypoint reference from the DB from the specified level name
        /// </summary>
        /// <param name="levelFileName"></param>
        /// <returns></returns>
        public (StinkyLevel level, SPoint position) Load(string levelFileName)
        {
            var element = GetDataElement(new StinkyLevel() { LevelFilePath = levelFileName }).FirstOrDefault();
            if (element == null)
                return default;
            return Load(element);
        }

        /// <summary>
        /// Finds all waypoint DB elements that reference this level
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public IEnumerable<XElement> GetDataElement(StinkyLevel level)
        {
            XDocument doc = XDocument.Load(DBPath);
            var name = System.IO.Path.GetFileName(level.LevelFilePath);
            return doc.Root.Elements("point").Where(x => x.Element("LevelFileName").Value == name);
        }

        /// <summary>
        /// Saves the data element referencing this level - Autosaves DB
        /// </summary>
        /// <param name="level"></param>
        /// <param name="location"></param>
        /// <param name="duplicate">Remove other references to this waypoint? Can cause many problems if true!</param>
        public void Save(StinkyLevel level, SPoint location, bool duplicate = false)
        {
            XDocument doc = XDocument.Load(DBPath);
            var name = System.IO.Path.GetFileName(level.LevelFilePath);
            var element = GetDataElement(level).ToList();
            foreach (var e in element)
                e.Remove();
            doc.Root.Add(new XElement("point",
                new XElement("LevelFileName", name),
                new XElement("Position", $"{location.X},{location.Y}")));
            doc.Save(DBPath);
        }

        /// <summary>
        /// Removes the waypoint reference from the DB - Autosaves DB
        /// </summary>
        /// <param name="level"></param>
        public void Remove(StinkyLevel level)
        {
            XDocument doc = XDocument.Load(DBPath);
            var element = GetDataElement(level).ToList(); // foreach collection change workaround
            foreach (var e in element)
                e.Remove();
            doc.Save(DBPath);
        }
    }
}
