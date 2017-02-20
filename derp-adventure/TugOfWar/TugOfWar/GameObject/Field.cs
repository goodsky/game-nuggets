using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

using TugOfWar.GameObject.Derps;
using TugOfWar.GameObject.DNA;

namespace TugOfWar.GameObject
{
    class Field : IGameObject
    {
        public static int BLOCK_WIDTH = 50;
        public static int BLOCK_HEIGHT = 48;

        public static Field field = null;

        // The Field HEIGHT and WIDTH in number of blocks
        public int Width { get { return width; } }
        public int Height { get { return height; } }
        private int width = 32;
        private int height = 10;

        // GameGrid represents the Field Elements
        private char[,] gameGrid;

        // blockTexture stores the image for each block
        private Dictionary<char, Texture2D> blockTexture;
        // blockSrcOffset is the src rectangle to get the image from
        private Dictionary<char, Rectangle> blockSrcOffset;
        // blockPositionOffset stores the integer offset for raised tiles
        private Dictionary<char, int> blockPositionOffset;

        // The Spawners that the DNA is built from on the left and right
        private Spawner[] leftSpawners;
        private Spawner[] rightSpawners;
        
        // selectorTexture stores images for selection box
        private Texture2D[] selectorTexture;
    
        // camera parameters for rendering
        private int camX = 0;
        private int camY = 0;
        private double xVel = 0.0;

        // the location of the current selected box
        public Point selectedBox;

        // derp manager to control ALL THE DERPS
        public static DerpManager derpManager;

        // ***********************
        // Constructors
        public Field(int w, int h)
        {
            width = w;
            height = h;

            gameGrid = new char[height, width+1];
            leftSpawners = new Spawner[height];
            rightSpawners = new Spawner[height];

            blockTexture = null;

            selectorTexture = null;

            selectedBox = new Point(-1, -1);

            derpManager = new DerpManager();

            // set the singleton (maybe a bad idea to do this, but it helps with the derps)
            if (field == null)
                field = this;
        }

