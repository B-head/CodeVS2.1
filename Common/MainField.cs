using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Common
{
    public class MainField : Field
    {
        public readonly int S; 
        bool[,] eraseFlag;
        int[] earthPoint;
        int[] floatPoint;

        public MainField(int width, int height, int s) : base(width, height, s + 1)
        {
            S = s;
            eraseFlag = new bool[Width, Height];
            earthPoint = new int[Width];
            floatPoint = new int[Width];
            for (int i = 0; i < Width; i++)
            {
                earthPoint[i] = Height - 1;
                floatPoint[i] = Height;
            }
        }

        public MainField Clone()
        {
            MainField ret = new MainField(Width, Height, S);
            CopyTo(ret);
            return ret;
        }

        public void CopyTo(MainField to)
        {
            Rect rect = new Rect() { Left = 0, Top = 0, Right = Width - 1, Bottom = Height - 1 };
            CopyTo(to, rect);
        }

        public void CopyTo(MainField to, Rect rect)
        {
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                for (int y = rect.Top; y <= rect.Bottom; y++)
                {
                    to[x, y] = this[x, y];
                }
            }
            for (int i = 0; i < Width; i++)
            {
                to.earthPoint[i] = earthPoint[i];
                to.floatPoint[i] = floatPoint[i];
            }
        }

        public bool IsLineExistBlock(int y)
        {
            for (int x = 0; x < Width; x++)
            {
                if (this[x, y] > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsInField(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public void InFieldRect(ref Rect rect)
        {
            rect.Left = Math.Max(rect.Left, 0);
            rect.Top = Math.Max(rect.Top, 0);
            rect.Right = Math.Min(rect.Right, Width - 1);
            rect.Bottom = Math.Min(rect.Bottom, Height - 1);
        }

        public Rect CreateReverseRect()
        {
            return new Rect { Left = Width - 1, Top = Height - 1, Right = 0, Bottom = 0 };
        }

        public int GetUnevenness()
        {
            int ret = 0;
            for (int x = 0; x < Width - 1; x++)
            {
                ret += Math.Abs(earthPoint[x] - earthPoint[x + 1]);
            }
            return ret;
        }

        public int GetSpace(int deadLine)
        {
            int ret = 0;
            for (int x = 0; x < Width; x++)
            {
                ret += earthPoint[x] - deadLine;
            }
            return ret;
        }

        public int GetMostBottom()
        {
            int ret = 0;
            for (int x = 0; x < Width; x++)
            {
                ret = Math.Max(ret, earthPoint[x]);
            }
            return ret;
        }

        public int GetEarthPoint(int x)
        {
            if (x < 0 || x >= Width) return -1;
            return earthPoint[x];
        }

        public int SetBlock(int num, int x, int y)
        {
            this[x, y] = (byte)num;
            if (floatPoint[x] > y)
            {
                floatPoint[x] = y;
                earthPoint[x] = y - 1;
            }
            for (y++; y < Height && this[x, y] == 0; y++)
            {
                this[x, y] = (byte)(S + 1);
            }
            return --y;
        }

        public Rect FixPack(Field pack, int cx)
        {
            Rect rect = CreateReverseRect();
            int w = pack.Width, h = pack.Height;
            for (int x = 0; x < w; x++)
            {
                int tx = x + cx;
                if (tx < 0 || tx >= Width) continue;
                int bottom = earthPoint[tx];
                for (int y = h - 1; y >= 0; y--)
                {
                    byte num = pack[x, y];
                    if (num <= 0) continue;
                    this[tx, earthPoint[tx]--] = num;
                }
                int nowfp = earthPoint[tx] + 1;
                if (bottom < nowfp) continue;
                rect.MaxRect(tx, nowfp, tx, bottom);
                floatPoint[tx] = nowfp;
            }
            return rect;
        }

        public int FallingBlocks(ref Rect rect)
        {
            int fallingCount = 0;
            Rect resultRect = CreateReverseRect();
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                int top = floatPoint[x], bottom = earthPoint[x];
                for (int y = bottom; y >= top; y--)
                {
                    byte num = this[x, y];
                    if (num <= 0) continue;
                    this[x, y] = 0;
                    this[x, earthPoint[x]--] = num;
                    fallingCount++;
                }
                int nowfp = earthPoint[x] + 1;
                if (bottom < nowfp) continue;
                resultRect.MaxRect(x, nowfp, x, bottom);
                floatPoint[x] = nowfp;
            }
            rect = resultRect;
            return fallingCount;
        }

        private void AdjustEarthFlort(ref Rect rect)
        {
            Rect resultRect = CreateReverseRect();
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                if (rect.Bottom < earthPoint[x]) continue;
                for (int y = rect.Bottom; y >= rect.Top; y--)
                {
                    if (this[x, y] <= 0)
                    {
                        earthPoint[x] = y;
                        break;
                    }
                }
                if (floatPoint[x] > earthPoint[x]) continue;
                resultRect.MaxRect(x, floatPoint[x], x, earthPoint[x]);
            }
            rect = resultRect;
        }

        public int EraseBlocks(ref Rect rect, ref int eraseBlock, ref int eraseObstacle)
        {
            int eraseCount = CheckRectSum(ref rect);
            if (eraseCount <= 0)
            {
                return eraseCount;
            }
            eraseCount += RectEraseObstacles(ref rect, ref eraseBlock, ref eraseObstacle);
            AdjustEarthFlort(ref rect);
            return eraseCount;
        }

        private int CheckRectSum(ref Rect rect)
        {
            Rect resultRect = CreateReverseRect();
            int tx, ty, length, eraseSum = 0;
            int w = rect.Right - rect.Left + 1, h = rect.Bottom - rect.Top + 1, side = w < h ? w : h;
            for (int y = rect.Top; y <= rect.Bottom; y++)
            {
                tx = rect.Left; ty = y; 
                length = w;
                eraseSum += CheckLineSum(ref tx, ref ty, 1, 0, ref length);
                resultRect.MaxRect(tx, ty, tx + length, ty);

                tx = rect.Left; ty = y; 
                length = Math.Min(side, rect.Top + h - y);
                eraseSum += CheckLineSum(ref tx, ref ty, 1, 1, ref length);
                resultRect.MaxRect(tx, ty, tx + length, ty + length);

                tx = rect.Left; ty = y; 
                length = Math.Min(side, y - rect.Top + 1);
                eraseSum += CheckLineSum(ref tx, ref ty, 1, -1, ref length);
                resultRect.MaxRect(tx, ty - length, tx + length, ty);
            }
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                tx = x; ty = rect.Top; 
                length = h;
                eraseSum += CheckLineSum(ref tx, ref ty, 0, 1, ref length);
                resultRect.MaxRect(tx, ty, tx, ty + length);
                if (x == rect.Left) continue;

                tx = x; ty = rect.Top; 
                length = Math.Min(side, rect.Left + w - x);
                eraseSum += CheckLineSum(ref tx, ref ty, 1, 1, ref length);
                resultRect.MaxRect(tx, ty, tx + length, ty + length);

                tx = x; ty = rect.Bottom; 
                length = Math.Min(side, rect.Left + w - x);
                eraseSum += CheckLineSum(ref tx, ref ty, 1, -1, ref length);
                resultRect.MaxRect(tx, ty - length, tx + length, ty);
            }
            InFieldRect(ref resultRect);
            rect = resultRect;
            return eraseSum;
        }

        private int CheckLineSum(ref int x, ref int y, int vx, int vy, ref int length)
        {
            int eraseSum = 0, e = 0, inErase = 0;
            PartCheckLineSum(out e, ref x, ref y, -vx, -vy);
            length += e - 1;
            int ix = x, iy = y;
            for (int i = 0; i < length || i < inErase; i++)
            {
                if (i < length)
                {
                    if (PartCheckLineSum(out e, ix, iy, vx, vy))
                    {
                        eraseSum += e; inErase = i + e;
                    }
                }
                if (i < inErase)
                {
                    eraseFlag[ix, iy] = true;
                }
                ix += vx; iy += vy;
            }
            length = Math.Max(length - 1, inErase - 1);
            return eraseSum;
        }

        private bool PartCheckLineSum(out int e, int x, int y, int vx, int vy)
        {
            return PartCheckLineSum(out e, ref x, ref y, vx, vy);
        }

        private bool PartCheckLineSum(out int e, ref int x, ref int y, int vx, int vy)
        {
            int sum = 0; 
            e = 1;
            while (IsInField(x, y))
            {
                int temp = this[x, y];
                if (temp <= 0 || temp > S) break;
                sum += temp; 
                if (sum == S)
                {
                    return true;
                }
                else if (sum > S)
                {
                    break;
                }
                x += vx; y += vy;
                e++;
            }
            return false;
        }

        private int RectEraseObstacles(ref Rect rect, ref int eraseBlock, ref int eraseObstacle)
        {
            int eraseSum = 0;
            Rect resultRect = CreateReverseRect();
            for (int x = rect.Left; x <= rect.Right; x++)
            {
                for (int y = rect.Top; y <= rect.Bottom; y++)
                {
                    if (eraseFlag[x, y] == false) continue;
                    int e = EraseObstacles(x, y);
                    this[x, y] = 0;
                    eraseFlag[x, y] = false;
                    eraseBlock++;
                    eraseObstacle += e;
                    eraseSum += e;
                }
            }
            rect.MaxRect(rect.Left - 1, rect.Top - 1, rect.Right + 1, rect.Bottom + 1);
            InFieldRect(ref rect);
            return eraseSum;
        }

        private int EraseObstacles(int x, int y)
        {
            int e = 0;
            for (int i = 0; i < 8; i++)
            {
                int tx = x, ty = y;
                switch (i)
                {
                    case 0: tx += 1; break;
                    case 1: tx += 1; ty += 1; break;
                    case 2: ty += 1; break;
                    case 3: tx -= 1; ty += 1; break;
                    case 4: tx -= 1; break;
                    case 5: tx -= 1; ty -= 1; break;
                    case 6: ty -= 1; break;
                    case 7: tx += 1; ty -= 1; break;
                }
                if (!IsInField(tx, ty)) continue;
                if (this[tx, ty] > S)
                {
                    this[tx, ty] = 0;
                    e++;
                }
            }
            return e;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(Width * 3 - 1);
            for (int x = 0; x < Width; x++)
            {
                if (x > 0) sb.Append(" ");
                int temp = Height - earthPoint[x] - 1;
                sb.Append(temp.ToString("D2"));
            }
            return sb.ToString();
        }
    }
}
