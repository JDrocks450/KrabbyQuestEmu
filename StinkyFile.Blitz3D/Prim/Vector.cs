using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Blitz3D.Prim
{
    /// <summary>
    /// A 3D vector
    /// </summary>
    public struct Vector
    {
        public float X, Y, Z;
        public Vector(float uniform) : this(uniform, uniform, uniform)
        {

        }
        public Vector(float x, float y, float z) : this()
        {
            X = x;
            Y = y;
            Z = z;            
        }
        public override string ToString()
        {
            return $"X:{X}, Y:{Y}, Z:{Z}";
        }
    }
}
