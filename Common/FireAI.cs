using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Common
{
    public class FireAI : BranchAndBoundIDDFS
    {
        readonly int Th;

        public FireAI(GameModel gm, int maxLevel, bool parallel = true)
            : base(gm, maxLevel, parallel)
        {
            Th = gm.Setting.Th;
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
            if (now.RawScore >= Th)
            {
                now.AppraisalScore = (long)Th * Th / now.EraseBlock;
            }
            else if (now.RawScore > 0)
            {
                now.AppraisalScore = now.RawScore / now.EraseBlock;
            }
            else
            {
                now.AppraisalScore = 0;
            }
            result.AppraisalScore += now.AppraisalScore;
        }

        protected override TurnInfo ReckonAppraiseScore(TurnInfo forwardInfo, TurnInfo appraiseInfo)
        {
            if(forwardInfo.AppraisalScore < 0)
            {
                return forwardInfo;
            }
            TurnInfo nowInfo = TurnInfo.MaxInfo(forwardInfo, appraiseInfo);
            long score = appraiseInfo.AppraisalScore;
            if (forwardInfo.RawScore >= Th)
            {
                score += (long)Th * Th * Th / forwardInfo.EraseBlock;
            }
            else if (forwardInfo.EraseBlock > 0)
            {
                score /= forwardInfo.EraseBlock;
            }
            nowInfo.AppraisalScore = score;
            return nowInfo;
        }
    }
}
