using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Installation
{
    public class PythonParameterEditor
    {
        /// <summary>
        /// True if the script is opened correctly
        /// </summary>
        public bool Opened => python != null;
        public String FilePath;
        private string python;

        public PythonParameterEditor()
        {

        }

        public PythonParameterEditor(string PythonScriptPath) : base()
        {
            Load(PythonScriptPath);
        }

        public void Load(string Path)
        {
            python = File.ReadAllText(Path);
            FilePath = Path;
        }

        public void Save()
        {
            File.WriteAllText(FilePath, python, Encoding.ASCII);
        }

        public int Replace(string parameterName, int offset, string value)
        {
            int index = python.IndexOf(parameterName, offset) + parameterName.Length+1;
            int endIndex = python.IndexOf("\"", index);
            python = python.Remove(index, endIndex - index);
            python = python.Insert(index, value);
            return index;
        }

        public static int ReplaceParameter(string FilePath, string ParamName, int Offset, string Value)
        {
            var editor = new PythonParameterEditor(FilePath);
            int index = editor.Replace(ParamName, Offset, Value);
            editor.Save();
            return index;
        }
    }
}
