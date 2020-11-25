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
        public static DialogResult BrowseForFile(ref string Path)
        {
            OpenFileDialog fBrowserdiag = new OpenFileDialog()
            {
                InitialDirectory = @"C:\Program Files",
                Title = "Find Blender.exe on your PC",
                FileName = Path,
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
