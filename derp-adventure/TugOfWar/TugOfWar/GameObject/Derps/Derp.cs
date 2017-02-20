using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

// The Derp Class will control a single Derp instance including the derp's stats, position, etc
// Also included in this file are the Node and Cost classes used for the waypoint navigation of maps and A*

namespace TugOfWar.GameObject.Derps
{
    public enum TEAM { HOME, AWAY };

    public class Derp
    {
        // The stats IDs for all derps throughout the game
        public static long nextDerpID = 0;

        // Information about me
        public double x, y;
        private myVector vel = new myVector(0, 0);
        public TEAM team;

        public long myID;

        // The pre-packaged path the Derp will followed (calcualted by the Field)
        List<Node> path;
        int pathID;

        // The stats will be evolved through the evolutionary algorithm
        public DerpStats stats;

        // The graphic will have to be much more complicated in the future
        // It will change depending on the stats of the derp
        private static Dictionary<String, Texture2D> baseTextures = null;
        private static Dictionary<String, SpriteInfo> baseOffsets = null;
        private static Dictionary<String, Texture2D> accessoryTextures = null;
        private static Dictionary<String, SpriteInfo> accessoryOffsets = null;
        public static void InitializeTextures(Dictionary<String, Texture2D> baseTextures, Dictionary<String, SpriteInfo> baseOffsets, Dictionary<String, Texture2D> accessoryTextures, Dictionary<String, SpriteInfo> accessoryOffsets)
        {
            Derp.baseTextures = baseTextures;
            Derp.baseOffsets = baseOffsets;
            Derp.accessoryTextures = accessoryTextures;
            Derp.accessoryOffsets = accessoryOffsets;
        }

        // The base texture, depending on your epoch and size
        private Texture2D baseTexture;
        private SpriteInfo baseOffset;
        private int lastDir = 0;
        private int lastStep = 0;

        // The accessories we will draw over the base
        private List<Texture2D> accessories;
        private List<SpriteInfo> accessoriesOffest;

        // I need my parent manager to ask about collisions
        DerpManager manager;

        // If I am running into something ahead of me
        public bool isColliding = false;

        // part of the jamming prevention system, we must move out of the way of faster derps
        public bool moveOutOfWay = false;
        public myVector moveOutOfWayVector = null;

        // for attacking
        public bool enemyDetected = false;
        public bool isAttacking = false;
        private DateTime lastAttack = DateTime.Now;

        // Help I'm Alive
        public bool alive = true;

        // debug
        public bool isStuck = false;
        public double debugDist = 0.0;

        // CONSTRUCTOR
        public Derp(double x, double y, TEAM team, DerpStats stats, DerpManager manager, List<Node> path)
        {
            this.x = x;
            this.y = y;

            this.team = team;
            this.stats = stats;

            this.manager = manager;
            this.path = path;

            if (team == TEAM.HOME)
                pathID = 1;
            else
                pathID = path.Count - 2;

            myID = nextDerpID++;

            // Set up my image!
            baseTexture = baseTextures["e0s1"];
            baseOffset = baseOffsets["e0s1"];
        }

