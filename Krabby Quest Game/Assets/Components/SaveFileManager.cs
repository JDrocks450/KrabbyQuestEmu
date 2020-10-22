using StinkyFile.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Components
{
    /// <summary>
    /// A class that serves the current save file opened by the user
    /// </summary>
    public static class SaveFileManager
    {
        /// <summary>
        /// The current SaveFile opened by the user
        /// </summary>
        public static SaveFile Current { get; private set; }

        /// <summary>
        /// Is a save file opened already?
        /// </summary>
        public static bool IsFileOpened => Current != null;

        /// <summary>
        /// Set the current save file
        /// </summary>
        /// <param name="Current"></param>
        public static void SetCurrentSaveFile(SaveFile Current) => SaveFileManager.Current = Current;
    }
}
