using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile.Primitive
{
    public static class FileTools
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
        /// Reads an 32 bit signed integer, at the filestream position
        /// </summary>
        /// <param name="fileData">The file data</param>
        /// <returns></returns>
        public static Int32 ReadInt(FileStream fileData)
        {
            byte[] buffer = new byte[4];
            fileData.Read(buffer, 0, 4);
            return ReadInt(buffer, 0, out _);
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
        public static String ReadString(FileStream fileData, int length)
        {            
            byte[] buffer = new byte[length];
            fileData.Read(buffer, 0, length);
            return Encoding.ASCII.GetString(buffer);
        }

        /// <summary>
        /// Reads a Single at the filestream position
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public static float ReadFloat(FileStream fileData)
        {
            byte[] buffer = new byte[4];
            fileData.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }
    }
}