        // UPDATE
        private double SPEED_CONST = 0.05;
        private double GENERIOUS_THRESHHOLD = 50.0;
        public void Update()
        {
            // CHECK FOR ATTACKS
            // use the derp radar to search for visible enemies in range
            Derp enemy = manager.SearchForEnemy(this, 100.0);
            enemyDetected = (enemy != null);

            // see if we can attack this enemy
            myVector toEnemy = null;
            if (enemyDetected)
            {
                toEnemy = new myVector(enemy.x - x, enemy.y - y);
                isAttacking = (toEnemy.mag() < stats.radius + enemy.stats.radius + stats.rng);
                debugDist = toEnemy.mag();

                if (isAttacking && (DateTime.Now.Subtract(lastAttack).TotalMilliseconds > (2000 - 15 * stats.aspd)))
                {
                    lastAttack = DateTime.Now;

                    enemy.takeHit(stats.atk);
                    manager.AddAttack(this, enemy);
                }
            }
            else
            {
                isAttacking = false;
            }

            // FOLLOW THE TRAIL
            // Find the vector between the derp and its current node destination
            double dx = path[pathID].x - x;
            double dy = path[pathID].y - y;
            double mag = Math.Sqrt(dx * dx + dy * dy);

            double t = 1.0;
            myLineSegment throwaway;
            int nextPathID = pathID + (team == TEAM.HOME ? 1 : -1);

            // Check if we can move to the next path node
            if (mag < (stats.spd * SPEED_CONST))
            {
                pathID = nextPathID;

                // Recalculate for the next node
                dx = path[pathID].x - x;
                dy = path[pathID].y - y;
                mag = Math.Sqrt(dx * dx + dy * dy);
            }
            // Sometimes we can abort the current node for the next-next node
            else if (nextPathID >= 0 && nextPathID < path.Count)
            {
                // if the next node is behind us (meaning we passed it accidentily) then just keep going
                // we use a manual dot product for this
                double ndx = path[nextPathID].x - x;
                double ndy = path[nextPathID].y - y;
                if (dx * ndx + dy * ndy < 0.0 && !Field.field.CheckFieldCollision(this, new myVector(path[nextPathID].x - x, path[nextPathID].y - y), out t, out throwaway))
                {
                    pathID = nextPathID;

                    // Recalculate for the next node
                    dx = path[pathID].x - x;
                    dy = path[pathID].y - y;
                    mag = Math.Sqrt(dx * dx + dy * dy);
                }
                // we can give a more liberal option to move forward to the next node
                // if we are colliding or seeking an enemy derp, and we can see the next node
                else if (isColliding && mag < GENERIOUS_THRESHHOLD && 
                    !Field.field.CheckFieldCollision(this, new myVector(path[nextPathID].x - x, path[nextPathID].y - y), out t, out throwaway))
                {
                    pathID = nextPathID;

                    // Recalculate for the next node
                    dx = path[pathID].x - x;
                    dy = path[pathID].y - y;
                    mag = Math.Sqrt(dx * dx + dy * dy);
                }
            }

            // DETERMINE MOVE
            if (!isAttacking)
            {
                // Calculate velocity and try to make the step
                double velocity = stats.spd * SPEED_CONST;
                myVector v;
                if (enemyDetected && toEnemy != null)
                {
                    toEnemy.toUnit();
                    v = new myVector(toEnemy.x * velocity, toEnemy.y * velocity);
                }
                else if (moveOutOfWay)
                {
                    // Move out of the way!
                    v = new myVector(moveOutOfWayVector.x * velocity, moveOutOfWayVector.y * velocity);
                }
                else
                {
                    // Move Towards next Node
                    v = new myVector((dx / mag) * velocity, (dy / mag) * velocity);
                }

                // DEBUG: error catching
                double oldx = x;
                double oldy = y;

                if (isStuck)
                    isStuck = false;

                // Try to Step
                attemptStep(v, false);

                // DEBUG: error catching
                if (Math.Abs(x - oldx) < 1e-3)
                {
                    isStuck = true;
                }

                // Update our current instantaneous velocity
                vel.x = x - oldx; vel.y = y - oldy;

                // WE SHOULD NEVER MOVE THIS FAST. FIND MATH ERRORS.
                if (Math.Abs(vel.x) > 50 || Math.Abs(vel.y) > 50)
                {
                    x = oldx;
                    y = oldy;
                }
            }

            // Reset our MoveOUtOfWay variable, it will be set again next round if we need to keep moving out of the way
            moveOutOfWay = false;

            // This will be the game end condition eventually
            if ((team == TEAM.HOME && x > (Field.field.Width - 1) * Field.BLOCK_WIDTH) || (team == TEAM.AWAY && x < Field.BLOCK_WIDTH) || stats.hp <= 0)
                Kill();
        }

