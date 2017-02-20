using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GameOfSounds.Resources;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Threading;

/*******************************
 * THE GAME OF SOUNDS
 * A TECHNOLOGICAL/DIPLOMATIC FEVERDREAM
 * Game Page- this is where most of the game content is located
 * 
 * author: Skyler Goodell
 *******************************/

namespace GameOfSounds
{
    public partial class GamePage : PhoneApplicationPage
    {
         // Singleton hack
        public static GamePage gamePage;

        // Start Position and environment constants
        const double startX = 0.0;
        const double startY = 0.0;
        const double MapWidth = 3730;
        const double MapHeight = 3200;

        const double friction = 0.8;

        // **************
        // Various Images
        // **************
        // Toggle Buttons
        BitmapImage[] zoomButtonImage;

        // **************
        // Structs make everything beautiful.
        // **************
        class Zone
        {
            public int ID;
            public string name;
            public string filename;
            public int offsetX, offsetY;
            public int originalWidth, originalHeight;
            public int centerX, centerY;

            public Image zoneImage;
        }
        Dictionary<int, Zone> zoneDict;

        // Current Map Location, velocity and scale
        struct MapState
        {
            public double x, y;
            public double vx, vy;
            public bool zoomed;

            public double OriginalSizeX;
            public double OriginalSizeY;
            public double scaleUp;
            public double scaleDown;
        }
        MapState mapState;

        // The Mouse State
        struct MouseState
        {
            public Point? mouseOrigin;
            public bool mouseDown;
            public Zone selectedZone;
        }
        MouseState mouseState;

        // The Game State
        struct GameState
        {
            public int TurnNumber;
            public int Year;
            public int Season;

            public bool loggedIn;
            public int playerID;
        }
        GameState gameState;

        // Timer for Tick Method
        DispatcherTimer tick;

        // REST client interface
        RestCommand restCommand;

        // Constructor
        public GamePage()
        {
            // Create the XAML defined components
            InitializeComponent();

            // set singleton
            gamePage = this;

            // create rest client
            restCommand = new RestCommand();

            // Load Various Images
            LoadImages();

            // Load Zones
            LoadZones();

            // Initialize the state
            mapState.x = startX;
            mapState.y = startY;
            mapState.vx = mapState.vy = 0.0;

            mouseState.mouseOrigin = null;
            mouseState.mouseDown = false;
            mouseState.selectedZone = null;

            // Copy the original size and zoom out. Default to zoomed out.
            mapState.OriginalSizeX = MapWidth;
            mapState.OriginalSizeY = MapHeight;
            mapState.scaleDown = canvas.ActualHeight / mapState.OriginalSizeY; // used for all the scaling
            mapState.scaleUp = mapState.OriginalSizeY / canvas.ActualHeight; // used for all the scaling
            mapState.zoomed = false;

            zoomScreen(mapState.zoomed);

            // Start the tick
            tick = new DispatcherTimer();
            tick.Interval = new TimeSpan(500);
            tick.Tick += tick_Tick;
            tick.Start();
        }

        // ***********
        // Tick Method
        // Note: Perf issues- http://msdn.microsoft.com/en-us/library/windowsphone/develop/ff967560(v=vs.105).aspx#BKMK_Images
        // ***********
        int count = 0;
        private void tick_Tick(object sender, EventArgs e)
        {
            if (!mouseState.mouseDown && (Math.Abs(mapState.vx) > 1e-3 || Math.Abs(mapState.vy) > 1e-3))
            {
                // slow velocity
                mapState.vx *= friction;
                mapState.vy *= friction;

                moveScreen(mapState.vx, mapState.vy);
            }
        }

        // **************
        // Canvas Methods
        // **************
        #region CANVAS METHODS

        // Finger Down
        private void canvas_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            canvas.CaptureMouse();

            mouseState.mouseDown = true;
            mouseState.mouseOrigin = e.GetPosition(canvas);

            mapState.vx = mapState.vy = 0.0;
        }

        // Finger Up
        private void canvas_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            mouseState.mouseOrigin = null;
            mouseState.mouseDown = false;

