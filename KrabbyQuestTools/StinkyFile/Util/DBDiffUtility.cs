using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Util
{
    /// <summary>
    /// Compares 2 Databases for changes
    /// </summary>
    public class DBDiffUtility
    {
        public DBSelection Selection { get; }
        public string Path1 { get; }
        public string Path2 { get; }

        public LevelDataBlock[] DB1_BlockData { get; private set; }
        public LevelDataBlock[] DB2_BlockData { get; private set; }
        public Dictionary<string, LevelDataBlock> AddedBlocks, RemovedBlocks;
        public bool ChangesFound
        {
            get; private set;
        }
        public event EventHandler<double> PercentUpdated;

        public enum DBSelection
        {
            BlockDB,            
        }
        public DBDiffUtility(DBSelection selection, string Path1, string Path2)
        {
            Selection = selection;
            this.Path1 = Path1;
            this.Path2 = Path2;
        }

        public async Task FindDifferences()
        {
            if (Path1 == null || Path2 == null) throw new Exception("None of the paths submitted can be null.");
            var oldDBPath = LevelDataBlock.BlockDatabasePath;
            LevelDataBlock.BlockDatabasePath = Path1;
            DB1_BlockData = LevelDataBlock.LoadAllFromDB();
            LevelDataBlock.BlockDatabasePath = Path2;
            DB2_BlockData = LevelDataBlock.LoadAllFromDB();
            AddedBlocks = new Dictionary<string, LevelDataBlock>();
            RemovedBlocks = new Dictionary<string, LevelDataBlock>();
            double total = DB1_BlockData.Length + DB2_BlockData.Length, current = 0;
            await Task.Run(delegate
            {
                foreach (var entry in DB1_BlockData)
                {
                    current++;
                    PercentUpdated?.Invoke(this, current / total);
                    if (!RemovedBlocks.ContainsKey(entry.GUID))
                        if (!DB2_BlockData.Where(x => x.GUID == entry.GUID).Any())
                        {
                            RemovedBlocks.Add(entry.GUID, entry);
                            continue;
                        }
                }
                foreach (var entry in DB2_BlockData)
                {
                    current++;
                    PercentUpdated?.Invoke(this, current / total);
                    if (!AddedBlocks.ContainsKey(entry.GUID))
                        if (!DB1_BlockData.Where(x => x.GUID == entry.GUID).Any())
                        {
                            AddedBlocks.Add(entry.GUID, entry);
                            continue;
                        }
                }
            });
        }
    }
}
