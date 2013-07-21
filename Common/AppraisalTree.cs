using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class AppraisalTree : IComparable<AppraisalTree>
    {
        public readonly int CX;
        public readonly int CR;
        public TurnInfo Info;
        AppraisalTree[] child;

        public AppraisalTree(int cx = 0, int cr = 0)
        {
            CX = cx;
            CR = cr;
            Info = new TurnInfo { AppraisalScore = long.MinValue };
        }

        public int CompareTo(AppraisalTree other)
        {
            if (Info.AppraisalScore < other.Info.AppraisalScore)
            {
                return -1;
            }
            else if (Info.AppraisalScore > other.Info.AppraisalScore)
            {
                return 1;
            }
            else
            {
                int a = CX * 4 + CR, b = other.CX * 4 + other.CR;
                if (a < b)
                {
                    return -1;
                }
                else if (a > b)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void Sort()
        {
            Array.Sort<AppraisalTree>(child);
            Array.Reverse(child);
        }

        public bool IsExistChild()
        {
            return child != null;
        }

        public void GenerateChild(int w, int t)
        {
            int length = (w + t - 1) * 4;
            int minX = 1 - t;
            int maxX = w - 1;
            child = new AppraisalTree[length];
            for (int x = minX; x <= maxX; x++)
            {
                for (int r = 0; r < 4; r++)
                {
                    int i = ((x + t - 1) << 2) + r;
                    child[i] = new AppraisalTree(x, r);
                }
            }
        }

        public void ReleaseChild()
        {
            child = null;
        }

        public AppraisalTree this[int i]
        {
            get
            {
                return child[i];
            }
            set
            {
                child[i] = value;
            }
        }

        public int Length
        {
            get
            {
                return child.Length;
            }
        }

        public override string ToString()
        {
            if (child == null) return "null";
            string ret = string.Empty;
            for (int i = 0; i < Length; i++)
            {
                if (i > 0) ret += " ";
                ret += child[i].Info.AppraisalScore.ToString();
            }
            return ret;
        }
    }
}
