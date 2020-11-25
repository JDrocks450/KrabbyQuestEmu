using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Installation
{
    /// <summary>
    /// Handles uninstallation of KrabbyQuestEmu by using the manifest file the installer creates
    /// </summary>
    internal class Uninstall
    {
        string InstallDir;
        internal Dictionary<string, FileChange> Entries = new Dictionary<string, FileChange>();
        internal StringBuilder errors = new StringBuilder();
        internal StringBuilder manifest = new StringBuilder();

        /// <summary>
        /// Creates an uninstaller instance
        /// </summary>
        /// <param name="InstallDir">The place where the game is expected to be installed</param>
        internal Uninstall(string InstallDir)
        {
            this.InstallDir = InstallDir;
        }

        public static string GetManifestPath(string InstallDir) => Path.Combine(InstallDir, "manifest.txt");

        public static Dictionary<string, FileChange> ParseInstallerManifest(string InstallDir)
        {
            var Entries = new Dictionary<string, FileChange>();
            using (var fs = File.OpenText(GetManifestPath(InstallDir)))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    if (line[0] != '[' || !line.Contains(']'))
                        continue;
                    var startPathIndex = line.IndexOf(']') + 1;
                    var changeStr = line.Substring(1, startPathIndex-2);
                    var change = (FileChange)Enum.Parse(typeof(FileChange), changeStr);
                    var path = line.Substring(startPathIndex);  
                    if (!Entries.ContainsKey(path))
                        Entries.Add(path, change);
                }
            }
            return Entries;
        }

        void ParseInstallerManifest() => Entries = ParseInstallerManifest(InstallDir);

        /// <summary>
        /// Starts the uninstallation
        /// </summary>
        internal bool Start()
        {
            ParseInstallerManifest();
            return DeleteFiles();
        }

        bool verifyPath(string path) => Path.GetDirectoryName(path).Contains(InstallDir);

        private void FileChanged(string path, FileChange change) => manifest.AppendLine(AutoInstall.FileChangedFormat(path, change));

        private bool DeleteFiles()
        {            
            foreach(var entry in Entries)
            {
                if (!File.Exists(entry.Key))
                    continue;
                if (verifyPath(entry.Key))
                {
                    File.Delete(entry.Key);
                    FileChanged(entry.Key, FileChange.DELETE);
                }
                else
                {
                    errors.AppendLine("The entry: " + entry.Key + " is likely in error. The uninstallation is canceled for " +
                        "safety. Is this is not in error, please delete the file manually and try again.");
                    return false;
                }
            }
            var manifestPath = Path.Combine(InstallDir, "manifest.txt");
            if (verifyPath(manifestPath))
            {
                File.Delete(manifestPath);
                FileChanged(manifestPath, FileChange.DELETE);
            }
            return true;
        }
    }
}
