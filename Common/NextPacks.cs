using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class NextPacks
    {
        readonly Field[] field;
        readonly int[] left;
        readonly int[] right;
        public readonly int N;
        public readonly int T;

        public NextPacks(int n, int t)
        {
            field = new Field[n * 4];
            left = new int[n * 4];
            right = new int[n * 4];
            N = n; T = t;
        }

        public int TurnToIndex(int turn, int cr)
        {
            return turn * 4 + cr;
        }

        public void GetLeftRight(int index, out int l, out int r)
        {
            l = left[index];
            r = right[index];
        }

        public Field this[int i]
        {
            get
            {
                return field[i];
            }
        }

        public int Length
        {
            get
            {
                return field.Length;
            }
        }

        private void SetPack(Field f, int i)
        {
            int ti = i * 4;
            field[ti] = f;
            LeftRight(field[ti], out left[ti], out right[ti]);
            for (int j = ti + 1; j < ti + 4; j++)
            {
                field[j] = field[j - 1].RotationClone();
                LeftRight(field[j], out left[j], out right[j]);
            }
        }

        private void LeftRight(Field f, out int l, out int r)
        {
            l = f.Width; r = 0;
            int w = f.Width, h = f.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int temp = f[x, y];
                    if (temp <= 0) continue;
                    if (l > x)
                    {
                        l = x;
                    }
                    if (r < x)
                    {
                        r = x;
                    }
                }
            }
        }

        public static NextPacks ReadNextPacks(TextReader reader, int n, int t, int s)
        {
            NextPacks np = new NextPacks(n, t);
            for (int i = 0; i < n; i++)
            {
                Field f = Field.ReadPack(reader, t, s);
                np.SetPack(f, i);
            }
            return np;
        }
    }
}
