using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TugOfWar.GameObject.Derps
{
    public class DerpStats
    {
        ////////////////
        // Stats
        // hp - the health points of this derp
        public int hp;
        // spd - the speed this derp moves at
        public int spd;
        // atk - attack power
        public int atk;
        // aspd - attack speed
        public int aspd;
        // rng - attack range
        public int rng;

        // Physical Stats
        public int width, height, radius;

        // This is used to sort by speed
        public SortKey key;

        public DerpStats(int hp, int spd, int atk, int aspd, int rng, int width, int height)
        {
            this.hp = hp;
            this.spd = spd;
            this.atk = atk;
            this.aspd = aspd;
            this.rng = rng;

            this.width = width;
            this.height = height;
            radius = Math.Max(width/2, height/2);

            key = new SortKey(spd);
        }
    }

    // This is a comparable but not equal class
    // This is used for C#'s SortedList datastructure, so that we can have multiple same keys
    public class SortKey : IComparable<SortKey>
    {
        // guid to identify unique keys
        static int guid_counter = 0;

        public double keyval;
        private int guid;

        public SortKey(int val)
        {
            keyval = val;
            guid = guid_counter++;
        }

        public SortKey(double val)
        {
            keyval = val;
            guid = guid_counter++;
        }

        public int CompareTo(SortKey o)
        {
            int ret = keyval.CompareTo(o.keyval);
            if (ret != 0)
                return -ret; // sort in inverse order (largest speed first)
            else
                return guid.CompareTo(o.guid);
        }
    }
}
