using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Save
{
    /// <summary>
    /// Represents the data to save to accurately determine a <see cref="StinkyLevel"/>'s completion
    /// </summary>
    [Serializable]
    public class LevelCompletionInfo
    {
        /// <summary>
        /// 5 minutes in seconds -- the default level time
        /// </summary>
        public const int DefaultTime = 300;
        /// <summary>
        /// 1 minute in seconds -- the default bonus level time
        /// </summary>
        public const int BonusTime = 60;

        /// <summary>
        /// The name of the level
        /// </summary>
        public string LevelName;
        /// <summary>
        /// The amount collected of the collectable item
        /// </summary>
        public int BonusesCollected, PattiesCollected;
        /// <summary>
        /// The time (in seconds) remaining when the level was completed. See <see cref="DefaultTime"/>
        /// </summary>
        public float TimeRemaining = DefaultTime;
        /// <summary>
        /// The calculated score of the level -- <see cref="BonusScore"/> + <see cref="TimeScore"/>
        /// </summary>
        public int LevelScore => BonusScore + TimeScore;     
        /// <summary>
        /// The calculated bonus score -- 250pts * <see cref="BonusesCollected"/>
        /// </summary>
        public int BonusScore => (250 * BonusesCollected);
        /// <summary>
        /// The calculated time score -- <see cref="TimeRemaining"/> * 10
        /// </summary>
        public int TimeScore => ((int)TimeRemaining) * 10;
        /// <summary>
        /// Represents whether the level available to play or not
        /// </summary>
        public bool IsAvailable;
         /// <summary>
        /// Represents whether the level successfully completed or not
        /// </summary>
        public bool WasSuccessful;
        /// <summary>
        /// This needs to be equal to <see cref="StinkyLevel.LevelWorldName"/> of the target level this is for
        /// </summary>
        public string LevelWorldName;
         /// <summary>
        /// Represents whether the level was beaten to perfection or not
        /// </summary>
        public bool WasPerfect;
        /// <summary>
        /// An array of the last 10 scores of this level
        /// </summary>
        public int[] HighScores;
        public int GetHighScore()
        {
            if (HighScores == null)
            {
                HighScores = new int[10];
                return 0;
            }
            return HighScores.Max();
        }
    }
}
