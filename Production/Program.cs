using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Common;

namespace Production
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GameModel gm = GameModel.ReadGameModel(Console.In);
                GameAI ai = SerectAI(gm);
                for (int i = 0; i < gm.Setting.N; i++)
                {
                    int cx, cr;
                    ai.NextControl(gm, out cx, out cr);
                    OutputControl(cx, cr);
                    TurnInfo lastInfo = gm.ForwardStep(cx, cr);
                    Trace.TraceInformation("{0}, CX {1}, CR {2}, {3}", gm.ToString(), cx, cr, lastInfo.ToString());
                    if (lastInfo.AppraisalScore < 0) break;
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("{0}", e.ToString());
                Console.WriteLine(e.ToString());
            }
            Trace.Close();
        }

        public static GameAI SerectAI(GameModel gm)
        {
            if (gm.Setting.W == 10)
            {
                return new SwitchingAI(gm, 2, 4);
            }
            else if (gm.Setting.W == 15)
            {
                return new SwitchingAI(gm, 2, 4);
            }
            else if (gm.Setting.W == 20)
            {
                return new SwitchingAI(gm, 2, 4);
            }
            else
            {
                throw new ApplicationException();
            }
        }

        public static void OutputControl(int x, int r)
        {
            Console.WriteLine("{0:D} {1:D}", x, r);
        }
    }
}
