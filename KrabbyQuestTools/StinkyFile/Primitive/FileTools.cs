using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Primitive
{
    internal static class FileTools
    {
        /// <summary>
        /// Reads an 32 bit signed integer, at the index
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="Position">The position to read at</param>
        /// <param name="nPosition">The new file position after reading</param>
        /// <returns></returns>
        public static Int32 ReadInt(byte[] fileData, int Position, out int nPosition)
        {
            nPosition = Position + 4;
            return BitConverter.ToInt32(fileData, Position);
        }

        /// <summary>
        /// Reads a string by taking the length first, then returning the string data
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <param name="Position">The position to read at</param>
        /// <param name="nPosition">The new file position after reading</param>
        /// <returns></returns>
        public static String ReadString(byte[] fileData, int Position, out int nPosition)
        {
            int strLen = ReadInt(fileData, Position, out Position);
            nPosition = Position + strLen;
            return Encoding.ASCII.GetString(fileData, Position, strLen); 
        }

    }
}
