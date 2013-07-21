using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class GameModel
    {
        public readonly GameSetting Setting;
        public readonly NextPacks Next;
        public readonly MainField Main;
        public int Turn { get; private set; }
        public long Score { get; private set; }
        public long RawScore { get; private set; }
        public long ExtraScore { get; private set; }
        public int FireCount { get; private set; }
        public int MaxChain { get; private set; }
        public int MaxSameErase { get; private set; }

        public GameModel(GameSetting setting, NextPacks next, MainField main)
        {
            Setting = setting;
            Next = next;
            Main = main;
        }

        public GameModel Clone()
        {
            return new GameModel(Setting, Next, Main.Clone())
            {
                Turn = Turn,
                Score = Score,
                RawScore = RawScore,
                ExtraScore = ExtraScore,
                FireCount = FireCount,
                MaxChain = MaxChain,
                MaxSameErase = MaxSameErase,
            };
        }

        public void CopyTo(GameModel to)
        {
            Rect rect = new Rect() { Left = 0, Top = 0, Right = Main.Width - 1, Bottom = Main.Height - 1 };
            CopyTo(to, rect);
        }

        public void CopyTo(GameModel to, Rect rect)
        {
            to.Turn = Turn;
            to.Score = Score;
            to.RawScore = RawScore;
            to.ExtraScore = ExtraScore;
            to.FireCount = FireCount;
            to.MaxChain = MaxChain;
            to.MaxSameErase = MaxSameErase;
            Main.CopyTo(to.Main, rect);
        }

        public Field GetNextPack(int cr)
        {
            int ni = Next.TurnToIndex(Turn++, cr);
            return Next[ni];
        }

        private TurnInfo CreateGameOverInfo(Rect changeRect = new Rect())
        {
            return new TurnInfo
            {
                AppraisalScore = -1,
                ChangeRect = changeRect,
            };
        }

        public TurnInfo ForwardStep(int cx, int cr)
        {
            if (Setting.N <= Turn || !IsPossibleFix(cx, cr) || Main.IsLineExistBlock(Setting.GetGameOverLine()))
            {
                return CreateGameOverInfo();
            }
            Rect rect = Main.FixPack(GetNextPack(cr), cx);
            TurnInfo info = ChainLoop(rect);
            Score += info.AppraisalScore;
            RawScore += info.RawScore;
            ExtraScore += info.ExtraScore;
            if (info.RawScore >= Setting.Th) FireCount++;
            MaxChain = Math.Max(MaxChain, info.Chain);
            MaxSameErase = Math.Max(MaxSameErase, info.MaxEraseCount);
            if (Main.IsLineExistBlock(Setting.GetGameOverLine()))
            {
                return CreateGameOverInfo(info.ChangeRect);
            }
            return info;
        }

        public TurnInfo IgnitionPointForwardStep(int num, int x, int y)
        {
            int ry = Main.SetBlock(num, x, y);
            Rect rect = new Rect { Left = x, Top = y, Right = x, Bottom = ry };
            return ChainLoop(rect);
        }

        public TurnInfo ChainLoop(Rect rect)
        {
            Rect changeRect = rect;
            Rect eraseRect = Main.CreateReverseRect();
            long rawScore = 0, extraScore = 0;
            int c = 0, e = 0, eraseCount = 0, eraseBlock = 0, eraseObstacle = 0,
                maxEraseCount = 0, maxEraseChain = 0, fallingCount = 0;
            while ((e = Main.EraseBlocks(ref rect, ref eraseBlock, ref eraseObstacle)) > 0)
            {
                eraseRect.MaxRect(rect);
                rawScore += Setting.FigureChainScore(e, ++c);
                eraseCount += e;
                if (maxEraseCount <= e)
                {
                    maxEraseCount = e;
                    maxEraseChain = c;
                }
                extraScore += e * e * c;
                fallingCount += Main.FallingBlocks(ref rect);
            }
            long assessScore = Setting.FigureTurnScore(rawScore, FireCount);
            changeRect.MaxRect(eraseRect);
            int eraseWidth = eraseRect.Right - eraseRect.Left + 1;
            return new TurnInfo
            {
                AppraisalScore = assessScore,
                RawScore = rawScore,
                ExtraScore = extraScore,
                Chain = c,
                TotalEraseCount = eraseCount,
                EraseBlock = eraseBlock,
                EraseObstacle = eraseObstacle,
                EraseWidth = eraseWidth < 0 ? 0 : eraseWidth,
                MaxEraseCount = maxEraseCount,
                MaxEraseChain = maxEraseChain,
                MostBottom = Main.GetMostBottom(),
                FieldSpace = Main.GetSpace(Setting.GetGameOverLine()),
                FallingCount = fallingCount,
                ChangeRect = changeRect,
            };
        }

        private bool IsPossibleFix(int cx, int cr)
        {
            if (Setting.N <= Turn) return false;
            int l, r, ni = Next.TurnToIndex(Turn, cr);
            Next.GetLeftRight(ni, out l, out r);
            return cx + l >= 0 && cx + r < Setting.W;
        }

        public override string ToString()
        {
            return string.Format("Turn {0}, Score {1}, RawScore {2}, ExtraScore {3}, MaxChain {4}, MaxSameErase {5}, FireCount {6}",
                Turn, Score, RawScore, ExtraScore, MaxChain, MaxSameErase, FireCount);
        }

        public string GetStringInformation()
        {
            return string.Format("Turn {0}\nScore {1}\nRawScore {2}\nExtraScore {3}\nMaxChain {4}\nMaxSameErase {5}\nFireCount {6}\n",
                Turn, Score, RawScore, ExtraScore, MaxChain, MaxSameErase, FireCount);
        }

        public static GameModel ReadGameModel(TextReader reader)
        {
            GameSetting gs = GameSetting.ReadHeader(reader);
            NextPacks np = NextPacks.ReadNextPacks(reader, gs.N, gs.T, gs.S);
            MainField mf = gs.CreateMainField();
            return new GameModel(gs, np, mf);
        }
    }
}
