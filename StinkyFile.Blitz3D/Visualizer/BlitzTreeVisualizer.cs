using StinkyFile.Blitz3D.B3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Visualizer
{
    public partial class BlitzTreeVisualizer
    {
        StringBuilder output;
        IEnumerable<BlitzObject> source;
        public BlitzObject Object
        {
            get; private set;
        }
        public Dictionary<BlitzObject, BlitzTreeItem> Tree;

        public BlitzTreeVisualizer(IEnumerable<BlitzObject> ObjectSource, BlitzObject baseObj)
        {
            output = new StringBuilder();
            source = ObjectSource;
            Tree = new Dictionary<BlitzObject, BlitzTreeItem>();
            Refresh(baseObj);
        }

        public void Refresh(BlitzObject blitzObj)
        {
            Object = blitzObj;
            createTree();
        }

        private void createTree()
        {            
            foreach(var obj in source)
            {
                var treeItem = new BlitzTreeItem(source, obj);
                Tree.Add(obj, treeItem);         
            }
        }

        void textDisplayTreeItem(BlitzTreeItem current, string indentAmount = "")
        {
            output.AppendLine($"{indentAmount} + { BlitzTreeItem.DisplayTypeName(current.Focus) } {current.Focus.Name}");
            indentAmount += "       ";
            foreach (var prop in current.GetPropertyValues())
            {
                output.AppendLine($"{indentAmount} {prop.Key}: {prop.Value}");
            }
            output.AppendLine("");
            indentAmount += "   ";
            foreach (var treeItem in current.Children)
                textDisplayTreeItem(Tree[treeItem], indentAmount);            
        }

        public string GetTextTree(BlitzTreeItem ViewFrom = default)
        {
            if (ViewFrom == default) ViewFrom = Tree.Values.First();
            if (ViewFrom == default) return "";
            output.Clear();
            textDisplayTreeItem(ViewFrom);
            return output.ToString();
        }
    }
}
