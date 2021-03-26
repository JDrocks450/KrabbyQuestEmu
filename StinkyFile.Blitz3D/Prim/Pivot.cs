using StinkyFile.Primitive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Prim
{
    /// <summary>
    /// Blitz3D Animation Pivot point
    /// </summary>
    public class Pivot : BlitzObject
    {
        [EditorVisible()]
        public int Vertex { get; set; }
        [EditorVisible()]
        public float Weight { get; set; }
    }
}
