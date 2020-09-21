using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinkyFile
{
    public class FontConverter
    {
        const int MAX_FONT_PIECE = 6, COLUMNS = 16, ROWS = 8;
        string[] fontPieces = new string[MAX_FONT_PIECE];
        public FontConverter(string GraphicsDir)
        {
            for(int i = 0; i < MAX_FONT_PIECE; i++)            
                fontPieces[i] = Path.Combine(GraphicsDir, $"fontpiece{i + 1}b.bmp");            
        }

        public void Convert(string DestinationFileName)
        {
            Image img = Image.Load<Rgba32>(fontPieces[0]); // destination image
            int index = 0;
            foreach(var fileName in fontPieces)
            {
                if (index == 0)
                {
                    index++;
                    continue;
                }
                var replaceImage = Image.Load(fontPieces[index]);
                var rowHeight = replaceImage.Height / ROWS;
                replaceImage.Mutate(x => x.Crop(replaceImage.Width, rowHeight));
                img.Mutate(x =>
                {
                    x.DrawImage(replaceImage, new Point(0, index * rowHeight), 1.0f); // take top row and put it at the next available row in destination image
                });
                index++;
            }            
            img.Save(DestinationFileName, new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder());
        }
    }
}
