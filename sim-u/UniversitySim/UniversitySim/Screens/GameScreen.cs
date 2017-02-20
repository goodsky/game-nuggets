using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Framework;
using UniversitySim.GUI;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.Screens
{
    /// <summary>
    /// The game screen
    /// </summary>
    class GameScreen : BaseScreen
    {
        // Configuration file for the room
        Config roomConfig;

        // Media required for drawing
        // the tile image that is the background
        Texture2D tile;
        Texture2D cursor;

        // Fonts used on game screen
        SpriteFont defaultFont;

        // //////////////////////
        // GUI
        // //////////////////////

        // Side folder on the left of the screen where you build stuff from
        private static SideFolder folder;

        // Heads up GUI in the top center of the screen
        private static HUD hud;

        // Bottom status bar
        private static StatusBar status;

        // //////////////////////
        // BUILDING DATA
        // //////////////////////

        // Catalog of buildings
        private static CampusCatalog campusCatalog;

        // Construction Key- null when not constructing anything
        public static GhostConstruction Construction = null;

        // Width of the game room
        public static int GameWidth { get; private set; }

        // Height of the game room
        public static int GameHeight { get; private set; }

        // White cursor object to draw backgrounds
        public static Texture2D WhiteCursor;

        /// <summary>
        /// Set the Gamestate to be building this building key
        /// </summary>
        /// <param name="key">ID of the building to be built</param>
        public static void BeginBuilding(string key)
        {
            // Stop constructing anything old
            if (GameScreen.Construction != null)
            {
                GameScreen.Construction.Delete();
            }

            if (string.IsNullOrEmpty(key))
            {
                GameScreen.Construction = null;
            }
            else
            {
                var data = campusCatalog.GetData(key);
                StatusBar.UpdateStatus(string.Format("Building {0}.", data.Name));

                // Building construction and Path construction are different beasts
                switch (data.Type)
                { 
                    case DataType.Building:
                        GameScreen.Construction = new GhostBuilding(data as BuildingData);
                        break;

                    case DataType.Path:
                        GameScreen.Construction = new GhostPath(data as PathData);
                        break;

                    default:
                        Logger.Log(LogLevel.Error, "GameScreen Unknown building", "Trying to build unknown building type. Building " + data.Name);
                        break;
                }

                // folder.AddChild(GameScreen.Construction); // this is an uneccessary relationship, also it leaves the folder open on the left side which blocks the view
                ScreenElementManager.Instance.SetNewFocus(GameScreen.Construction, LeftOrRight.Neither);
            }
        }

        /// <summary>
        /// Create an instance of a game room.
        /// Use the config file to set up information about this room.
        /// </summary>
        /// <param name="configFile"></param>
        public GameScreen(string configFile)
        {
            this.roomConfig = new Config(configFile);
        }

        /// <summary>
        /// Set up the game room.
        /// </summary>
        public override void Initialize() 
        {
            int startX = this.roomConfig.GetIntValue("CameraStartX", 0);
            int startY = this.roomConfig.GetIntValue("CameraStartY", 0);
            GameWidth = this.roomConfig.GetIntValue("RoomWidth", Constants.WINDOW_WIDTH);
            GameHeight = this.roomConfig.GetIntValue("RoomHeight", Constants.WINDOW_HEIGHT);

            // set up the singleton camera
            Camera.Setup(startX, startY, GameWidth, GameHeight);

            // Initialize Building stuff
            campusCatalog = new CampusCatalog();

            // Initialize the Building manager
            CampusManager.Instance.Initialize(GameWidth, GameHeight);

            // Initialize GUI
            folder = new SideFolder(campusCatalog);
            hud = new HUD();
            status = new StatusBar();
        }

        /// <summary>
        /// Load images needed for the game room
        /// </summary>
        /// <param name="contentMan"></param>
        public override void LoadContent(ContentManager contentMan) 
        {
            // Load fonts
            this.defaultFont = contentMan.Load<SpriteFont>(@"Fonts\DefaultFont");

            // Load background elements
            this.tile = contentMan.Load<Texture2D>(@"GrassTile");
            this.cursor = contentMan.Load<Texture2D>(@"IsoCursor");
            GameScreen.WhiteCursor = contentMan.Load<Texture2D>(@"IsoCursorWhite");

            // Initialize the building catalog
            campusCatalog.LoadContent(contentMan);

            // Initialize the misc image catalog
            ImageCatalog.Instance.LoadContent(contentMan);

            // Load the GUI
            folder.LoadContent(contentMan);
            hud.LoadContent(contentMan);
            status.LoadContent(contentMan);
        }

        /// <summary>
        /// Unload the Game Screen Content
        /// </summary>
        public override void UnloadContent() 
        {
            // some day we might have to actually dispose our content
            // but right now I just load everything into memory at once. it doesn't seem too bad.
            GameScreen.WhiteCursor.Dispose();
        }

        /// <summary>
        /// Update loop.
        /// </summary>
        /// <param name="gametime"></param>
        public override void Update(GameTime gameTime) 
        {
            // Update the state of the mouse. This will call the element manager to check "MouseOver" and "Click" events.
            this.CheckMouseState();

            // Move the camera if required.
            this.CalculateCameraMovement();

            // Update all elements finally.
            ScreenElementManager.Instance.UpdateElements(gameTime);
        }

        /// <summary>
        /// Draw loop.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch) 
        {
            // Draw the grass background and lines
            this.DrawBackground(spriteBatch);
            
            // Draw other misc elements (cursor, etc)
            this.DrawElements(gameTime, spriteBatch);

            // Draw game elements
            ScreenElementManager.Instance.DrawElements(gameTime, spriteBatch);

            // DEBUGGING OUTPUT
            this.DrawDebug(spriteBatch);
        }
        
        // -----------------------------------------------------
        // UPDATE STEP METHODS
        // -----------------------------------------------------

        // describe what the mouse is doing right now
        MouseState mouseState = MouseState.UnClicked;
        MouseState mouseState2 = MouseState.UnClicked; // right click

        // The position of the mouse down event in WORLD COORDINATES
        Pair<int> mouseDownPosition = new Pair<int>(0, 0);
        Pair<int> mouseDownPosition2 = new Pair<int>(0, 0);

        // The element under the mouse down event (if any)
        ScreenElement mouseDownElement = null;
        ScreenElement mouseDownElement2 = null;

        // The time of the mouse down event
        DateTime mouseDownTime = DateTime.Now;
        DateTime mouseDownTime2 = DateTime.Now;

        // The previous mouse's position in WORLD COORDINATES
        Pair<int> lastMouse = new Pair<int>(0, 0);

        // This enumeration will describe all possible states of the mouse after a mouse click
        enum MouseState
        {
            UnClicked,      // default state
            Clicked,        // mouse is clicked, but it is not moving the camera or over a valid element (basically the do-nothing mouse state)
            ClickedNothing,
            ClickedElement,
            ClickedGUI,
            CameraMoving
        }

        /// <summary>
        /// Check and update the state of the mouse.
        /// This will see if we are clicking on UI, elements or moving the camera
        /// </summary>
        private void CheckMouseState()
        {
            var mouseOver = ScreenElementManager.Instance.CheckMouseState(Input.MouseWorldPosition);

            // LEFT MOUSE --------------------------------------------------------------------------
            // Mouse down event
            // Ignore when the game state is constructing
            if (Input.MouseLeftKeyClicked())
            {
                mouseDownPosition.x = lastMouse.x = Input.MouseX;
                mouseDownPosition.y = lastMouse.y = Input.MouseY;
                mouseDownElement = mouseOver;
                mouseDownTime = DateTime.Now;

                if (mouseOver == null)
                {
                    mouseState = MouseState.ClickedNothing;
                }
                else if (mouseOver.Type == ElementType.GUI)
                {
                    mouseState = MouseState.ClickedGUI;
                }
                else
                {
                    mouseState = MouseState.ClickedElement;
                }

                // Stop any rightclicking nonsense that might be going one. Left click is the superior click!
                // UPDATE: since we've moved right-click to the camera moving click... we have removed left click's superiority
                //mouseState2 = MouseState.Clicked;
            }

            // Check if the user is moving the mouse like they want to move the camera
            // Here's my thought process:
            //  If you clicked nothing, then you will turn into a camera movement if you move from the orgin or move the mouse a certain distance from the start
            //  If you clicked down on a GUI element then we will never move the camera. However if you mouse off the original element before you release the mouse then don't fire the clicked event.
            //  For any other mouse down state you want to turn to "camera moving" if the mouse is either clicked too long or you start moving the mouse like you want to move.
            if (mouseState == MouseState.ClickedNothing)
            {
                if (Utilities.Geometry.Dist(lastMouse, mouseDownPosition) > Constants.CLICK_PIXEL_DISTANCE || DateTime.Now.Subtract(mouseDownTime).TotalMilliseconds > Constants.CLICK_MILLISECONDS)
                {
                    mouseState = MouseState.CameraMoving;
                }
            }
            else if (mouseState == MouseState.ClickedGUI)
            {
                if (mouseOver == null || !mouseOver.Equals(mouseDownElement))
                {
                    mouseState = MouseState.Clicked;
                }
            }
            else if (mouseState == MouseState.ClickedElement)
            {
                if (mouseOver == null || !mouseOver.Equals(mouseDownElement) || Utilities.Geometry.Dist(lastMouse, mouseDownPosition) > Constants.CLICK_PIXEL_DISTANCE || DateTime.Now.Subtract(mouseDownTime).TotalMilliseconds > Constants.CLICK_MILLISECONDS)
                {
                    mouseState = MouseState.CameraMoving;
                }
            }

            // On release check if we need to click an element
            if (Input.MouseLeftKeyReleased())
            {
                if (mouseState == MouseState.ClickedElement || mouseState == MouseState.ClickedGUI)
                {
                    ScreenElementManager.Instance.SetNewFocus(mouseOver, LeftOrRight.Left);
                }
                else if (mouseState == MouseState.ClickedNothing)
                {
                    StatusBar.UpdateStatus(string.Empty);
                    ScreenElementManager.Instance.SetNewFocus(null, LeftOrRight.Left);
                }

                mouseState = MouseState.UnClicked;
            }

            // RIGHT MOUSE --------------------------------------------------------------------------
            // Right mouse currenly only will lose focus when you click on nothing and release quickly
            if (Input.MouseRightKeyClicked())
            {
                mouseDownPosition2.x = Input.MouseX;
                mouseDownPosition2.y = Input.MouseY;
                mouseDownElement2 = mouseOver;
                mouseDownTime2 = DateTime.Now;

                if (mouseOver == null)
                {
                    mouseState2 = MouseState.ClickedNothing;
                }
                else if (mouseOver.Type == ElementType.GUI)
                {
                    mouseState2 = MouseState.ClickedGUI;
                }
                else
                {
                    mouseState2 = MouseState.ClickedElement;
                }
            }

            // Update step for the right mouse click
            if (mouseState2 == MouseState.ClickedNothing)
            {
                if (Utilities.Geometry.Dist(lastMouse, mouseDownPosition2) > Constants.CLICK_PIXEL_DISTANCE || DateTime.Now.Subtract(mouseDownTime2).TotalMilliseconds > Constants.CLICK_MILLISECONDS)
                {
                    mouseState2 = MouseState.CameraMoving;
                }
            }
            else if (mouseState2 == MouseState.ClickedGUI)
            {
                if (mouseOver == null || !mouseOver.Equals(mouseDownElement2))
                {
                    mouseState2 = MouseState.Clicked;
                }
            }
            else if (mouseState2 == MouseState.ClickedElement)
            {
                if (mouseOver == null || !mouseOver.Equals(mouseDownElement2) || Utilities.Geometry.Dist(lastMouse, mouseDownPosition2) > Constants.CLICK_PIXEL_DISTANCE || DateTime.Now.Subtract(mouseDownTime2).TotalMilliseconds > Constants.CLICK_MILLISECONDS)
                {
                    mouseState2 = MouseState.CameraMoving;
                }
            }

            // Right click currently can move your focus up a parent level.
            // Also, if the left mouse (mouseState) is clicked at all then ignore the right click.
            // My current mouse philosophy is very anti right-click
            if (Input.MouseRightKeyReleased())
            {
                if (mouseState2 == MouseState.ClickedElement || mouseState2 == MouseState.ClickedGUI)
                {
                    ScreenElementManager.Instance.SetNewFocus(mouseOver, LeftOrRight.Right);
                }
                else if (mouseState2 == MouseState.ClickedNothing &&
                    DateTime.Now.Subtract(mouseDownTime2).TotalMilliseconds <= Constants.CLICK_MILLISECONDS)
                {
                    StatusBar.UpdateStatus(string.Empty);
                    ScreenElementManager.Instance.SetNewFocus(null, LeftOrRight.Right);
                }

                mouseState2 = MouseState.UnClicked;
            }
        }

        /// <summary>
        /// Calculate the camera movement this step
        /// </summary>
        private void CalculateCameraMovement()
        {
            // Move if the mouse is clicked and dragging
            if (mouseState2 == MouseState.CameraMoving)
            {
                Camera.Instance.moveCamera(lastMouse.x - Input.MouseX, lastMouse.y - Input.MouseY);
            }
            // Move by mouse proximity to the edge and arrow keys
            else
            {
                int dx = 0; int dy = 0;
                bool isInScreen = true;
                bool mouseScroll = false;

                // Make sure the mouse isn't far outside the screen
                // TODO: make sure this only happens if the screen is in focus
                if (Input.MouseX < -100 || Input.MouseX > Constants.WINDOW_WIDTH + 100 || Input.MouseY < -100 || Input.MouseY > Constants.WINDOW_HEIGHT + 100)
                {
                    isInScreen = false;
                }

                // Use 75px as "hot zone" for moving
                // TODO: read the movement speed value from a config file
                if ((mouseScroll && isInScreen && Input.MouseX < 75) || Input.KeyDown(Keys.Left))
                {
                    dx -= 5;
                }

                if ((mouseScroll && isInScreen && Input.MouseX > Constants.WINDOW_WIDTH - 75) || Input.KeyDown(Keys.Right))
                {
                    dx += 5;
                }

                if ((mouseScroll && isInScreen && Input.MouseY < 75) || Input.KeyDown(Keys.Up))
                {
                    dy -= 5;
                }

                if ((mouseScroll && isInScreen && Input.MouseY > Constants.WINDOW_HEIGHT - 75) || Input.KeyDown(Keys.Down))
                {
                    dy += 5;
                }

                Camera.Instance.moveCamera(dx, dy);
            }

            lastMouse.x = Input.MouseX;
            lastMouse.y = Input.MouseY;
        }

        // -----------------------------------------------------
        // DRAW STEP METHODS
        // -----------------------------------------------------

        /// <summary>
        /// Draw the grass and tiles on the background
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawBackground(SpriteBatch spriteBatch)
        {
            int offsetX = Camera.Instance.x % Constants.TILE_WIDTH;
            int offsetY = Camera.Instance.y % Constants.TILE_HEIGHT;

            int xIter = (Constants.WINDOW_WIDTH / Constants.TILE_WIDTH) + 2;
            int yIter = (Constants.WINDOW_HEIGHT / Constants.TILE_HEIGHT) + 2;

            // loop and draw the background
            Rectangle location = new Rectangle(-offsetX, -offsetY, Constants.TILE_WIDTH, Constants.TILE_HEIGHT);
            for (int y = 0; y < yIter; ++y)
            {
                location.Y = -offsetY + (y * Constants.TILE_HEIGHT);
                for (int x = 0; x < xIter; ++x)
                {
                    location.X = -offsetX + (x * Constants.TILE_WIDTH);

                    spriteBatch.Draw(tile, location, Color.White);
                }
            }
        }

        /// <summary>
        /// Draw game elements.
        /// Also draw the mouse cursor and other random elements.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="spriteBatch"></param>
        /// 
        private void DrawElements(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw mouse cursor
            if (mouseState2 == MouseState.UnClicked && Construction == null)
            {
                spriteBatch.Draw(GameScreen.WhiteCursor, new Rectangle(Input.IsoWorldX - Camera.Instance.x, Input.IsoWorldY - Camera.Instance.y, Constants.TILE_WIDTH, Constants.TILE_HEIGHT), Color.Black * 0.5f);
            }
        }

        /// <summary>
        /// This method draws the debugging elements over the UI.
        /// Don't forget to remove this sort of stuff before the game is "released"
        /// </summary>
        /// <param name="spriteBatch"></param>
        private void DrawDebug(SpriteBatch spriteBatch)
        {
            //spriteBatch.DrawString(defaultFont, string.Format("MouseX: {0} MouseY: {1}", Input.MouseX, Input.MouseY), new Vector2(50.0f, 5.0f), Color.Black);
            //spriteBatch.DrawString(defaultFont, string.Format("IsoX: {0} IsoY: {1}", Input.IsoGridX, Input.IsoGridY), new Vector2(50.0f, 25.0f), Color.Black);
        }
    }
}
