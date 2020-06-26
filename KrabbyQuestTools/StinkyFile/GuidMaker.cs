using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile
{
    public class GuidMaker
    {
        private static Random rand = new Random();
        public static HashSet<string> TakenGUIDs = new HashSet<string>();
        public static string GetNextGuid(char leadingChar = 'O')
        {
            string id = default;
            do
            {
                id = leadingChar + rand.Next(999999).ToString();
            } while (TakenGUIDs.Contains(id));
            return id;
        }
        public static void Free(string Guid) => TakenGUIDs.Remove(Guid);
        public static void Reserve(string Guid) => TakenGUIDs.Add(Guid);
    }
}
