using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace TestClient
{
    class GenerateInput
    {
        public static StringReader SmallGenerate(Random rand, int n)
        {
            double c = n / 1000.0;
            int a = (int)(400 * c), b_obs = (int)(1000 * c);
            return Generate(rand, 10, 16, n, 4, 10, 25, a, b_obs, 100);
        }

        public static StringReader MediumGenerate(Random rand, int n)
        {
            double c = n / 1000.0;
            int a = (int)(213 * c), b_obs = (int)(3000 * c);
            return Generate(rand, 15, 23, n, 4, 20, 30, a, b_obs, 1000);
        }

        public static StringReader LargeGenerate(Random rand, int n)
        {
            double c = n / 1000.0;
            int a = (int)(240 * c), b_obs = (int)(7200 * c);
            return Generate(rand, 20, 36, n, 5, 30, 35, a, b_obs, 10000);
        }

        public static StringReader Generate(Random rand, int w, int h, int n, int t, int s, int p, int a, int b_obs, int th)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} {1} {2} {3} {4} {5} {6} {7} {8}", w, h, n, t, s, p, a, b_obs, th);
            sb.AppendLine();
            int total;
            int[] amount = GenerateAmount(out total, s, a, b_obs);
            byte[] block = GenerateBlockLine(amount, total);
            ShuffleLine(rand, block);
            byte[][] packs = GeneratePackLine(block, n, t);
            ShuffleLine(rand, packs);
            for (int i = 0; i < packs.Length; i++)
            {
                ShuffleLine(rand, packs[i]);
                AppendPack(sb, packs[i], t, s);
            }
            return new StringReader(sb.ToString());
        }

        private static void AppendPack(StringBuilder sb, byte[] pack, int t, int s)
        {
            for (int y = 0; y < t; y++)
            {
                for (int x = 0; x < t; x++)
                {
                    if (x > 0) sb.Append(' ');
                    byte temp = pack[x + y * t];
                    sb.Append(temp);
                }
                sb.AppendLine();
            }
            sb.AppendLine("END");
        }

        private static void ShuffleLine<T>(Random rand, T[] array)
        {
            for (int i = 0; i < array.Length; i++)
            {
                int a = rand.Next(i, array.Length);
                T temp = array[i];
                array[i] = array[a];
                array[a] = temp;
            }
        }

        private static int[] GenerateAmount(out int total, int s, int a, int b_obs)
        {
            int[] amount = new int[s + 2];
            total = 0;
            for (int i = 0; i < amount.Length; i++)
            {
                int temp = 0;
                if (i == s + 1)
                {
                    temp = b_obs;
                }
                else if (i > 0)
                {
                    int d = 4 * (i - 1) / s;
                    if (d == 0)
                    {
                        temp = a;
                    }
                    else if (d == 1)
                    {
                        temp = a * 2;
                    }
                }
                amount[i] = temp;
                total += temp;
            }
            return amount;
        }

        private static byte[] GenerateBlockLine(int[] amount, int total)
        {
            byte[] block = new byte[total];
            int a = 0;
            for (int i = 0; i < amount.Length; i++)
            {
                while (amount[i]-- > 0)
                {
                    block[a++] = (byte)i;
                }
            }
            Debug.Assert(a == total);
            return block;
        }

        private static byte[][] GeneratePackLine(byte[] block, int n, int t)
        {
            byte[][] packs = new byte[n][];
            int a = 0, total = block.Length;
            for (int i = 0; i < packs.Length; i++)
            {
                byte[] temp = new byte[t * t];
                int count = i < total % n ? total / n + 1 : total / n;
                for (int j = 0; j < count; j++)
                {
                    temp[j] = block[a++];
                }
                packs[i] = temp;
            }
            Debug.Assert(a == total);
            return packs;
        }
    }
}
