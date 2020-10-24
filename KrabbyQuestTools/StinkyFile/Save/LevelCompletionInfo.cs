using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Save
{
    [Serializable]
    public class LevelCompletionInfo
    {
        public string LevelName;
        public int BonusesCollected, PattiesCollected;
        public float TimeRemaining;
        public int LevelScore => BonusScore + TimeScore;        
        public int BonusScore => (250 * BonusesCollected);
        public int TimeScore => ((int)TimeRemaining);
        public bool IsAvailable;
        public bool WasSuccessful;
        public string LevelWorldName;
        public bool WasPerfect;
    }
}
