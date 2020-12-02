using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Primitive
{
    [System.AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Property)]
    public class EditorVisible : System.Attribute
    {
        public const int LevelMinVersion = 3;
        public int LevelVersion = LevelMinVersion;
        public string Description;
        public EditorVisible()
        {

        }
        public EditorVisible(int LevelVersion) : this()
        {
            this.LevelVersion = LevelVersion;
        }
        public EditorVisible(string Description, int LevelVersion = LevelMinVersion) : this(LevelVersion)
        {
            this.Description = Description;
        }
    }
}
