using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Prim
{
    /// <summary>
    /// A 3D Quaternion
    /// </summary>
    public struct Quat
    {
        public float W;
        public Vector V;
        public Quat(float W, Vector V)
        {
            this.W = W;
            this.V = V;
        }
        public override string ToString()
        {
            return $"W:{W}, {V}";
        }
    }
}
