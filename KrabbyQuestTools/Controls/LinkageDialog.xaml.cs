using KrabbyQuestTools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KrabbyQuestTools.Controls
{
    /// <summary>
    /// Interaction logic for LinkageDialog.xaml
    /// </summary>
    public partial class LinkageDialog : Window
    {
        public string LinkageName, B3DPath, GLBPath, Workspace;
        public LinkageDialog(string Name, string B3DPath, string GLBPath, string Workspace)
        {
            InitializeComponent();

            LinkageName = Name;
            this.Name.Text = Name;
            BPath.Text = B3DPath;
            this.GLBPath = GLBPath;
            this.Workspace = Workspace;
            //GPath.Text = GLBPath;
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            while (true)
            {
                string path = GPath.Text;
                if (FileBrowser.BrowseForFile(ref path, "Find a GLB File") == System.Windows.Forms.DialogResult.Cancel)
                    break;
                if (path.StartsWith(Workspace))
                {
                    GPath.Text = path;
                    break;
                }
                else
                {
                    MessageBox.Show("That path is not a child of the Workspace Directory. These files need to be in the workspace directory in order for the game to find them.");
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LinkageName = Name.Text;
            B3DPath = BPath.Text.Remove(0, Workspace.Length+1);            
            GLBPath = GPath.Text.Remove(0, Workspace.Length+1);
            DialogResult = true;
            Close();
        }
    }
}