        public int LoadLevel(string filename)
        {
            // Reset Field Values
            camX = 0;
            xVel = 0;

            derpManager.Reset();

            // Load the file
            using (StreamReader mapData = new StreamReader(filename))
            {
                //////////////////////////////////////////////////
                // Read the texture information portion of the map
                String[] header = mapData.ReadLine().Split(new char[] { ' ' });

                // Error #1 improper header format
                if (header.Length != 2) return 1;

                int X = Int32.Parse(header[0]);
                int Y = Int32.Parse(header[1]);

                width = X;
                height = Y;
                gameGrid = new char[height, width+1];
                leftSpawners = new Spawner[height];
                rightSpawners = new Spawner[height];

                for (int y = 0; y < Y; ++y)
                {   
                    String line = mapData.ReadLine();
                    for (int x = 0; x < X; ++x)
                    {
                        gameGrid[y, x] = line[x];
                    }
                }

                /////////////////////////////////////////////////////////////
                // Run A* and create the NodeLists for all starting positions
                // Loop over all posible Y position starts
                List<Node>[] pathTraversals = new List<Node>[Y];
                for (int y = 0; y < Y; ++y)
                {
                    pathTraversals[y] = null;

                    // If one side is a hub but not the other, then it's not even (which is a somewhat arbitrary requirement I am enforcing)
                    if ((gameGrid[y, 0] == 'A' || gameGrid[y, 0] == 'B' || gameGrid[y, 0] == 'C') != (gameGrid[y, X-1] == 'A' || gameGrid[y, X-1] == 'B' || gameGrid[y, X-1] == 'C'))
                        return 2;

                    if (gameGrid[y, 0] == 'A' || gameGrid[y, 0] == 'B' || gameGrid[y, 0] == 'C')
                    {
                        // Find the length of the 'building zone' on each side (building zone is the '-' character)
                        int buildingZoneLength = 0;

                        // NOTICE: max length of building length is 32 right now... this could cause weird bugs in the future
                        for (int i = 1; i < 32; ++i)
                        {
                            // Again, I'm enforcing strict symmetry on this portion of the map
                            if ((gameGrid[y, i] == '-') != (gameGrid[y, X - 1 - i] == '-'))
                                return 2;

                            if (gameGrid[y, i] == '-')
                                ++buildingZoneLength;
                            else
                                break;
                        }

                        // Start our spawner here
                        leftSpawners[y] = new Spawner(TEAM.HOME, 0, y, buildingZoneLength, (gameGrid[y, 0] - 'A' + 1));
                        rightSpawners[y] = new Spawner(TEAM.AWAY, X - 1, y, buildingZoneLength, (gameGrid[y, 0] - 'A' + 1));
                        
                        // Set all spawners to a default 'A' from now on
                        gameGrid[y, 0] = 'A';

                        // A*
                        Node start = new Node(buildingZoneLength + 1, y, null, 0.0, (X - 1 - buildingZoneLength - 1 - (buildingZoneLength + 1)));
                        Node end = new Node(X - 1 - buildingZoneLength - 1, y);
                        Node finalNode = null; // will contain end node at the end (or null meaning no path was found)

                        SortedList<Cost, Node> pQ = new SortedList<Cost, Node>();
                        pQ.Add(start.cost, start);

                        // separate square and diagonal movement, first 4 are square, second 4 are diagonals
                        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
                        int[] dy = { 1, 0, -1, 0, 1, -1, -1, 1 };

                        // visited array
                        bool[,] vis = new bool[height, width + 1];
                        for (int i = 0; i < height; ++i)
                            for (int j = 0; j < width + 1; ++j)
                                vis[i, j] = false;

                        vis[start.y, start.x] = true;

                        // array of nodes (so garbage collection doesn't mess with my reconstruction phase)
                        Node[,] nodes = new Node[height, width + 1];

                        // bfs w/ A* priority queue
                        while (pQ.Count > 0)
                        {
                            Node cur = pQ.Values[0];
                            pQ.RemoveAt(0);

                            // end case
                            if (cur.x == end.x && cur.y == end.y)
                            {
                                finalNode = cur;
                                break;
                            }

                            // diagonal movement
                            for (int i = 0; i < 8; ++i)
                            {
                                int nx = cur.x + dx[i];
                                int ny = cur.y + dy[i];

                                // ensure we are in-bounds and on a legal block ('+')
                                if (nx < 0 || nx >= X || ny < 0 || ny >= Y || vis[ny, nx] || gameGrid[ny, nx] != '+')
                                    continue;

                                // make sure the two diagonal passing blocks are legal, or a half-block ('+', '1', '2', '3', '4')
                                // note: this check is only made when i > 3, meaning a diagonal move
                                char c1 = gameGrid[ny, cur.x];
                                char c2 = gameGrid[cur.y, nx];
                                if (i > 3 && !(c1 == '+' || c1 == '1' || c1 == '2' || c1 == '3' || c1 == '4') || !(c2 == '+' || c2 == '1' || c2 == '2' || c2 == '3' || c2 == '4'))
                                    continue;

                                // square moves are 1 unit, diagonal are sqrt(2)
                                double addedCost = 1.0;
                                if (i > 3)
                                    addedCost = Math.Sqrt(2.0);

                                // Heuristic Value
                                double ex = end.x - nx;
                                double ey = end.y - ny; 
                                double heuristic = Math.Sqrt(ex*ex + ey*ey);

                                nodes[ny, nx] = new Node(nx, ny, cur, cur.cost.GetCost() + addedCost, heuristic);
                                vis[ny, nx] = true;
                                pQ.Add(nodes[ny, nx].cost, nodes[ny, nx]);
                            }
                        }

                        // Make sure we found a path
                        if (finalNode == null)
                            return 2;

                        // Reconstruct the node path and save it
                        Stack<Node> reconstruct = new Stack<Node>();
                        reconstruct.Push(new Node(X - 1, y)); // the final spawn hub node
                        // the idea is to only include nodes that change direction, the minimal hull of the path that keeps the shape
                        int vx = 0;
                        int vy = 0;
                        for (;;)
                        {
                            Node nextNode = finalNode.from;
                            if (nextNode == null)
                            {
                                reconstruct.Push(finalNode);
                                break;
                            }

                            int newVX = nextNode.x - finalNode.x;
                            int newVY = nextNode.y - finalNode.y;

                            if (vx != newVX || vy != newVY)
                            {
                                reconstruct.Push(finalNode);
                            }

                            finalNode = nextNode;
                            vx = newVX;
                            vy = newVY;
                        }
                        reconstruct.Push(new Node(0, y)); // the first spawn hub node

                        // Now Pop the reconstructed path in reverse order and put it into the final array
                        // ALSO finally convert them to final world units
                        pathTraversals[y] = new List<Node>();
                        while (reconstruct.Count > 0)
                        {
                            Node final = reconstruct.Pop();

                            final.x = (final.x * Field.BLOCK_WIDTH) + (Field.BLOCK_WIDTH / 2);
                            final.y = (final.y * Field.BLOCK_HEIGHT) + (Field.BLOCK_HEIGHT / 2);

                            pathTraversals[y].Add(final);
                        }
                    }
                }

                // After all of that A* work, set up the Derp Manager with the traversal nodes
                derpManager.InitializePaths(pathTraversals);
            }

            // success
            return 0;
        }

