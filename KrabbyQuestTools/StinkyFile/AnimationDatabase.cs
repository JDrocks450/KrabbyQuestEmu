using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile
{
    [JsonObject]
    public class AnimationDatabaseEntry
    {
        [JsonProperty]
        public string B3DFilePath
        {
            get; set;
        }
        [JsonProperty]
        public string GLBFilePath
        {
            get;set;
        }
        [JsonProperty]
        public string Name
        {
            get;set;
        }
    }

    /// <summary>
    /// Links B3D files to GLB files to indicate which objects have animated properties
    /// </summary>
    public static class AnimationDatabase
    {
        /// <summary>
        /// The path where the AnimationDatabase is kept
        /// </summary>
        public static string RelativeAnimationDatabasePath
        {
            get; set;
        } = "Resources/animDB.json";

        private static List<AnimationDatabaseEntry> Entries;
        private static bool entriesLoaded = false;

        public static AnimationDatabaseEntry GetEntryByName(string Name)
        {
            if (!entriesLoaded)
                GetLinkages();
            return Entries.FirstOrDefault(x => x.Name == Name);
        }

        public static AnimationDatabaseEntry GetEntryByB3DPath(string B3DPath)
        {
            if (!entriesLoaded)
                GetLinkages();
            return Entries.FirstOrDefault(x => x.B3DFilePath == B3DPath);
        }

        public static AnimationDatabaseEntry GetEntryByGLBPath(string GLBPath)
        {
            if (!entriesLoaded)
                GetLinkages();
            return Entries.FirstOrDefault(x => x.GLBFilePath == GLBPath);
        }

        /// <summary>
        /// Loads all animation database entries from the path provided.
        /// </summary>
        /// <param name="path">Default uses the <see cref="RelativeAnimationDatabasePath"/> to find the animation database, or optionally this parameter overrides those.</param>
        /// <returns></returns>
        public static IEnumerable<AnimationDatabaseEntry> GetLinkages(string path = default)
        {
            if (entriesLoaded)
                return Entries.ToList();            
            if (path == default)
                path = RelativeAnimationDatabasePath;
            if (string.IsNullOrWhiteSpace(path))
                throw new Exception("The path provided was null. Make sure the WorkspaceDir and RelativeAnimationDatabasePath variables are set.");
            if (!File.Exists(path))
                return Entries = new List<AnimationDatabaseEntry>();
            entriesLoaded = true;
            var content = File.ReadAllText(path);
            JArray array = JArray.Parse(content);
            return Entries = array.Select(x => x.ToObject<AnimationDatabaseEntry>()).ToList();
        }

        /// <summary>
        /// Adds a new linkage to the database
        /// </summary>
        /// <param name="name"></param>
        /// <param name="B3D">Relative path from the workspace to the B3D file</param>
        /// <param name="GLB">Relative path from the workspace to the GLB file</param>
        public static void AddLinkage(string name, string B3D, string GLB)
        {
            if (!entriesLoaded)
                GetLinkages();
            var linkages = Entries;
            linkages.Add(new AnimationDatabaseEntry()
            {
                Name = name,
                B3DFilePath = B3D,
                GLBFilePath = GLB
            });
            Save();
        }

        public static void Save()
        {
            string path = RelativeAnimationDatabasePath;
            JArray array = new JArray();
            foreach(AnimationDatabaseEntry entry in Entries)            
                array.Add(JToken.FromObject(entry));
            File.WriteAllText(path, array.ToString());
        }
    }
}
