using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Components
{
    public class FontCreator
    {
        const int ASCIIEnd = 122, ASCIIStart = 32, Columns = 16, Rows = 8;
        private static Font _font;

        public static Font KrabbyQuestFont
        {
            get
            {
                if (_font != null)
                    return _font;
                _font = Resources.Load("Font") as Font;
                return LoadKrabbyQuestFontSettings(_font);
            }
        }

        public static Font LoadKrabbyQuestFontSettings(Font font)
        {            
            var texture = TextureLoader.RequestTexture("Graphics/font.bmp", "0,0,0", true);
            var material = Resources.Load("Materials/Font/Font Material") as Material;
            material.mainTexture = texture;            
            int total = ASCIIEnd - ASCIIStart,
                cRow = 0,
                cColumn = 0;
            CharacterInfo[] info = new CharacterInfo[total];
            for (int i = 0; i < total; i++)
            {
                float pX = cColumn * 16,
                    pY = cRow * (256 / Rows),
                    pW = 16,
                    pH = -(256 / Rows);
                float uvW = pW / 256,
                    uvH = pH / 256,
                    uvX = uvW * cColumn,
                    uvY = uvH * cRow;
                info[i] = new CharacterInfo()
                {
                    uvTopRight = new Vector2(uvX + uvW, uvY),
                    uvTopLeft = new Vector2(uvX, uvY),
                    uvBottomLeft = new Vector2(uvX, uvY + uvH),
                    uvBottomRight = new Vector2(uvX + uvW, uvY + uvH),                    
                    //uv = new Rect(uvX, uvY, uvW, uvH),
                    vert = new Rect(0, 0, pW, pH),
                    advance = (int)pW,
                    index = i,
                    
                };
                cColumn++;
                if (cColumn == Columns)
                {
                    cRow++;
                    cColumn = 0;
                }
            }
            font.characterInfo = info;
            return font;
        }
    }
}
