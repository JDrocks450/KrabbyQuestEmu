using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Primitive
{
    /// <summary>
    /// A simple reference to a primitive data type
    /// </summary>
    public class RefPrim<T>
    {
        /// <summary>
        /// Creates a new <see cref="RefPrim{T}"/> of type <see cref="T"/> with value
        /// </summary>
        public RefPrim()
        {
        }

        /// <summary>
        /// Creates a new <see cref="RefPrim{T}"/> of type <see cref="T"/> with value
        /// </summary>
        /// <param name="Value"></param>
        public RefPrim(T Value) : this()
        {
            this.Value = Value;
        }
        /// <summary>
        /// The value of this reference primitive data type
        /// </summary>
        public T Value
        {
            get; set;
        }
    }
}
