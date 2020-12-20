using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KrabbyQuestTools.Common
{
    public class FileBrowser
    {
        public static DialogResult BrowseForFile(ref string Path, string Title)
        {
            var initialDir = System.IO.Path.GetDirectoryName(Path);
            OpenFileDialog fBrowserdiag = new OpenFileDialog()
            {
                InitialDirectory = initialDir == "" ? @"C:\Program Files" : initialDir,
                Title = Title,
                FileName = Path,
                RestoreDirectory = true,                
                Multiselect = false,
                AutoUpgradeEnabled = true,
                AddExtension = true,
                DereferenceLinks = true,
                DefaultExt = "exe",
                ValidateNames = true,                
            };
            if (fBrowserdiag.ShowDialog() == DialogResult.Cancel)
                return DialogResult.Cancel;
            Path = fBrowserdiag.FileName;
            return DialogResult.OK;
        }

        public static DialogResult BrowseForDirectory(ref string path)
        {
            FolderBrowserDialog fBrowserdiag = new FolderBrowserDialog()
            {
                Description = "Select a folder to save files to",
                SelectedPath = path,
                ShowNewFolderButton = true
            };
            if (fBrowserdiag.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return DialogResult.Cancel; // cancelled
            path = fBrowserdiag.SelectedPath;
            return DialogResult.OK;
        }
    }
}