        // This is the function that tries to make a step, it can recurse once.
        public void attemptStep(myVector v, bool recurse)
        {
            // Optimistic no collision
            double t = 1.0;
            myVector newV = null;

            // Check for team derp collisions
            double derpT;
            myVector derpV = null;

            int colID = manager.CheckDerpCollision(this, v, out derpT, enemyDetected); // note: this returns -1 if either there is no collision, or if we are colliding with the other team
            isColliding = (colID != -1); 

            // We have collided with another derp
            // Tell the other to move out of the way, and have us try to move perpendicular to them
            if (isColliding && !recurse)
            {
                Derp slowDerp = manager.SetToMoveOutOfWay(colID, this);

                if (slowDerp != null)
                {
                    // Get the Unit Vector towards the slow Derp
                    myVector u = new myVector(slowDerp.x - x, slowDerp.y - y);
                    u.toUnit();

                    // Make sure that the slow derp is actually in front of where you want to go
                    if (u.dot(v) > 0.0)
                    {
                        // Get a perpendicular unit vector to follow
                        myVector uPerp = new myVector(-u.y, u.x);

                        // Make sure it is the correct way
                        double dir = uPerp.dot(v);

                        // Wrong way
                        if (dir < -1e-3)
                        {
                            uPerp.x *= -1;
                            uPerp.y *= -1;
                        }
                        // Special Zero case, pick a random direction
                        // this should... almost never happen
                        // I'm concerned this could mess up random synch in multiplayer. Just pick a constant direction.
                        else if (dir < 1e-3)
                        {
                            /*
                            if (MyRandom.Next(team, 2) % 2 == 0)
                            {
                                uPerp.x *= -1;
                                uPerp.y *= -1;
                            }
                            */
                        }

                        // Try moving along the new path
                        double remainingT = (1.0 - derpT);
                        derpV = new myVector(uPerp.x * remainingT * v.mag(), uPerp.y * remainingT * v.mag());
                    }
                }
            }

            // Check for field collisions
            double fieldT;
            myVector fieldV = null;
            myLineSegment collisionVect;

            if (Field.field.CheckFieldCollision(this, v, out fieldT, out collisionVect) && !recurse)
            {
                // try to move parallel to the wall with the extra t
                collisionVect.v.toUnit();

                // make sure this unit vector is in the right direction
                double dir = collisionVect.v.dot(v);

                // Wrong way
                if (dir < -1e-3)
                {
                    collisionVect.v.x *= -1;
                    collisionVect.v.y *= -1;
                }

                // ignore zero case
                if (Math.Abs(dir) > 1e-3)
                {
                    // Try moving along the new path
                    double remainingT = (1.0 - fieldT);
                    fieldV = new myVector(collisionVect.v.x * remainingT * v.mag(), collisionVect.v.y * remainingT * v.mag());
                }
            }

            // See if we have a collision
            // If either of the recalculated V values are not null, we hit something along the way
            if (derpT < 1.0 - 1e-6 || fieldT < 1.0 - 1e-6)
            {
                if (fieldT < derpT)
                {
                    t = fieldT;
                    newV = fieldV;
                }
                else
                {
                    t = derpT;
                    newV = derpV;
                }
            }

            // Only step as far as we can
            x += v.x * t;
            y += v.y * t;

            if (newV != null)
            {
                attemptStep(newV, true);
            }
        }

        // Take a Hit
        public void takeHit(int damage)
        {
            stats.hp -= damage;
        }

        // KILL
        public void Kill()
        {
            alive = false;
        }

        // DRAW
        // Draw the Base Image First and then draw the Accessories over top
        public void Draw(SpriteBatch spriteBatch, Rectangle camera)
        {
            // Which direction frame to draw
            int frameID = 0;

            if (vel.mag2() > 1e-6)
            {
                frameID = Geometry.GetNearestCardinalDir(vel);
            }
            else
            {
                frameID = lastDir;
            }

            lastDir = frameID;

            // Which step in the animation are we on
            int stepID = lastStep;

            spriteBatch.Draw(baseTexture, new Rectangle((int)(x - camera.X - baseOffset.offsetX), (int)(y - camera.Y - baseOffset.offsetY), baseOffset.width, baseOffset.height),
                                          new Rectangle(stepID * baseOffset.width, frameID * baseOffset.height, baseOffset.width, baseOffset.height), Color.White);

            // DEBUG:
            //if (isStuck)
            //{
            //    spriteBatch.DrawString(GUI.smallFont, "X", new Vector2((float)(x - camera.X - 6), (float)(y - 10)), Color.Red);
            //}
        }
    }


    // ***************************************************************************************

    // This class helps us know where to draw elements
    public class SpriteInfo
    {
        public int width, height;
        public int offsetX, offsetY;
        public int frames;

        public SpriteInfo(int width, int height, int offsetX, int offsetY, int frames)
        {
            this.width = width;
            this.height = height;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.frames = frames;
        }
    }

    // The node simply represents the point of interest for the derp to move towards
    // It is also used in my A* during Map Initialization
    public class Node
    {
        public int x, y;

        // used for A*
        public Node from;
        public Cost cost;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;

            from = null;
            cost = null;
        }

        public Node(int x, int y, Node from, double cost, double heuristic)
        {
            this.x = x;
            this.y = y;

            this.from = from;
            this.cost = new Cost(cost, heuristic);
        }
    }

    // This might be confusing
    // I'm making a custom Cost class that is basically just a double, but never returns equals in comparison checks
    public class Cost : IComparable<Cost>
    {
        private double cost;
        private double heuristic;

        public Cost(double cost, double heuristic)
        {
            this.cost = cost;
            this.heuristic = heuristic;
        }

        public double GetCost()
        {
            return cost;
        }

        public double GetTotal()
        {
            return cost + heuristic;
        }

        public int CompareTo(Cost o)
        {
            int ret = GetTotal().CompareTo(o.GetTotal());

            if (ret == 0)
                return 1;
            else
                return ret;
        }
    }
}
