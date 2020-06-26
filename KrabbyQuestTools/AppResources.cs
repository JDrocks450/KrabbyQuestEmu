using StinkyFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace KrabbyQuestTools
{
    public static class AppResources
    {
        public static StinkyParser Parser { get; set; }

        public static Color S_ColorConvert(S_Color color) => Color.FromArgb(color.A, color.R, color.G, color.B);
    }
}
