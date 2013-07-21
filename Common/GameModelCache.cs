using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    class GameModelCache
    {
        public int Level { get; private set; }
        GameModel[] cache;
        Rect[] change;

        public GameModelCache(int maxLevel, GameModel gm)
        {
            Level = 0;
            cache = new GameModel[maxLevel + 1];
            change = new Rect[maxLevel + 1];
            for (int i = 0; i <= maxLevel; i++)
            {
                cache[i] = gm.Clone();
            }
            NextLevel();
        }

        public TurnInfo ForwardStep(int cx, int cr)
        {
            TurnInfo info = cache[Level].ForwardStep(cx, cr);
            change[Level] = info.ChangeRect;
            return info;
        }

        public void BackwardStep()
        {
            cache[Level - 1].CopyTo(cache[Level], change[Level]);
        }

        public void NextLevel()
        {
            Level++;
            cache[Level - 1].CopyTo(cache[Level]);
        }

        public void PreviousLevel()
        {
            Level--;
        }

        public GameModel GetCurrentGameModel()
        {
            return cache[Level];
        }
    }
}
