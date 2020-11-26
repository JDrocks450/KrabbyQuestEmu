using StinkyFile.Installation;
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
        public Exception ExceptionObject { get; private set; }

        public const string DefaultBlenderPath = @"C:\Program Files\Blender Foundation\Blender\blender.exe";
        public string BlenderPath { get; }
        public string B3DContentDirectory { get; }
        public string ExportDirectory { get; }
        public bool Paused { get; set; } = false;
        public bool TryingToSkip { get; set; } = false;

        /// <summary>
        /// Called whenever the model exporter makes progress. String Message, Double PercentComplete 0-1
        /// </summary>
        public event EventHandler<(string message, double percentComplete)> OnProgressChanged;
        /// <summary>
        /// Called whenever the model exporter extracts a file. FilePath is the supplied string
        /// </summary>
        public event EventHandler<string> OnFileExtracted;
        /// <summary>
        /// Called whenever the model exporter recieves output from Blender.exe
        /// </summary>
        public event EventHandler<string> OnStandardOutput;

        public ModelExporter(string BlenderPath, string B3DContentDirectory, string ExportDirectory)
        {
            this.BlenderPath = BlenderPath;
            this.B3DContentDirectory = B3DContentDirectory;
            this.ExportDirectory = ExportDirectory;
        }

        public bool ExportAll()
        {            
            ExceptionObject = null;
            bool asyncReading = false;
            Process BlenderProcess = new Process();
            try
            {
                Directory.CreateDirectory(ExportDirectory);
                var files = Directory.GetFiles(B3DContentDirectory, "*.b3d");
                var pluginPath = Path.Combine(Environment.CurrentDirectory, "BlenderBackgroundConverter.py").Replace("\\", "/");
                PythonParameterEditor pythonEditor = new PythonParameterEditor(pluginPath);
                int current = 0;                
                BlenderProcess.EnableRaisingEvents = true;
                BlenderProcess.ErrorDataReceived +=
                        (object s, DataReceivedEventArgs e) =>
                            OnStandardOutput?.Invoke(this, e.Data);
                BlenderProcess.OutputDataReceived +=
                        (object s, DataReceivedEventArgs e) =>
                            OnStandardOutput?.Invoke(this, e.Data);
                foreach (var file in files)
                {
                    if (TryingToSkip)
                        return true;
                    while (Paused)
                    {

                    }
                    if (TryingToSkip)
                        return true;
                    OnProgressChanged?.Invoke(this, ("Converting " + file, current / (double)files.Length));
                    var name = Path.GetFileNameWithoutExtension(file);
                    var destination = Path.Combine(ExportDirectory, name + ".obj").Replace("\\", "/");
                    int index = pythonEditor.Replace("filepath=", 0, file.Replace("\\", "/"));
                    index = pythonEditor.Replace("filepath=", index + file.Length + 5, destination);
                    pythonEditor.Save();
                    var logFile = Path.Combine(Environment.CurrentDirectory, "log.log");
                    var startInfo = new ProcessStartInfo(BlenderPath, "-b -P " + pluginPath)
                    {
                        UseShellExecute = false,
                        WorkingDirectory = Path.GetDirectoryName(BlenderPath),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };
                    BlenderProcess.StartInfo = startInfo;
                    BlenderProcess.Start();
                    BlenderProcess.BeginOutputReadLine();
                    BlenderProcess.BeginErrorReadLine();
                    asyncReading = true;
                    BlenderProcess.WaitForExit();
                    BlenderProcess.CancelOutputRead();
                    BlenderProcess.CancelErrorRead();
                    if (BlenderProcess.ExitCode != 0)
                    {
                        //Process.Start(logFile);
                        throw new Exception("Blender.exe exited with error code: " + BlenderProcess.ExitCode +
                            ". Try repeating the 3D Model extraction stage again. If the problem persists, " +
                            "try moving all files related to the installer to the desktop, as long filepaths can " +
                            "cause errors using Blender. The Blender output is shown above for more information.");
                    }
                    OnFileExtracted?.Invoke(this, destination);
                    current++;
                }
            }
            catch (Exception e)
            {
                ExceptionObject = e;
                OnProgressChanged?.Invoke(this, ("Could Not Convert Model", 0 / 1.0));
                if (asyncReading)
                {
                    BlenderProcess.CancelOutputRead();
                    BlenderProcess.CancelErrorRead();
                }
                return false;
            }
            OnProgressChanged?.Invoke(this, ("All Models Converted", 1/1.0));
            return true;
        }
    }
}
