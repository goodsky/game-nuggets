using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

// The DerpManager will have a collection of Derps that it draws out each step
// This will manage finding derps at positions and whatnot

namespace TugOfWar.GameObject.Derps
{

    public class DerpManager
    {
        // A complete Hash of all alive derps.
        public Dictionary<long, Derp> derpsHash;

        // List of derps. Will be kept sorted by Derp speed. Will allow us to update derps based on their speed (faster derps first).
        public SortedList<SortKey, Derp> derpsSpeed;

        // split the list of derps into teams here... this miiiight speed up collision detection a teensy bit
        public List<Derp> homeDerps;
        public List<Derp> awayDerps;

        // List of derp attacks that are happening. They will be kept sorted by their y position.
        public SortedList<SortKey, DerpAttack> derpAttacks;

        // Array of Lists of Nodes that are needed for each starting point in the map.
        // (the index is the y block that the derp spawns on, and the AWAY team traverses backwards)
        List<Node>[] pathTraversals;

        public DerpManager()
        {
            Reset();
        }

        public void Reset()
        {
            // default these to null just to be safe
            pathTraversals = new List<Node>[10];
            for (int i = 0; i < pathTraversals.Length; ++i)
                pathTraversals[i] = null;

            derpsHash = new Dictionary<long, Derp>();
            derpsSpeed = new SortedList<SortKey, Derp>();
            homeDerps = new List<Derp>();
            awayDerps = new List<Derp>();

            derpAttacks = new SortedList<SortKey, DerpAttack>();
        }

        public void InitializePaths(List<Node>[] paths)
        {
            pathTraversals = paths;
        }

        public void SpawnDerp(int xPos, int yPos, TEAM team, DerpStats stats)
        {
            int yStart = yPos / Field.BLOCK_HEIGHT;

            if (pathTraversals[yStart] == null)
            {
                MessageBox.Show("Failed to Spawn Derp at " + xPos + " " + yPos + ". No Hub.");
                return;
            }

            Derp babyDerp = new Derp(xPos, yPos, team, stats, this, pathTraversals[yStart]);

            // Add new derp to complete lists of derps
            derpsHash.Add(babyDerp.myID, babyDerp);
            derpsSpeed.Add(babyDerp.stats.key, babyDerp);

            // Also add the derp to its team-specific list
            if (team == TEAM.HOME)
                homeDerps.Add(babyDerp);
            else
                awayDerps.Add(babyDerp);
        }

        private void RemoveDerp(Derp d)
        {
            derpsHash.Remove(d.myID);
            derpsSpeed.Remove(d.stats.key);

            if (d.team == TEAM.HOME)
                homeDerps.Remove(d);
            else
                awayDerps.Remove(d);
        }

        public void Update(GameTime gameTime)
        {
            // Update all Derps
            // Use the kill stack to clean up dead derps
            Stack<Derp> killingStack = new Stack<Derp>();
            foreach (KeyValuePair<SortKey, Derp> keyVal in derpsSpeed)
            {
                Derp d = keyVal.Value;

                // Update each
                if (d.alive)
                {
                    d.Update();
                }
                else
                {
                    killingStack.Push(d);
                }
            }

            // Cleaning up
            while (killingStack.Count > 0)
            {
                RemoveDerp(killingStack.Pop());
            }
        }

        // COLLISION CODE
        // Find circle-cast position of the collision
        // returns the id of the derp if there is a collision, also set the t value to the max parametric value it can be without a collision
        // if checkBothTeams is set to true, then also collide with the enemy team
        double distThreshholdY = Field.BLOCK_WIDTH;
        public int CheckDerpCollision(Derp casting, myVector v, out double t, bool checkBothTeams)
        {
            // t is initialized optimistically to the full distance
            t = 1.0;

            // the id of the derp is -1 if there is no collision
            int ret = -1;

            // For collision we are checking only against our teammates (because we will always attack (and stop moving) before we collide with enemies) <- at least that's the plan
            List<Derp> checkingList;
            if (casting.team == TEAM.HOME)
                checkingList = homeDerps;
            else
                checkingList = awayDerps;
            
            // I'm making the optimistic assumption that these lists will be sorted because they are sorted every Draw Step
            int myID = checkingList.IndexOf(casting);

            // check derps above me
            for (int i = myID-1; i >= 0; --i)
            {
                // only check derps near me on the y axis
                if (casting.y - checkingList[i].y > distThreshholdY)
                    break;
                
                double check_t = Geometry.DerpCircleCast(casting, checkingList[i], v);
                if (check_t < t)
                {
                    t = check_t;
                    ret = i;
                }
            }

            // check derps below me
            for (int i = myID + 1; i < checkingList.Count; ++i)
            {
                // only check derps near me on the y axis
                if (casting.y - checkingList[i].y > distThreshholdY)
                    break;

                double check_t = Geometry.DerpCircleCast(casting, checkingList[i], v);
                if (check_t < t)
                {
                    t = check_t;
                    ret = i;
                }
            }

            // If we need to check the enemy team, then we can't do it quite as fast, but just check against them all
            if (checkBothTeams)
            {
                checkingList = (casting.team == TEAM.HOME ? awayDerps : homeDerps);
                foreach (Derp o in checkingList)
                {
                    double check_t = Geometry.DerpCircleCast(casting, o, v);
                    if (check_t < t)
                    {
                        t = check_t;
                        ret = -1;
                    }
                }
            }

            // the id of the colliding derp
            return ret;
        }