        public void InitializeBlocks(Dictionary<char, Texture2D> blocks, Dictionary<char, Rectangle> srcOffset, Dictionary<char, int> posOffset, Texture2D[] selector)
        {
            // Load each block image
            blockTexture = blocks;
            blockSrcOffset = srcOffset;
            blockPositionOffset = posOffset;

            selectorTexture = selector;
        }

        // Check for a Derp's Circle-Cast across the field
        // This will work like a BFS, starting from the initial position
        // then check all 4 corners around the position to build on the bfs
        public bool CheckFieldCollision(Derp d, myVector v, out double t, out myLineSegment col)
        {
            // initial optimistic setup that there will not be a collision
            bool ret = false;
            t = 1.0;
            col = null;

            // calculate starting point
            int startX = (int)(d.x / BLOCK_WIDTH);
            int startY = (int)(d.y / BLOCK_HEIGHT);

            // Unit Vector in desired direction
            myVector vUnit = new myVector(v.x, v.y);
            vUnit.toUnit();

            // set up the bfs
            Queue<SimpleNode> q = new Queue<SimpleNode>();
            bool[,] vis = new bool[height+1, width+1];

            q.Enqueue(new SimpleNode(startX, startY, 0));
            vis[startY, startX] = true;

            // Create the 4 line segments so we don't have to do quiiite as much object creation in this loop
            myLineSegment[] segs = new myLineSegment[4];
            for (int i = 0; i < 4; ++i)
                segs[i] = new myLineSegment(null, null);

            // BFS
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { -1, 0, 1, 0 };
            int cur_step = 0;
            while (q.Count > 0)
            {
                SimpleNode cur = q.Dequeue();

                // end early if we had a hit already in a previous step
                if (ret && cur_step != cur.step)
                    break;

                // checking 4 nodes around us
                myPoint p1 = new myPoint(cur.x * BLOCK_WIDTH, cur.y * BLOCK_HEIGHT);
                myPoint p2 = new myPoint((cur.x + 1) * BLOCK_WIDTH, cur.y * BLOCK_HEIGHT);
                myPoint p3 = new myPoint((cur.x + 1) * BLOCK_WIDTH, (cur.y + 1) * BLOCK_HEIGHT);
                myPoint p4 = new myPoint(cur.x * BLOCK_WIDTH, (cur.y + 1) * BLOCK_HEIGHT);
                segs[0].Update(p1, p2);
                segs[1].Update(p2, p3);
                segs[2].Update(p4, p3);
                segs[3].Update(p1, p4);

                for (int i = 0; i < 4; ++i)
                {
                    int nx = cur.x + dx[i];
                    int ny = cur.y + dy[i];

                    if (nx < 0 || nx > width || ny < 0 || ny >= height || vis[ny, nx])
                        continue;

                    double possible_t;
                    if (Geometry.DerpLineSegmentCast(d, v, segs[i], out possible_t))
                    {
                        // We have a hit! If the next zone is safe to move in, then continue the bfs
                        if (gameGrid[ny, nx] != '0')
                        {
                            q.Enqueue(new SimpleNode(nx, ny, cur.step + 1));
                            vis[ny, nx] = true;
                        }
                        // We hit an unnavigable space. Stop the BFS, this is as far as we go
                        else
                        {
                            ret = true;

                            if (Math.Abs(possible_t - t) < 1e-5 && col != null)
                            {
                                // break ties by taking the furthest behind the direction we wish to go
                                // Calculate the center point on the wall, and get the dot product of the vector to that point.
                                // The most negative value is the furthest behind
                                myPoint segMidPoint1 = new myPoint((segs[i].p1.x + segs[i].p2.x) / 2.0, (segs[i].p1.y + segs[i].p2.y) / 2.0);
                                myVector toMidPoint1 = new myVector(segMidPoint1.x - d.x, segMidPoint1.y - d.y);

                                myPoint segMidPoint2 = new myPoint((col.p1.x + col.p2.x) / 2.0, (col.p1.y + col.p2.y) / 2.0);
                                myVector toMidPoint2 = new myVector(segMidPoint2.x - d.x, segMidPoint2.y - d.y);

                                if (vUnit.dot(toMidPoint1) < vUnit.dot(toMidPoint2))
                                {
                                    t = possible_t;
                                    col = new myLineSegment(segs[i].p1.x, segs[i].p1.y, segs[i].p2.x, segs[i].p2.y); // careful... memory bugs
                                }
                            }
                            else if (possible_t < t)
                            {
                                t = possible_t;
                                col = new myLineSegment(segs[i].p1.x, segs[i].p1.y, segs[i].p2.x, segs[i].p2.y); // careful... memory bugs
                            }
                        }
                    }
                }

                // if we are a special diagonal case, then check the cross hit as well
                myLineSegment diag = null;

                char c = gameGrid[cur.y, cur.x];
                if (c == '1' || c == '3')
                    diag = new myLineSegment(p2, p4);

                if (c == '2' || c == '4')
                    diag = new myLineSegment(p1, p3);
                
                if (diag != null)
                {
                    double possible_t;
                    if (Geometry.DerpLineSegmentCast(d, v, diag, out possible_t))
                    {
                        ret = true;

                        if (Math.Abs(possible_t - t) < 1e-5 && col != null)
                        {
                            // break ties by taking the furthest behind the direction we wish to go
                            // Calculate the center point on the wall, and get the dot product of the vector to that point.
                            // The most negative value is the furthest behind
                            myPoint segMidPoint1 = new myPoint((diag.p1.x + diag.p2.x) / 2.0, (diag.p1.y + diag.p2.y) / 2.0);
                            myVector toMidPoint1 = new myVector(segMidPoint1.x - d.x, segMidPoint1.y - d.y);

                            myPoint segMidPoint2 = new myPoint((col.p1.x + col.p2.x) / 2.0, (col.p1.y + col.p2.y) / 2.0);
                            myVector toMidPoint2 = new myVector(segMidPoint2.x - d.x, segMidPoint2.y - d.y);

                            if (vUnit.dot(toMidPoint1) < vUnit.dot(toMidPoint2))
                            {
                                t = possible_t;
                                col = new myLineSegment(diag.p1.x, diag.p1.y, diag.p2.x, diag.p2.y); // careful... memory bugs
                            }
                        }
                        else if (possible_t < t)
                        {
                            t = possible_t;
                            col = new myLineSegment(diag.p1.x,diag.p1.y, diag.p2.x, diag.p2.y); // careful... memory bugs
                        }
                    }
                }

                cur_step = cur.step;
            }

            return ret;
        }

