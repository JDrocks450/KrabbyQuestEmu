using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Installation
{
    /// <summary>
    /// The automated installation for StinkyFile
    /// </summary>
    public class AutoInstall
    {
        /// <summary>
        /// The various types of installations this installer can accomplish
        /// </summary>
        public enum InstallationType
        {
            /// <summary>
            /// The installer is between states
            /// </summary>
            Unknown,
            /// <summary>
            /// The installer is initializing
            /// </summary>
            Initialize,
            /// <summary>
            /// The installer is extracting content
            /// </summary>
            ExtractContent,
            /// <summary>
            /// The installer is creating the bitmap font
            /// </summary>
            CreateFont,
            /// <summary>
            /// The installer is creating models. See <see cref="ModelExporter"/>
            /// </summary>
            CreateModels,
            /// <summary>
            /// The installer is finishing up
            /// </summary>
            Finish,
            /// <summary>
            /// The installer is done
            /// </summary>
            Done,
            /// <summary>
            /// The installer is uninstalling files
            /// </summary>
            Uninstallation
        }
        /// <summary>
        /// What the installer is currently installing
        /// </summary>
        public InstallationType CurrentType { get; private set; }
        /// <summary>
        /// Can the installation continue?
        /// </summary>
        public bool Paused { 
            get => _paused; 
            set
            {
                _paused = value;
                if (ThreeDModelExporter != null)
                    ThreeDModelExporter.Paused = value;
                OnPausedStateChanged?.Invoke(this, value);
            }
        }
        /// <summary>
        /// Raised when the installation is Paused/Unpaused
        /// </summary>
        public event EventHandler<bool> OnPausedStateChanged;
        /// <summary>
        /// The current errors with the installation
        /// </summary>
        public IEnumerable<string> Errors = new List<string>();
        /// <summary>
        /// All added/modified files that can be deleted later if necessary
        /// </summary>
        public StringBuilder Manifest
        {
            get; private set;
        } = new StringBuilder();
        /// <summary>
        /// Where Blender.exe is installed
        /// </summary>
        public string BlenderPath { get; set; }
        /// <summary>
        /// The place to install the files to
        /// </summary>
        public string DestinationDirectory{ get; set; } 
        /// <summary>
        /// Where KrabbyQuest is installed
        /// </summary>
        public string SourceDirectory { get; set; }
        /// <summary>
        /// Due to complications with support for Tga in DotNet, the implementation to extract a Font must be called by the frontend
        /// </summary>
        public EventHandler<(string path, Primitive.RefPrim<bool> success)> OnFontCreate;
        /// <summary>
        /// Due to complications with support for Tga in DotNet, the implementation to extract a Tga must be called by the frontend
        /// </summary>
        public EventHandler<(string path, ManifestChunk chunk, Primitive.RefPrim<bool> success)> OnTgaExtracting;
        /// <summary>
        /// Called when progress is made on the installation
        /// </summary>
        public EventHandler<InstallationCompletionInfo> OnProgressChanged;
        /// <summary>
        /// Called when a new installation step is started
        /// </summary>
        public EventHandler<InstallationType> OnStepStarted;
        /// <summary>
        /// The amount of time spent from when <c>Start</c> was called
        /// </summary>
        public TimeSpan ElapsedTime => stopWatch.Elapsed;
        private Stopwatch stopWatch;
        /// <summary>
        /// Flag to override automatic dumping of manifest
        /// </summary>
        private bool dontDump = false,
                     tryingToSkip = false;
        private ModelExporter ThreeDModelExporter;
        private bool _paused;
        private StringBuilder StandardOutput = new StringBuilder();

        /// <summary>
        /// Creates a new instance of the AutoInstaller
        /// </summary>
        public AutoInstall()
        {
            stopWatch = new Stopwatch();
        }

        /// <summary>
        /// Checks if there are files in the directory that can be installed
        /// </summary>
        /// <param name="InstallDir"></param>
        /// <returns></returns>
        public static bool CanUninstall(string InstallDir) => File.Exists(Path.Combine(InstallDir, "manifest.txt"));

        /// <summary>Here is an example of a bulleted list:
        /// <list type="bullet">
        /// <item>
        /// <description>Initialize is always called first unless specified using the <c>DoInitialize</c> flag.</description>
        /// </item>
        /// </list>
        /// </summary>
        public bool Start(InstallationType Type, bool DoInitialize = true)
        {
            dontDump = false;
            tryingToSkip = false;
            if (DoInitialize)
                if (!Initialize())
                {
                    stopWatch.Stop();
                    return false;
                }
            CurrentType = Type;
            bool retVal = true;
            switch (Type)
            {
                case InstallationType.Initialize:
                    OnStepStarted?.Invoke(this, Type);
                    if (!DoInitialize) 
                        retVal = Initialize();
                    break;
                case InstallationType.Finish:
                    OnStepStarted?.Invoke(this, Type);
                    retVal = FinishUp();
                    break;
                case InstallationType.ExtractContent:
                    OnStepStarted?.Invoke(this, Type);
                    retVal = Extract();
                    break;
                case InstallationType.CreateModels:
                    OnStepStarted?.Invoke(this, Type);
                    retVal = ExtractModels();
                    break;
                case InstallationType.CreateFont:
                    OnStepStarted?.Invoke(this, Type);
                    retVal = CreateFont();
                    break;
                case InstallationType.Uninstallation:
                    OnStepStarted?.Invoke(this, Type);
                    retVal = Uninstall();
                    break;
            }
            if (!dontDump)
                DumpManifest();
            dontDump = false;
            stopWatch.Stop();
            return retVal;
        }

        private bool Uninstall()
        {
            if (!CanUninstall(DestinationDirectory))
            {
                PushError("There are no files left to install, or the game was not installed using this installer." +
                    "This installer can only uninstall KrabbyQuestEmu if it was installed using this program.");
                return false;
            }
            var uninstaller = new Uninstall(DestinationDirectory);
            uninstaller.Start();
            Manifest = uninstaller.manifest;
            PushError(uninstaller.errors.ToString());
            dontDump = true;
            return true;
        }

        /// <summary>
        /// Starts the installation with the current parameters, highly recommended to call this on a background thread as many methods are blocking.
        /// </summary>
        public bool Start()
        {
            Manifest = new StringBuilder();
            bool exit(bool success)
            {
                stopWatch.Stop();
                return success;
            }

            if (!Start(InstallationType.Initialize, false)) return exit(false);
            AwaitPaused();
            if (!Start(InstallationType.ExtractContent, false)) return exit(false);
            AwaitPaused();
            if (!Start(InstallationType.CreateFont, false)) return exit(false);
            AwaitPaused();
            if (!Start(InstallationType.CreateModels, false)) return exit(false);
            AwaitPaused();
            if (!Start(InstallationType.Finish, false)) return exit(false);
            return exit(true);
        }

        /// <summary>
        /// Add an entry into the manifest that indicates a target file and what has happened to it.
        /// <para><see cref="FileChange.ADD"/> will mark a file as uninstallable</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="changeMode"></param>
        public void FileChanged(string path, FileChange changeMode) => Manifest.AppendLine(FileChangedFormat(path, changeMode));

        internal static string FileChangedFormat(string path, FileChange changeMode) => $"[{Enum.GetName(typeof(FileChange), changeMode)}]{path}";

        private void DumpManifest(string path = default)
        {
            if (path == default)
                path = Path.Combine(DestinationDirectory, "manifest.txt");
            var data = Encoding.UTF8.GetBytes(Manifest.ToString());
            using (var fs = File.OpenWrite(path))
            {
                fs.Seek(0, SeekOrigin.End);
                fs.Write(data, 0, data.Length);
            }       
        }

        /// <summary>
        /// Finish up the installation
        /// </summary>
        /// <returns></returns>
        private bool FinishUp()
        {
            if (!CanUninstall(DestinationDirectory))
            {
                CallInstallationUpdate(
                "Finalization",
                "Unable to check files",
                1 / 1.0);
                PushError("Could not find installer manifest file, the installation cannot be verified. ");
                return true;
            }
            var entries = StinkyFile.Installation.Uninstall.ParseInstallerManifest(DestinationDirectory);
            int current = 0;
            foreach (var entry in entries)
            {
                CallInstallationUpdate(
                                "Finalization",
                                "Checking Files (" + current + "/" + entries.Count,
                                current/(double)entries.Count);
                if (entry.Value == FileChange.ADD && !File.Exists(entry.Key))
                    PushError(entry.Key + " is missing!");
                current++;
            }            
            return true;
        }

        /// <summary>
        /// Create the font images into one happy spritefont -- or at least tells the frontend to do so!
        /// </summary>
        /// <returns></returns>
        private bool CreateFont()
        {
            CallInstallationUpdate(
                    "Font Creation",
                    $"Creating Font: KrabbyQuestFont",
                    0.0 / 1
                );
            var path = System.IO.Path.Combine(DestinationDirectory, "Graphics");
            Primitive.RefPrim<bool> success = new Primitive.RefPrim<bool>(false);
            OnFontCreate?.Invoke(this, (path, success));            
            CallInstallationUpdate(
                    "Font Creation",
                    $"Created Font: KrabbyQuestFont",
                    1 / 1
                );
            return success.Value;
        }

        /// <summary>
        /// Extracts all the models to the models directory
        /// </summary>
        private bool ExtractModels()
        {
            var exporter = ThreeDModelExporter = new ModelExporter(BlenderPath,
                Path.Combine(DestinationDirectory, "Graphics"),
                Path.Combine(DestinationDirectory, "Export"))
            {
                Paused = Paused
            };
            exporter.OnStandardOutput += (object s, string output) => StandardOutput.AppendLine(output);
            exporter.OnFileExtracted += (object se, string path) => FileChanged(path, FileChange.ADD);
            exporter.OnProgressChanged +=
                (object s, (string message, double percentage) e) =>
                {                    
                    CallInstallationUpdate("3D Model Creation", e.message, e.percentage);
                };
            AwaitPaused();
            if (tryingToSkip)
            {
                PushError("3D Model Extraction step was skipped, the installation is incomplete.");
                return true;
            }
            var result = exporter.ExportAll();
            if (!result)
                PushError(exporter.ExceptionObject.Message);
            return result;
        }

        /// <summary>
        /// Extracts the content from the content files
        /// </summary>
        /// <returns></returns>
        private bool Extract()
        {
            ManifestParser Parser = default;
            try
            {
                Parser = new ManifestParser(
                    Path.Combine(SourceDirectory, "res1.dat"),
                    Path.Combine(SourceDirectory, "res2.dat"));
            }
            catch(Exception e)
            {
                PushError("The extraction failed with exception, \n" + e.ToString());
                return false;
            }
            if (Parser.ManifestPathException != null)
            {
                PushError("The extraction failed with exception, \n" + Parser.ManifestPathException.Message);
                return false;
            }
            if (Parser == default)
                return false;            
            int count = 0;
            foreach (var chunk in Parser.Chunks)
            {
                AwaitPaused();
                if (tryingToSkip)
                {
                    PushError("Extraction step was skipped, the installation is incomplete.");
                    return true;
                }
                string path = System.IO.Path.Combine(DestinationDirectory, chunk.Name);
                CallInstallationUpdate(
                    "Extraction",
                    $"Extracting {chunk.Name}, {chunk.Size} bytes",
                    (double)count / Parser.Chunks.Count
                );
                count++;
                if (File.Exists(path))
                    continue;                
                var filePath = Parser.ExtractFile(DestinationDirectory, chunk);
                if (System.IO.Path.GetExtension(chunk.Name) == ".tga")
                {
                    if (File.Exists(path.Remove(path.Length-4) + ".png"))
                        continue;  
                    try
                    {
                        var refBool = new Primitive.RefPrim<bool>(false);
                        OnTgaExtracting?.Invoke(this, (path, chunk, refBool));
                        if (!refBool.Value)
                            return false;                        
                    }
                    catch(Exception e)
                    {
                        PushError("Extracting Targa image failed, installation continuing... exception: \n" + e.ToString());
                        continue;
                    }
                    FileChanged(filePath, FileChange.DELETE);
                }     
                else FileChanged(filePath, FileChange.ADD);
            }
            return true;
        }

        /// <summary>
        /// Checks everything is present before starting the installation
        /// </summary>
        private bool Initialize()
        {
            stopWatch.Start();
            bool hasErrors = false;
            CallInstallationUpdate("Initialization", "Initializing...", 0);
            if (string.IsNullOrWhiteSpace(DestinationDirectory) || 
                string.IsNullOrWhiteSpace(BlenderPath) || 
                string.IsNullOrWhiteSpace(SourceDirectory))
            {
                PushError("One or more fields have been left blank. The installation cannot continue.");
                hasErrors = true;
            }
            if (hasErrors)
                return false;
            if (!BlenderPath.ToLower().Contains("blender") ||
                !File.Exists(BlenderPath))
            {
                PushError("Blender.exe was not found at that path. Check the path and try again.");
                hasErrors = true;
            }
            if (!Directory.Exists(SourceDirectory))
            {
                PushError("The source directory doesn't exist. The installation cannot continue" +
                    " without the KrabbyQuest files being present at the source directory.");
                hasErrors = true;
            }
            var path = Path.Combine(SourceDirectory, "res1.dat");
            if (!File.Exists(path))
            {
                PushError("One or more files that are required for installation are not present at the " +
                    "source directory. Make sure that the Krabby Quest game files are present at: " + path);
                hasErrors = true;
            }
            if (OnTgaExtracting == null)            
                PushError("Developer oversight caught, Targa image extraction not implemented. Installation may be incomplete.");
            if (hasErrors) 
                return false;
            Directory.CreateDirectory(DestinationDirectory);
            CallInstallationUpdate("Initialization", "Success!", 1);
            return true;
        }

        public void Skip()
        {
            Paused = false;
            tryingToSkip = true;
        }

        private void CallInstallationUpdate(string Title, string Description, double Completion)
        {
            OnProgressChanged.Invoke(this, new InstallationCompletionInfo()
            {
                StepName = Title,
                CurrentTask = Description,
                PercentComplete = Completion,
                ElapsedTime = ElapsedTime,
                StandardOutput = StandardOutput.ToString() == "" ? null : StandardOutput.ToString()
            });
            StandardOutput = new StringBuilder();
        }

        /// <summary>
        /// Pauses/Unpauses the installation
        /// </summary>
        public void TogglePause()
        {
            Paused = !Paused;            
        }

        /// <summary>
        /// Waits until the installation is no longer paused. ~Blocks the current thread -- only use in async environment~
        /// </summary>
        private void AwaitPaused()
        {                                    
            while (Paused)
            {
                Task.Delay(1000);
            }
        }

        public void PushError(string error) => (Errors as List<string>).Add(error);
    }    
}
