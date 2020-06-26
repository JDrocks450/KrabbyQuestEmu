namespace StinkyFile
{
    public struct S_Color
    {
        public byte R, G, B, A;
        public S_Color(byte R, byte G, byte B, byte A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }
        public S_Color(byte R, byte G, byte B) : this(R,G,B,255)
        {

        }

        public override string ToString () => $"{R},{G},{B},{A}";
        public static S_Color Parse(string content)
        {
            var array = content.Split(',');
            S_Color newColor = default;
            for(int i = 0; i < 4; i++)
            {
                byte number = byte.Parse(array[i]);
                switch (i)
                {
                    case 0: newColor.R = number; break;
                    case 1: newColor.G = number; break;
                    case 2: newColor.B = number; break;
                    case 3: newColor.A = number; break;
                }
            }
            return newColor;
        }
    }
}
