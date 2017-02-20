using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TugOfWar.GameObject.Derps;

namespace TugOfWar
{
    // We need randomized values for some activities
    // It is particularly important that we synchronize these values in multiplayer
    // All random values will be generated in this class to hopefully keep the feat managable
    public class MyRandom
    {
        // Multiplayer "Random" numbers must be synchronized
        // here is where it will be done
        static int home_seed;
        static int away_seed;
        static Random home_random;
        static Random away_random;

        public static void generateSeeds()
        {
            home_random = new Random();
            away_random = new Random();
        }

        public static void setSeeds(int h, int a)
        {
            home_seed = h;
            away_seed = a;

            home_random = new Random(h);
            away_random = new Random(a);
        }

        public static int Next(TEAM t)
        {
            if (t == TEAM.HOME)
                return home_random.Next();
            else
                return away_random.Next();
        }

        public static int Next(TEAM t, int h)
        {
            if (t == TEAM.HOME)
                return home_random.Next(h);
            else
                return away_random.Next(h);
        }

        // note: l and h are inclusive and exclusive respectively
        public static int Next(TEAM t, int l, int h)
        {
            if (t == TEAM.HOME)
                return home_random.Next(l, h);
            else
                return away_random.Next(l, h);
        }

        public static double NextDouble(TEAM t)
        {
            if (t == TEAM.HOME)
                return home_random.NextDouble();
            else
                return away_random.NextDouble();
        }
    }
}
