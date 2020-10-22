using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile
{
    public class ArrayEqualityComparer<T> : EqualityComparer<T[]>
    {
        public override bool Equals(T[] x, T[] y)
        {
            if (x.Length != y.Length)            
                return false;            
            for (int i = 0; i < x.Length; i++)            
                if (!x[i].Equals(y[i]))                
                    return false;                            
            return true;
        }

        public bool CheckEquality(T[] x, T[] y) => Equals(x, y);

        public override int GetHashCode(T[] obj)
        {
            int result = 17;
            for (int i = 0; i < obj.Length; i++)
            {
                unchecked
                {
                    result = result * 23 + obj[i].GetHashCode();
                }
            }
            return result;
        }
    }
}
