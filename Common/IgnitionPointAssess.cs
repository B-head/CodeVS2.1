using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Common
{
    delegate void IgnitionResultAppraise(ref TurnInfo result, TurnInfo now);

    class IgnitionPointAssess
    {
        readonly int W;
        readonly int H;
        readonly int S;
        readonly int T; 
        readonly GameModel gmCache;
        readonly long[] lightScoreCache;
        readonly int[] sumCache1;
        readonly int[] sumCache2;

        public IgnitionPointAssess(GameModel gm)
        {
            W = gm.Main.Width;
            H = gm.Main.Height;
            S = gm.Setting.S;
            T = gm.Setting.T;
            gmCache = gm.Clone();
            lightScoreCache = new long[S + 1];
            sumCache1 = new int[S];
            sumCache2 = new int[S];
        }

        public TurnInfo FieldAssess(GameModel terget, IgnitionResultAppraise ira)
        {
            TurnInfo result = new TurnInfo();
            MainField field = terget.Main;
            terget.CopyTo(gmCache);
            for (int x = 0; x < W; x++)
            {
                int top, bottom, maxNum;
                bottom = field.GetEarthPoint(x);
                top = bottom - 0;
                maxNum = S / 2;
                if (field.GetEarthPoint(x - 1) >= bottom || field.GetEarthPoint(x + 1) >= bottom)
                {
                    //bottom++;
                    //if (bottom >= field.Height) bottom = field.Height - 1;
                    //maxNum = S;
                }
                for (int y = top; y <= bottom; y++)
                {
                    for (int n = 1; n <= maxNum; n++)
                    {
                        TurnInfo now = gmCache.IgnitionPointForwardStep(n, x, y);
                        terget.CopyTo(gmCache, now.ChangeRect);
                        ira(ref result, now);
                    }
                }
            }
            return result;
        }

        private void CheckValidNumber(MainField field, int x, int y, long[] score)
        {
            score.Initialize();
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        CacheLineSum(field, x, y, 1, 0, sumCache1);
                        CacheLineSum(field, x, y, -1, 0, sumCache2);
                        break;
                    case 1:
                        CacheLineSum(field, x, y, 1, 1, sumCache1);
                        CacheLineSum(field, x, y, -1, -1, sumCache2);
                        break;
                    case 2:
                        CacheLineSum(field, x, y, 0, 1, sumCache1);
                        CacheLineSum(field, x, y, 0, -1, sumCache2);
                        break;
                    case 3:
                        CacheLineSum(field, x, y, 1, -1, sumCache1);
                        CacheLineSum(field, x, y, -1, 1, sumCache2);
                        break;
                }
                for (int a = 0; a < S; a++)
                {
                    if (sumCache1[a] >= S) break;
                    for (int b = 0; b < S; b++)
                    {
                        int temp = sumCache1[a] + sumCache2[b];
                        if (temp >= S) break;
                        temp = S - temp;
                        score[temp] = int.MaxValue;
                    }
                }
            }
        }

        private void CacheLineSum(MainField field, int x, int y, int vx, int vy, int[] sumCache)
        {
            int sum = 0;
            for (int index = 0; index < S; index++)
            {
                sumCache[index] = sum;
                x += vx; y += vy;
                if (sum >= S) break;
                if (!field.IsInField(x, y))
                {
                    sum = S;
                    continue;
                }
                int temp = field[x, y];
                if (temp > 0)
                {
                    sum += temp;
                }
                else
                {
                    sum = S;
                }
            }
        }
    }
}
