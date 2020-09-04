using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile
{
    public class ManifestChunk
    {
        /// <summary>
        /// The name of this section
        /// </summary>
        public string Name
        {
            get;set;
        }
        public int Extra
        {
            get;set;
        }
        public ulong Offset
        {
            get;set;
        }
        public ulong EndOffset
        {
            get;set;
        }
        public ulong Size => EndOffset - Offset;
        public ManifestChunk()
        {

        }
        public ManifestChunk(string Name, ulong Offset, ulong EndOffset)
        {
            this.Name = Name;
            this.Offset = Offset;
            this.EndOffset = EndOffset;
        }
        public byte[] GetData(FileStream DataStream)
        {
            DataStream.Position = (long)Offset;
            byte[] array = new byte[Size];
            DataStream.Read(array, 0, (int)Size);
            return array;
        }
    }
    public class ManifestParser
    {
        public Exception ManifestPathException
        {
            get; private set;
        }
        public Exception DataPathException
        {
            get; private set;
        }
        /// <summary>
        /// The path to the Manifest dat file
        /// </summary>
        public string ManifestPath
        {
            get;set;
        }
        /// <summary>
        /// The path to the Data dat file
        /// </summary>
        public string DataPath
        {
            get;set;
        }

        public FileStream ManifestSource { get; private set; }
        public FileStream DataSource { get; private set; }
        public List<ManifestChunk> Chunks { get; private set; } = new List<ManifestChunk>();

        public ManifestParser()
        {

        }
        public ManifestParser(string manifestPath, string dataPath)
        {
            ManifestPath = manifestPath;
            DataPath = dataPath;
            Refresh();
        }

        public void Refresh()
        {
            bool error = false;
            try
            {
                ManifestSource = File.OpenRead(ManifestPath);
            } catch (Exception e)
            {
                ManifestPathException = e;
                error = true;
            }
            try
            {
                DataSource = File.OpenRead(DataPath);
            } catch (Exception e)
            {
                DataPathException = e;
                return;
            }
            if (error) return;
            DataPathException = null;
            ManifestPathException = null;
            Chunks.Clear();
            ulong EndOffset = 0;
            while (ManifestSource.Position != ManifestSource.Length) {
                Chunks.Add(GetChunk(ManifestSource.Position, EndOffset, out EndOffset));
            }
        }

        private ManifestChunk GetChunk(long ManifestOffset, ulong DataOffset, out ulong EndDataOffset)
        {
            ManifestSource.Position = ManifestOffset;
            var chunk = new ManifestChunk();
            byte[] nameLengthDat = new byte[4];
            ManifestSource.Read(nameLengthDat, 0, 4);
            int nameLen = BitConverter.ToInt32(nameLengthDat, 0);
            byte[] byte1 = { 0, 0, 0, 0 }; // the chunk after the name
            byte[] size = { 0, 0, 0, 0 }; // the size of the chunk
            byte[] nameData = new byte[nameLen];
            ManifestSource.Read(nameData, 0, nameLen);
            ManifestSource.Read(byte1, 0, 4);
            ManifestSource.Read(size, 0, 4);
            chunk.Name = Encoding.ASCII.GetString(nameData);
            chunk.Offset = DataOffset;
            int cSize = BitConverter.ToInt32(size, 0);
            chunk.EndOffset = EndDataOffset = DataOffset + (ulong)cSize;
            chunk.Extra = BitConverter.ToInt32(byte1, 0);
            return chunk;
        }
        
        public string ExtractFile(string DestinationDir, ManifestChunk Data)
        {
            var destPath = Path.Combine(DestinationDir, Data.Name);
            var data = Data.GetData(DataSource);
            Directory.CreateDirectory(Path.GetDirectoryName(destPath));
            using (var file = File.Create(destPath))
                file.Write(data, 0, data.Length);
            return destPath;
        }
    }
}
