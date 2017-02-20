using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TugOfWar.GameObject
{
    public class GameState
    {
        // Singleton
        public static GameState state;

        public int Energy;
        public int EnergyGain;
        public int EnergyLoss;

        static GameState()
        {
            state = new GameState();
        }

        private GameState()
        {
            Energy = 0;
            EnergyGain = 0;
            EnergyLoss = 0;
        }

        public void ResetAll()
        {
            Energy = 0;
            EnergyGain = 0;
            EnergyLoss = 0;
        }
    }
}