            canvas.ReleaseMouseCapture();
        }

        // Move finger
        private void canvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // Don't move the frame if we don't have a reference origin
            if (mouseState.mouseOrigin == null)
            {
                return;
            }

            var newMousePoint = e.GetPosition(canvas);

            // Set Velocity
            mapState.vx = newMousePoint.X - mouseState.mouseOrigin.Value.X;
            mapState.vy = newMousePoint.Y - mouseState.mouseOrigin.Value.Y;

            moveScreen(mapState.vx, mapState.vy);

            mouseState.mouseOrigin = newMousePoint;
        }

        // Tap on the canvas
        private void canvas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            // stop velocity
            mapState.vx = mapState.vy = 0;

            // Get position relative to the mapCanvas
            int x = (int)e.GetPosition(mapCanvas).X;
            int y = (int)e.GetPosition(mapCanvas).Y;

            int hits = 0;

            // Check if the tap was on a zone
            foreach (var zonePair in zoneDict)
            {
                var zone = zonePair.Value;
                
                // scale Position takes into account if we are zoomed now or not
                if (x >= zone.offsetX * (mapState.zoomed ? 1.0 : mapState.scaleDown) && x <= (zone.offsetX + zone.originalWidth) * (mapState.zoomed ? 1.0 : mapState.scaleDown) &&
                    y >= zone.offsetY * (mapState.zoomed ? 1.0 : mapState.scaleDown) && y <= (zone.offsetY + zone.originalHeight) * (mapState.zoomed ? 1.0 : mapState.scaleDown))
                {
                    var bitmap = (WriteableBitmap)zone.zoneImage.Source;

                    double dx = x - zone.offsetX * (mapState.zoomed ? 1.0 : mapState.scaleDown);
                    double dy = y - zone.offsetY * (mapState.zoomed ? 1.0 : mapState.scaleDown);

                    if (!mapState.zoomed)
                    {
                        dx *= mapState.scaleUp;
                        dy *= mapState.scaleUp;
                    }

                    if (((bitmap.Pixels[(int)dx + (int)dy * bitmap.PixelWidth] >> 24) & 255) > 0)
                    {
                        if (mouseState.selectedZone == null || mouseState.selectedZone.ID != zone.ID)
                        {
                            if (mouseState.selectedZone != null)
                            {
                                setZoneColor(mouseState.selectedZone.zoneImage, 255, 255, 255);
                            }

                            mouseState.selectedZone = zone;
                            setZoneColor(zone.zoneImage, 255, 216, 0);

                            DebugTextbox.Text = "Hit: " + zone.name;
                            ++hits;
                        }
                    }
                }
            }

            if (hits > 1)
            {
                DebugTextbox.Text = "ERROR: multiple hits";
            }
        }

        // Tap the zoom in/out button (not technically the canvas, but it manipulated it)
        private void zoomButton_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            mapState.zoomed = !mapState.zoomed;

            zoomScreen(mapState.zoomed);
        }
        #endregion

        

        // ****************
        // Helper Functions
        // ****************
        private void moveScreen(double move_x, double move_y)
        {
            mapState.x = cap(mapState.x + move_x, -(map.Width - canvas.Width), 0.0);
            mapState.y = cap(mapState.y + move_y, -(map.Height - canvas.Height), 0.0);

            Canvas.SetLeft(mapCanvas, mapState.x);
            Canvas.SetTop(mapCanvas, mapState.y);
        }

        // Zoom the screen in and out
        private void zoomScreen(bool zoomed)
        {
            if (mapState.zoomed)
            {
                zoomButton.Source = zoomButtonImage[1];

                // Scale map to original size
                map.Height = mapState.OriginalSizeY;
                map.Width = mapState.OriginalSizeX;

                // Scale Zones to original size
                foreach (var zonePair in zoneDict)
                {
                    var zone = zonePair.Value;
                    zone.zoneImage.Height = zone.originalHeight;
                    zone.zoneImage.Width = zone.originalWidth;
                    Canvas.SetLeft(zone.zoneImage, zone.offsetX);
                    Canvas.SetTop(zone.zoneImage, zone.offsetY);
                }

                // scale the current position to the new scale
                // The current position is calulated from the center of the screen
                double deltax = ((canvas.ActualWidth / 2.0) - mapState.x) * mapState.scaleUp - ((canvas.ActualWidth / 2.0) - mapState.x);
                double deltay = ((canvas.ActualHeight / 2.0) - mapState.y) * mapState.scaleUp - ((canvas.ActualHeight / 2.0) - mapState.y);
                moveScreen(-deltax, -deltay);
            }
            else
            {
                zoomButton.Source = zoomButtonImage[0];

                // Scale map to smaller, zoomed-out size
                map.Height = mapState.OriginalSizeY * mapState.scaleDown;
                map.Width = mapState.OriginalSizeX * mapState.scaleDown;

                foreach (var zonePair in zoneDict)
                {
                    var zone = zonePair.Value;
                    zone.zoneImage.Height = zone.originalHeight * mapState.scaleDown;
                    zone.zoneImage.Width = zone.originalWidth * mapState.scaleDown;
                    Canvas.SetLeft(zone.zoneImage, zone.offsetX * mapState.scaleDown);
                    Canvas.SetTop(zone.zoneImage, zone.offsetY * mapState.scaleDown);
                }

                // scale the current position to the new scale
                // The current position is calulated from the center of the screen
                double deltax = ((canvas.ActualWidth / 2.0) - mapState.x) * mapState.scaleDown - ((canvas.ActualWidth / 2.0) - mapState.x);
                double deltay = ((canvas.ActualHeight / 2.0) - mapState.y) * mapState.scaleDown - ((canvas.ActualHeight / 2.0) - mapState.y);
                moveScreen(-deltax, -deltay);
            }
        }

        // Cap values
        private double cap(double val, double min, double max)
        {
            if (val < min)
                return min;
            if (val > max)
                return max;
            return val;
        }

        // Change the color of a zone to the given RGB values
        private void setZoneColor(Image zone, int R, int G, int B)
        {
            var zoneBitmapWriteable = (WriteableBitmap)zone.Source;
            for (int y = 0; y < zoneBitmapWriteable.PixelHeight; ++y)
            {
                for (int x = 0; x < zoneBitmapWriteable.PixelWidth; ++x)
                {
                    int pixelVal = zoneBitmapWriteable.Pixels[x + y * zoneBitmapWriteable.PixelWidth];

                    int curR = (pixelVal >> 16) & 255;
                    int curG = (pixelVal >> 8) & 255;
                    int curB = (pixelVal >> 0) & 255;
                    int curA = (pixelVal >> 24) & 255;

                    if (curA != 0)
                    {
                        zoneBitmapWriteable.Pixels[x + y * zoneBitmapWriteable.PixelWidth] = (R << 16) | (G << 8) | (B << 0) | (curA << 24);
                    }
                }
            }

            zone.Source = zoneBitmapWriteable;
        }

        // Load the various random images we will need
        private void LoadImages()
        {
            // Toggle Button
            zoomButtonImage = new BitmapImage[2];
            zoomButtonImage[0] = new BitmapImage(new Uri(string.Format("/Assets/Buttons/Button_Zoom.png"), UriKind.Relative));
            zoomButtonImage[1] = new BitmapImage(new Uri(string.Format("/Assets/Buttons/Button_UnZoom.png"), UriKind.Relative));
        }

        // Load the Zones from the ZoneList text file
        private void LoadZones()
        {
            StreamReader zoneMetadata = new StreamReader("Assets/Zones/ZoneList.txt");
            zoneDict = new Dictionary<int, Zone>();

            // Read the metadata lines from the Zones.txt file
            while (!zoneMetadata.EndOfStream)
            {
                string line = zoneMetadata.ReadLine();

                // Ignore comments
                if (line.StartsWith("//"))
                {
                    continue;
                }

                // Split: <ID> <name> <fileName> <offsetX> <offsetY> <centerX> <centerY>
                var lineSplit = line.Split(' ');

                Zone newZone = new Zone();
                newZone.ID = int.Parse(lineSplit[0]);
                newZone.name = lineSplit[1].Replace('_', ' ');
                newZone.filename = lineSplit[2];
                newZone.offsetX = int.Parse(lineSplit[3]);
                newZone.offsetY = int.Parse(lineSplit[4]);
                newZone.centerX = int.Parse(lineSplit[5]);
                newZone.centerY = int.Parse(lineSplit[6]);

                // Create the image
                var zoneImage = new Image();
                zoneImage.IsHitTestVisible = false;

                var uriSource = new Uri(string.Format("/Assets/Zones/{0}", newZone.filename), UriKind.Relative);
                var zoneBitmapImage = new BitmapImage();
                zoneBitmapImage.CreateOptions = BitmapCreateOptions.None; // force bitmap load right now
                zoneBitmapImage.UriSource = uriSource;

                // Note to future Skyler: I'm sorry if you haveto do this asynchrounously later. I'm just not feeling like doing it right now. Enjoy the load lag.
                var writeableZoneBitmapImage = new WriteableBitmap(zoneBitmapImage);
                zoneImage.Source = writeableZoneBitmapImage;

                // Grab these after the image has loaded
                newZone.originalWidth = zoneBitmapImage.PixelWidth;
                newZone.originalHeight = zoneBitmapImage.PixelHeight;

                // Make the zone white to start
                setZoneColor(zoneImage, 255, 255, 255);

                // Add the image to the struct and to the canvas
                newZone.zoneImage = zoneImage;
                mapCanvas.Children.Add(newZone.zoneImage);

                // Add the Zone to the dictionary
                zoneDict.Add(newZone.ID, newZone);
            }
        }
    }
}