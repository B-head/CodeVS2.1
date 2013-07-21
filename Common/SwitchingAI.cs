using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class SwitchingAI : GameAI
    {
        FireAI fire;
        IgnitionAI standard;

        public SwitchingAI(GameModel gm, int maxLevel1, int maxLevel2)
        {
            fire = new FireAI(gm, maxLevel1);
            standard = new IgnitionAI(gm, maxLevel2);
        }

        public TurnInfo NextControl(GameModel gm, out int cx, out int cr)
        {
            TurnInfo turnInfo;
            if (gm.Turn < 500)
            {
                turnInfo = fire.NextControl(gm, out cx, out cr);
            }
            else
            {
                turnInfo = standard.NextControl(gm, out cx, out cr);
            }
            return turnInfo;
        }
    }
}
