using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class Field
    {
        byte[,] field;
        public readonly int Width;
        public readonly int Height;
        readonly int obs;
        readonly int wild;

        public Field(int width, int height, int obs = 254, int wild = 255)
        {
            field = new byte[width, height];
            this.Width = width;
            this.Height = height;
            this.obs = obs;
            this.wild = wild;
        }

        public byte this[int x, int y]
        {
            get
            {
                return field[x, y];
            }
            protected set
            {
                this.field[x, y] = value;
            }
        }

        public Field RotationClone()
        {
            Field ret = new Field(Height, Width, obs);
            int h = Height - 1;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    ret[h - y, x] = this[x, y];
                }
            }
            return ret;
        }

        public string GetStringField()
        {
            StringBuilder sb = new StringBuilder(Width * Height * 3);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (x > 0) sb.Append(' ');
                    byte temp = this[x, y];
                    if (temp == 0)
                    {
                        sb.Append("..");
                    }
                    else if (temp == obs)
                    {
                        sb.Append("##");
                    }
                    else if (temp == wild)
                    {
                        sb.Append("**");
                    }
                    else
                    {
                        sb.Append(temp.ToString("D2"));
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static Field ReadPack(TextReader reader, int t, int s)
        {
            Field f = new Field(t, t, s + 1);
            for (int y = 0; y < f.Height; y++)
            {
                string[] strs = reader.ReadLine().Split(' ');
                for (int x = 0; x < f.Width; x++)
                {
                    f[x, y] = byte.Parse(strs[x]);
                }
            }
            if (reader.ReadLine() != "END") throw new ApplicationException();
            return f;
        }
    }
}