        // Slide the FOV left and right
        public void SlideView(int dx)
        {
            xVel += dx;
        }

        public void Update(GameTime gameTime)
        {
            // Scroll back and forth
            if (Game.input.KeyDown(Keys.A) || Game.input.KeyDown(Keys.Left)) SlideView(-2);
            if (Game.input.KeyDown(Keys.D) || Game.input.KeyDown(Keys.Right)) SlideView(2);

            // Move the Screen with Velocity
            if (xVel < -15) xVel = -15;
            if (xVel > 15) xVel = 15;
            camX += (int)xVel;
            xVel /= 1.13;
            if (Math.Abs(xVel) < 0.9) xVel = 0.0;
            
            // Move the Screen with the mouse
            if (Game.input.MouseY() > 0 && Game.input.MouseY() < Game.GAME_HEIGHT)
            {
                // Move the screen to the Right. Cap the movement speed with the ternary operation
                if (Game.input.MouseX() < BLOCK_WIDTH * 2)
                    camX -= ((BLOCK_WIDTH * 2 - Game.input.MouseX()) / 10) <= 15 ? (BLOCK_WIDTH * 2 - Game.input.MouseX()) / 10 : 15;

                // Move the screen to the Left. Cap the movement speed with the ternary operation
                if (Game.input.MouseX() > Game.GAME_WIDTH - BLOCK_WIDTH * 2)
                    camX += ((Game.input.MouseX() - (Game.GAME_WIDTH - BLOCK_WIDTH * 2)) / 10) <= 15 ? ((Game.input.MouseX() - (Game.GAME_WIDTH - BLOCK_WIDTH * 2)) / 10) : 15;
            }

            // Cap the camera position
            if (camX < 0) camX = 0;
            if (camX > width * BLOCK_WIDTH - Game.WINDOW_WIDTH) camX = width * BLOCK_WIDTH - Game.WINDOW_WIDTH;

            // Calculare Selected box
            int selectedX = (camX + Game.input.MouseX()) / BLOCK_WIDTH;
            int selectedY = Game.input.MouseY() / BLOCK_HEIGHT;
            selectedBox = new Point(selectedX, selectedY);

            // Update all Spawners
            for (int y = 0; y < height; ++y)
            {
                if (leftSpawners[y] != null)
                {
                    leftSpawners[y].Update(gameTime);
                    rightSpawners[y].Update(gameTime);
                }
            }

            // Handle the "Mouse State"
            // This will change when we are trying to build DNA, delete DNA, etc
            // This is also where we will build DNA onto the field

            // Stop Construction if we Right-Click (a universal cancel)
            if (Game.input.constructionState != 0 && Game.input.MouseRightKeyClicked())
            {
                Game.input.constructionState = 0;
            }

            // Check for Construction if we Left-Click
            if (Game.input.constructionState != 0 && Game.input.MouseLeftKeyClicked())
            {
                // Find the X,Y coordinate in the grid that the mouse is in
                int sx = selectedBox.X - (camX / BLOCK_WIDTH);
                int sy = selectedBox.Y - (camY / BLOCK_HEIGHT);

                // Make sure we are within the game boundry, otherwise do nothing
                if (sx >= 0 && sx <= Game.GAME_WIDTH / BLOCK_WIDTH && sy >= 0 && sy <= Game.GAME_HEIGHT / BLOCK_HEIGHT && Game.input.mouseClear)
                {
                    // Check if any Spawner accepts this position as a valid DNA location
                    // If so, add the DNA to their strand and continue. Luckily there should never be overlap, so we don't have to worry about doing double.
                    int mx = Game.input.MouseX();
                    int my = Game.input.MouseY();
                    for (int i = 0; i < leftSpawners.Length; ++i)
                    {
                        if (leftSpawners[i] != null && leftSpawners[i].CheckMouseOver(camX + mx, my) != -1)
                        {
                            leftSpawners[i].AddDNA(leftSpawners[i].CheckMouseOver(camX + mx, my), Game.input.constructionState - 1);
                        }

                        // DEBUG: Only do this on the right side for debugging purposes
                        if (rightSpawners[i] != null && rightSpawners[i].CheckMouseOver(camX + mx, my) != -1)
                        {
                            rightSpawners[i].AddDNA(rightSpawners[i].CheckMouseOver(camX + mx, my), Game.input.constructionState - 1);
                        }
                    }

                    // Reset Construction State
                    Game.input.constructionState = 0;
                }
            }

            // DEBUG CODE:
            // - create a derp for now if you click on a hub
            if (Game.input.MouseLeftKeyClicked() && selectedX >= 0 && selectedX < width && selectedY >= 0 && selectedY < height && gameGrid[selectedY, selectedX] == 'A')
            {
                int hp = 5 + MyRandom.Next(TEAM.HOME, 10);
                int spd = 25 + MyRandom.Next(TEAM.HOME, 50);
                int atk = 3 + MyRandom.Next(TEAM.HOME, 4);
                int aspd = 25 + MyRandom.Next(TEAM.HOME, 50);
                int rng = 2 + MyRandom.Next(TEAM.HOME, 30);

                DerpStats stats = new DerpStats(hp, spd, atk, aspd, rng, 16, 16);
                TEAM derpTeam;

                if (selectedX == 0)
                {
                    derpTeam = TEAM.HOME;
                }
                else
                {
                    derpTeam = TEAM.AWAY;
                }
                    
                derpManager.SpawnDerp(selectedX * BLOCK_WIDTH + (BLOCK_WIDTH / 2), selectedY * BLOCK_HEIGHT + (BLOCK_HEIGHT / 2), derpTeam, stats);
            }

            // Update all the Derps
            derpManager.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the background blocks that are in the field of view
            int blockBaseX = camX / BLOCK_WIDTH;
            int blockBaseY = camY / BLOCK_HEIGHT;

            Rectangle blockPosition = new Rectangle();

            for (int y = 0; y < Game.GAME_HEIGHT / BLOCK_HEIGHT; ++y)
            {
                for (int x = 0; x <= Game.GAME_WIDTH / BLOCK_WIDTH; ++x)
                {
                    // Make sure this item exists in the Dictionary
                    int offset = 0;
                    char curGrid = gameGrid[blockBaseY + y, blockBaseX + x];

                    if (!blockPositionOffset.TryGetValue(curGrid, out offset))
                        continue;;

                    // Calculate the Position and Destination viewport size to draw
                    blockPosition.X = (blockBaseX + x) * BLOCK_WIDTH - camX;
                    blockPosition.Y = y * BLOCK_HEIGHT - offset;
                    blockPosition.Width = BLOCK_WIDTH;
                    blockPosition.Height = BLOCK_HEIGHT + offset;

                    spriteBatch.Draw(blockTexture[curGrid], blockPosition, blockSrcOffset[curGrid], Color.White);
                }
            }

            Rectangle camera = new Rectangle(camX, 0, Game.GAME_WIDTH, Game.GAME_HEIGHT);

            // Draw dem derps
            derpManager.Draw(spriteBatch, camera);

            // Draw all Spawners
            for (int y = 0; y < height; ++y)
            {
                if (leftSpawners[y] != null)
                {
                    // Check to see if we need to ghost a position
                    if (Game.input.constructionState != 0 && leftSpawners[y].CheckMouseOver(camX + Game.input.MouseX(), Game.input.MouseY()) != -1)
                    {
                        leftSpawners[y].ghostingIndex = leftSpawners[y].CheckMouseOver(camX + Game.input.MouseX(), Game.input.MouseY());
                    }
                    else
                    {
                        leftSpawners[y].ghostingIndex = -1;
                    }

                    // DEBUG: Do this only on the right side for debugging purposes
                    // Check to see if we need to ghost a position
                    if (Game.input.constructionState != 0 && rightSpawners[y].CheckMouseOver(camX + Game.input.MouseX(), Game.input.MouseY()) != -1)
                    {
                        rightSpawners[y].ghostingIndex = rightSpawners[y].CheckMouseOver(camX + Game.input.MouseX(), Game.input.MouseY());
                    }
                    else
                    {
                        rightSpawners[y].ghostingIndex = -1;
                    }


                    leftSpawners[y].Draw(spriteBatch, camera);
                    rightSpawners[y].Draw(spriteBatch, camera);
                }
            }

            // Draw the Selection Box, Ghost DNA or Delestroyer Box at the cursor's location
            int sx = selectedBox.X - blockBaseX;
            int sy = selectedBox.Y - blockBaseY;
            if (sx >= 0 && sx <= Game.GAME_WIDTH / BLOCK_WIDTH &&
                sy >= 0 && sy <= Game.GAME_HEIGHT / BLOCK_HEIGHT && Game.input.mouseClear)
            {
                blockPosition.X = (blockBaseX + sx) * BLOCK_WIDTH - camX;
                blockPosition.Y = sy * BLOCK_HEIGHT;
                blockPosition.Width = BLOCK_WIDTH;
                blockPosition.Height = BLOCK_HEIGHT;

                // If we are currently just selecting
                if (Game.input.constructionState == 0)
                {
                    spriteBatch.Draw(selectorTexture[0], blockPosition, Color.White);
                }
                // If we are currently deleting
                else if (Game.input.constructionState < 0)
                {
                    spriteBatch.Draw(selectorTexture[1], blockPosition, Color.White);
                }
                // Otherwise we must be building, draw a transparent dna
                else
                {
                    spriteBatch.Draw(Spawner.DNATexture[Game.input.constructionState-1], blockPosition, Color.White * 0.5f);
                }
            }
        }
    }
}