        // Set a derp to move out of the way of another derp. Also returns the slow derp. (even if it actually isn't slower?)
        // The fastDerp has run into the slow derp, it must tell it to move out of the way. Give the other derp the unit vector to move along
        public Derp SetToMoveOutOfWay(int slowDerpID, Derp fastDerp)
        {
            List<Derp> checkList = (fastDerp.team == TEAM.HOME ? homeDerps : awayDerps);
            Derp slowDerp = checkList[slowDerpID];

            // only actually do this if the slow derp is slower or equal
            if (slowDerp.stats.spd > fastDerp.stats.spd)
                return slowDerp;

            // The Move Vector should be directly away from the fast derp
            myVector awayVector = new myVector(slowDerp.x - fastDerp.x, slowDerp.y - fastDerp.y);
            awayVector.toUnit();

            // Tell 'em to move
            slowDerp.moveOutOfWay = true;
            slowDerp.moveOutOfWayVector = awayVector;

            return slowDerp;
        }

        // Search for enemy derps within our radar range
        // We return a pointer to the poor soul or null if none are in range
        public Derp SearchForEnemy(Derp d, double range)
        {
            // return null if there is no collision
            Derp ret = null;

            // For collision we are checking only against our enemies
            List<Derp> checkingList;
            if (d.team == TEAM.HOME)
                checkingList = awayDerps;
            else
                checkingList = homeDerps;

            double mindist = range * range;

            // check against all other enemy derps
            double throwaway1;
            myLineSegment throwaway2;
            foreach (Derp o in checkingList)
            {
                myVector toOther = new myVector(o.x - d.x, o.y - d.y);

                if (toOther.mag2() < mindist && !Field.field.CheckFieldCollision(d, toOther, out throwaway1, out throwaway2))
                {
                    mindist = toOther.mag2();
                    ret = o;
                }
            }

            // the unlucky nearest visible enemy
            return ret;
        }

        // Very similar to the above, but takes in a point and searches all derps
        // We return a pointer to the poor soul or null if none are in range
        public Derp SearchForDerp(int x, int y, double range)
        {
            // return null if there is no collision
            Derp ret = null;
            double mindist = range * range;

            // check against all other enemy derps
            foreach (Derp o in homeDerps)
            {
                myVector toOther = new myVector(o.x - x, o.y - y);

                if (toOther.mag2() < mindist)
                {
                    mindist = toOther.mag2();
                    ret = o;
                }
            }

            foreach (Derp o in awayDerps)
            {
                myVector toOther = new myVector(o.x - x, o.y - y);

                if (toOther.mag2() < mindist)
                {
                    mindist = toOther.mag2();
                    ret = o;
                }
            }

            // the unlucky nearest visible enemy
            return ret;
        }

        // Add an attack Animation here
        // TODO: maybe put the thing that actually applies the damage here, just to consolodate logical code
        public void AddAttack(Derp from, Derp to)
        {
            // This must be located two radius distance from the derp, in the direction of the delivering attacker
            myVector u = new myVector(from.x - to.x, from.y - to.y);
            u.toUnit();

            // opposite direction for nearest cardinal dir calculation
            myVector u2 = new myVector(-u.x, -u.y);

            DerpAttack attack = new DerpAttack(to.x + (u.x * to.stats.radius * 2), to.y + (u.y * to.stats.radius * 2), Geometry.GetNearestCardinalDir(u2));
            derpAttacks.Add(attack.key, attack);
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle camera)
        {
            // Draw all Derps
            // First Step, sort both Team Lists of Derps by Y position, then continually draw the lower Y value first
            DerpHeightSorter dHS = new DerpHeightSorter();
            homeDerps.Sort(dHS);
            awayDerps.Sort(dHS);

            // Merge the two lists and draw each derp in order of Y Position
            int i = 0;
            int j = 0;
            while (i < homeDerps.Count || j < awayDerps.Count)
            {
                Derp d;

                // Merge
                if (i == homeDerps.Count)
                {
                    d = awayDerps[j++];
                }
                else if (j == awayDerps.Count)
                {
                    d = homeDerps[i++];
                }
                else
                {
                    if (homeDerps[i].y > awayDerps[j].y)
                    {
                        d = awayDerps[j++];
                    }
                    else
                    {
                        d = homeDerps[i++];
                    }
                }

                // Draw
                if ((d.x + d.stats.width) > camera.X && (d.x - d.stats.width) < camera.X + camera.Width)
                {
                    d.Draw(spriteBatch, camera);
                }
            }

            // Draw All of the attacks now as well
            Stack<DerpAttack> killingStack = new Stack<DerpAttack>();

            foreach (KeyValuePair<SortKey, DerpAttack> kp in derpAttacks)
            {
                kp.Value.Draw(spriteBatch, camera);

                if (!kp.Value.alive)
                    killingStack.Push(kp.Value);
            }

            // Cleaning up
            while (killingStack.Count > 0)
            {
                DerpAttack deadAttack = killingStack.Pop();
                derpAttacks.Remove(deadAttack.key);
            }
        }
    }

    // Y Position Sorter for Derps
    class DerpHeightSorter : IComparer<Derp>
    {
        public int Compare(Derp a, Derp b)
        {
            return a.y.CompareTo(b.y);
        }
    }
}
