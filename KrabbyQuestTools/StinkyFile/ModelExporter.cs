using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StinkyFile
{
    public class ModelExporter
    {
        public const string DefaultBlenderPath = @"C:\Program Files\Blender Foundation\Blender\blender.exe";
        public string BlenderPath { get; }
        public string B3DContentDirectory { get; }
        public string ExportDirectory { get; }
        public ModelExporter(string BlenderPath, string B3DContentDirectory, string ExportDirectory)
        {
            this.BlenderPath = BlenderPath;
            this.B3DContentDirectory = B3DContentDirectory;
            this.ExportDirectory = ExportDirectory;
        }

        public void ExportAll()
        {
            var files = Directory.GetFiles(B3DContentDirectory, "*.b3d");
            foreach (var file in files)
            {
                var python = File.ReadAllText("BlenderBackgroundConverter.py");
                int index = python.IndexOf("filepath=", 0) + 10;
                int endIndex = python.IndexOf("\"", index);
                python = python.Remove(index, endIndex - index);
                python = python.Insert(index, file.Replace("\\", "/"));
                index = python.IndexOf("filepath=", index + file.Length + 5) + 10;
                endIndex = python.IndexOf("\"", index);
                python = python.Remove(index, endIndex - index);
                var name = Path.GetFileNameWithoutExtension(file);
                python = python.Insert(index, Path.Combine(ExportDirectory, name + ".obj").Replace("\\", "/"));
                File.WriteAllText("BlenderBackgroundConverter.py", python);
                var processInfo = Process.Start(BlenderPath, "--background --python " + Path.Combine(Environment.CurrentDirectory, "BlenderBackgroundConverter.py"));
                processInfo.WaitForExit();
            }
        }
    }
}
