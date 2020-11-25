﻿//#define CHECK_VERSION

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace KrabbyQuestTools.Common
{
    /// <summary>
    /// Handles patching KrabbyQuestEmu Game engine and installer
    /// </summary>
    public class GamePatcher
    {
        public string Owner
        {
            get; set;
        } = "JDrocks450";
        public string Repo
        {
            get; set;
        } = "KrabbyQuestEmu";
        public string InstallerGitReleaseTag
        {
            get; set;
        } = "install";
        public string GameGitReleaseTag
        {
            get; set;
        } = "game";
        public string DestinationDirectory { get; }

        /// <summary>
        /// Path that is populated <see cref="Start"/> is called and completed successfully.
        /// </summary>
        public string GameExePath, EditorExePath;

        public EventHandler<(string message, double percent)> ProgressChanged { get; set; }

        public GamePatcher(string DestinationDirectory)
        {            
            this.DestinationDirectory = DestinationDirectory;            
        }

        private static string ReadVersion() => File.ReadAllText("Resources/versioninfo.txt");        

        public async Task<bool> Start()
        {
            string extractPath = await GetLatestDist();
            string gameExtractPath = DestinationDirectory;
            ExtractZip(extractPath, gameExtractPath);
            return true;
        }

        private void ExtractZip(string source, string destination)
        {
            ZipFile.ExtractToDirectory(source, destination);
            GameExePath = Path.Combine(destination, "Krabby Quest Game", "Krabby Quest Game.exe");
            EditorExePath = Path.Combine(destination, "KrabbyQuestTools", "KrabbyQuestTools.exe");
        }

        private async Task<string> getDownloadUrl()
        {
            using (HttpClient client = new HttpClient() { BaseAddress = new Uri("https://api.github.com/") })
            {
                var productVersion = ReadVersion();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("KrabbyQuestEmu_Installer", productVersion));
                string uri = $"repos/{Owner}/{Repo}/releases";
                var msg = client.GetAsync(uri).Result;
                if (msg.IsSuccessStatusCode)
                {
                    JArray responseArray = JArray.Parse(await msg.Content.ReadAsStringAsync());
                    foreach (dynamic response in responseArray.Children())
                    {
                        string tag = response.tag_name;
#if CHECK_VERSION
                        if (!tag.StartsWith(GameGitReleaseTag)) continue; // this response is an update for something else
                        if (tag == ReadVersion()) return null; // no updates found
#endif
                        JArray assets = response.assets;
                        string downloadUrl = null;
                        foreach (dynamic asset in assets.Children())
                        {
                            if (asset.content_type != "application/x-zip-compressed" &&
                                asset.content_type != "application/zip") continue; // it isn't a zip file, so it can't be extracted
                            downloadUrl = asset.browser_download_url;
                        }
                        if (downloadUrl == null) return null;                        
                        File.WriteAllText("Resources/versioninfo.txt", tag); // write new version
                        return downloadUrl;
                    }
                }
                else return null;
            }
            return null;
        }

        public async Task<string> GetLatestDist()
        {
            if (!Directory.Exists(DestinationDirectory))
                Directory.CreateDirectory(DestinationDirectory);
            string url = await getDownloadUrl();
            if (url == null)
                throw new Exception("Could not get release info from GitHub.");
            WebClient client = new WebClient();
            string dest = "dist.zip";
            if (File.Exists("dist.zip"))
                return dest;
            client.DownloadProgressChanged += async (object s, 
                DownloadProgressChangedEventArgs e) => 
                    PushProgressChanged($"Downloading KrabbyQuestEmu ({e.TotalBytesToReceive} bytes)", e.ProgressPercentage);
            await client.DownloadFileTaskAsync(new Uri(url), dest);
            return dest;
        }

        private void PushProgressChanged(string message, double progress) => ProgressChanged?.Invoke(this, (message, progress));
    }
}
