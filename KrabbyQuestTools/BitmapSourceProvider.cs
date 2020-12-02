using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace KrabbyQuestTools
{
    public static class BitmapSourceProvider
    {
        private static Dictionary<string, BitmapImage> Sources = new Dictionary<string, BitmapImage>();
        public static BitmapImage Load(Uri fileName)
        {
            if (Sources.TryGetValue(fileName.OriginalString, out var source))
                return source;
            source = new BitmapImage();
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.UriSource = fileName;
            source.EndInit();
            Sources.Add(fileName.OriginalString, source);
            return source;
        }
    }
}
