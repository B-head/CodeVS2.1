using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace Common
{

    public class SearchStorage
    {
        internal readonly GameModelCache GMC;
        public readonly int NowLevel;
        public readonly long BoaderScore;
        public long MinimaxScore;
        public long CallCount;
        public TurnInfo resultInfo;

        protected SearchStorage(GameModel gm, int nowLevel, long boaderScore)
        {
            GMC = new GameModelCache(nowLevel, gm);
            NowLevel = nowLevel;
            BoaderScore = boaderScore;
            MinimaxScore = long.MinValue;
            CallCount = 0;
            resultInfo = new TurnInfo();
        }
    }

    public abstract class BranchAndBoundIDDFS : GameAI
    {
        readonly int maxLevel;
        readonly bool parallel;
        readonly int W;
        readonly int T;
        readonly int[] rankBoader;
        AppraisalTree rootNode;

        protected abstract SearchStorage CreateLocalStorage(GameModel gm, int nowLevel, long boaderScore);
        protected abstract TurnInfo AppraiseFunction(GameModel current, SearchStorage ss);
        protected abstract TurnInfo ReckonAppraiseScore(TurnInfo forwardInfo, TurnInfo assessInfo);

        public BranchAndBoundIDDFS(GameModel gm, int maxLevel, bool parallel)
        {
            this.maxLevel = maxLevel;
            this.parallel = parallel;
            W = gm.Setting.W;
            T = gm.Setting.T;
            rankBoader = new int[maxLevel + 1];
            rootNode = new AppraisalTree();
            int rank = (W + T - 1) * 4 - 1;
            for (int i = 1; i <= maxLevel; i++)
            {
                if (rank < 1) rank = 1;
                rankBoader[i] = rank;
                rank /= 1;
            }
        }

        public TurnInfo NextControl(GameModel gm, out int cx, out int cr)
        {
            long callCount = 0, boaderScore = 0;
            int nowLevel = 0, ci = 0;
            TurnInfo turnInfo = new TurnInfo();
            try
            {
                while (!MaxToControl(out ci, boaderScore) && nowLevel++ < maxLevel)
                {
                    if (!parallel)
                    {
                        SearchStorage ss = CreateLocalStorage(gm, nowLevel, boaderScore);
                        turnInfo = SearchNode(rootNode, 0, ss);
                        boaderScore = ss.MinimaxScore;
                        callCount = ss.CallCount;
                    }
                    else
                    {
                        long minScore = long.MaxValue;
                        long minimaxScore = 0;
                        SpinLock sl = new SpinLock();
                        if (!rootNode.IsExistChild())
                        {
                            rootNode.GenerateChild(W, T);
                        }
                        ParallelLoopResult plr = Parallel.For<SearchStorage>(0, rootNode.Length,
                            () => CreateLocalStorage(gm, nowLevel, boaderScore),
                            (index, state, local) =>
                            {
                                local.MinimaxScore = long.MinValue;
                                local.CallCount = 0;
                                Debug.Assert(local.GMC.Level == 1);
                                local.resultInfo = AppraiseNode(rootNode[index], 1, 0, local);
                                return local;
                            },
                            (local) =>
                            {
                                bool lockTaken = false;
                                while (!lockTaken) sl.Enter(ref lockTaken);
                                if (turnInfo.AppraisalScore < local.resultInfo.AppraisalScore)
                                {
                                    turnInfo = local.resultInfo;
                                }
                                if (local.resultInfo.AppraisalScore >= local.BoaderScore)
                                {
                                    minScore = Math.Min(minScore, local.resultInfo.AppraisalScore);
                                }
                                minimaxScore = Math.Max(minimaxScore, local.MinimaxScore);
                                callCount += local.CallCount;
                                sl.Exit();
                            }
                        );
                        Debug.Assert(plr.IsCompleted);
                        if (minScore < long.MaxValue)
                        {
                            boaderScore = Math.Max(minimaxScore, minScore);
                        }
                        else
                        {
                            boaderScore = minimaxScore;
                        }
                    }
                    Trace.TraceInformation("Turn {0}, Level {1}, CallCount {2}, BoaderScore {3}, {4}, {5}",
                        gm.Turn + 1, nowLevel, callCount, boaderScore, turnInfo.ToString(), rootNode.ToString());
                }
            }
            catch (OutOfMemoryException e)
            {
                Trace.TraceWarning("{0}", e.ToString());
            }
            if (ci != -1)
            {
                rootNode = rootNode[ci];
                cx = rootNode.CX;
                cr = rootNode.CR;
            }
            else
            {
                rootNode = new AppraisalTree();
                cx = 0; cr = 0;
            }
            return turnInfo;
        }

        private bool MaxToControl(out int ci, long boaderScore)
        {
            ci = -1;
            if (!rootNode.IsExistChild()) return false;
            long maxScore = 0;
            int count = 0, length = rootNode.Length;
            for (int i = 0; i < length; i++)
            {
                long temp = rootNode[i].Info.AppraisalScore;
                if (maxScore <= temp)
                {
                    maxScore = temp;
                    ci = i;
                }
                if (boaderScore <= temp)
                {
                    count++;
                }
            }
            return count == 1;
        }

        private TurnInfo SearchNode(AppraisalTree node, int level, SearchStorage ss)
        {
            if (!node.IsExistChild())
            {
                node.GenerateChild(W, T);
            }
            int length = node.Length;
            TurnInfo maxInfo = new TurnInfo { AppraisalScore = long.MinValue };
            long minScore = long.MaxValue;
            //long boader = GetRankBoaderScore(node, level + 1);
            long boader = ss.BoaderScore;
            for (int i = 0; i < length; i++)
            {
                TurnInfo nextInfo = AppraiseNode(node[i], level + 1, boader, ss);
                if (maxInfo.AppraisalScore < nextInfo.AppraisalScore)
                {
                    maxInfo = nextInfo;
                }
                if (nextInfo.AppraisalScore >= boader)
                {
                    minScore = Math.Min(minScore, nextInfo.AppraisalScore);
                }
            }
            if (minScore < long.MaxValue)
            {
                ss.MinimaxScore = Math.Max(ss.MinimaxScore, minScore);
            }
            return maxInfo;
        }

        private long GetRankBoaderScore(AppraisalTree node, int level)
        {
            node.Sort();
            int r = rankBoader[level];
            return node[r].Info.AppraisalScore;
        }

        private TurnInfo AppraiseNode(AppraisalTree node, int level, long boader, SearchStorage ss)
        {
            TurnInfo nowInfo = node.Info;
            TurnInfo forwardInfo = new TurnInfo();
            TurnInfo appraiseInfo = new TurnInfo();
            if (nowInfo.AppraisalScore != long.MinValue)
            {
                if (nowInfo.AppraisalScore < boader)
                {
                    node.ReleaseChild();
                    return nowInfo;
                }
                else if(level >= ss.NowLevel)
                {
                    return nowInfo;
                }
            }
            forwardInfo = ss.GMC.ForwardStep(node.CX, node.CR);
            if (forwardInfo.AppraisalScore >= 0)
            {
                if (level >= ss.NowLevel)
                {
                    appraiseInfo = AppraiseFunction(ss.GMC.GetCurrentGameModel(), ss);
                    ss.CallCount++;
                }
                else
                {
                    ss.GMC.NextLevel();
                    appraiseInfo = SearchNode(node, level, ss);
                    ss.GMC.PreviousLevel();
                }
            }
            ss.GMC.BackwardStep();
            nowInfo = ReckonAppraiseScore(forwardInfo, appraiseInfo);
            node.Info = nowInfo;
            return nowInfo;
        }
    }
}
