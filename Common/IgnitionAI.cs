using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class IgnitionStorage : SearchStorage
    {
        internal readonly IgnitionPointAssess ipa;

        public IgnitionStorage(GameModel gm, int nowLevel, long boaderScore)
            : base(gm, nowLevel, boaderScore)
        {
            ipa = new IgnitionPointAssess(gm);
        }
    }

    public class IgnitionAI : BranchAndBoundIDDFS
    {
        readonly int targetErase;
        readonly int targetChain;
        readonly int W;
        readonly int H;
        readonly int T;
        readonly int P3;
        readonly int FieldArea;

        public IgnitionAI(GameModel gm, int maxLevel, bool parallel = true)
            : base(gm, maxLevel, parallel)
        {
            W = gm.Setting.W;
            H = gm.Setting.H;
            T = gm.Setting.T;
            P3 = gm.Setting.P * 3;
            FieldArea = W * H;
            int a = (FieldArea - P3) / 2;
            targetErase = a + P3;
            targetChain = a / 3;
        }

        protected override SearchStorage CreateLocalStorage(GameModel gm, int nowLevel, long boaderScore)
        {
            return new IgnitionStorage(gm, nowLevel, boaderScore);
        }

        protected override TurnInfo AppraiseFunction(GameModel current, SearchStorage ss)
        {
            IgnitionStorage ign = (IgnitionStorage)ss;
            return ign.ipa.FieldAssess(current, ResultAppraise);
        }

        private void ResultAppraise(ref TurnInfo result, TurnInfo now)
        {
            now.AppraisalScore = PartReckonAppraiseScore(now);
            if (result.AppraisalScore <= now.AppraisalScore)
            {
                result = now;
            }
        }

        protected override TurnInfo ReckonAppraiseScore(TurnInfo forwardInfo, TurnInfo appraiseInfo)
        {
            if (forwardInfo.AppraisalScore < 0)
            {
                return forwardInfo;
            }
            double eb = (double)(forwardInfo.EraseBlock + forwardInfo.EraseObstacle) / FieldArea;
            if (eb >= 0.7)
            {
                forwardInfo.AppraisalScore = PartReckonAppraiseScore(forwardInfo);
            }
            else
            {
                forwardInfo.AppraisalScore = 0;
            }
            //appraiseInfo.AppraisalScore = (long)(appraiseInfo.AppraisalScore * (1 - eb));
            return TurnInfo.MaxInfo(forwardInfo, appraiseInfo);
        }

        private long PartReckonAppraiseScore(TurnInfo info)
        {
            double e = (double)info.MaxEraseCount / targetErase;
            double c = (double)info.MaxEraseChain / targetChain;
            double b = (double)(H + T - info.MostBottom) / H;
            double w = (double)info.EraseWidth / W;
            double s = (double)info.MaxEraseCount / (info.TotalEraseCount + 1);
            return (long)(int.MaxValue * (e + c + b + w));
        }
    }
}
