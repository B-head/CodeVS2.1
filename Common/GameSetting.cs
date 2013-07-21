using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class GameSetting
    {
        public readonly int W;
        public readonly int H;
        public readonly int N;
        public readonly int T;
        public readonly int S;
        public readonly int P;
        public readonly int A;
        public readonly int B_obs;
        public readonly int Th;
        public const int D = 3;

        public GameSetting(int w, int h, int n, int t, int s, int p, int a, int b_obs, int th)
        {
            W = w; H = h; N = n; T = t; S = s; P = p; A = a; B_obs = b_obs; Th = th;
        }

        public int GetGameOverLine()
        {
            return T - 1;
        }

        public long FigureChainScore(int e, int c)
        {
            return (1L << Math.Min(e / D, P)) * Math.Max(1, e / D - P + 1) * c;
        }

        public long FigureTurnScore(long raw, int fc)
        {
            return raw * (fc + 1);
        }

        public int GetBlockGrossVolume(int num)
        {
            if (num <= 0) return 0;
            if (num > S) return B_obs;
            int temp = 4 * (num - 1) / S;
            if (temp == 0) return A;
            if (temp == 1) return A * 2;
            return 0;
        }

        public MainField CreateMainField()
        {
            return new MainField(W, H + T, S);
        }

        public static GameSetting ReadHeader(TextReader reader)
        {
            string s = reader.ReadLine();
            if (s == "10 16 4 10 1000")//Small
            {
                return new GameSetting(10, 16, 1000, 4, 10, 25, 400, 1000, 100);
            }
            else if (s == "15 23 4 20 1000")//Medium
            {
                return new GameSetting(15, 23, 1000, 4, 20, 30, 213, 3000, 1000);
            }
            else if (s == "20 36 5 30 1000")//Large
            {
                return new GameSetting(20, 36, 1000, 5, 30, 35, 240, 7200, 10000);
            }
            else
            {
                string[] ss = s.Split(' ');
                int[] p = new int[9];
                for (int i = 0; i < p.Length; i++)
                {
                    p[i] = int.Parse(ss[i]);
                }
                return new GameSetting(p[0], p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8]);
            }
        }
    }
}
