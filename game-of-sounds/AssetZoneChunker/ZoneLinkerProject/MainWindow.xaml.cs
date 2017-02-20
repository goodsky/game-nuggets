using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZoneLinkerProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly string ZoneLocation = "../OutputZones";
        readonly double Scale = 0.25;

        // Zone Struct from the Game Of Sounds project
        // ADDED ADJACENCIES
        class Zone
        {
            public int ID;
            public string name;
            public string filename;
            public int offsetX, offsetY;
            public int originalWidth, originalHeight;
            public int centerX, centerY;
            public bool[] adj;
            public Line[] adjLine;

            public Image zoneImage;
        }
        Dictionary<int, Zone> zoneDict;

        // Construct!!!
        public MainWindow()
        {
            InitializeComponent();
            LoadZones();
            CreateAdjacencyMatrix();
        }

        // Tap on stuff
        Zone selected = null;
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Get position relative to the mapCanvas
            int x = (int)e.GetPosition(canvas).X;
            int y = (int)e.GetPosition(canvas).Y;

            int hits = 0;

            // Check if the tap was on a zone
            foreach (var zonePair in zoneDict)
            {
                var zone = zonePair.Value;

                // scale Position takes into account if we are zoomed now or not
                if (x >= zone.offsetX * Scale && x <= (zone.offsetX + zone.originalWidth) * Scale &&
                    y >= zone.offsetY * Scale && y <= (zone.offsetY + zone.originalHeight) * Scale)
                {
                    var bitmap = (WriteableBitmap)zone.zoneImage.Source;

                    double dx = x - zone.offsetX * Scale;
                    double dy = y - zone.offsetY * Scale;

                    dx *= 4;
                    dy *= 4;

                    int height = bitmap.PixelHeight;
                    int width = bitmap.PixelWidth;
                    int nStride = (bitmap.PixelWidth * bitmap.Format.BitsPerPixel + 7) / 8;
                    byte[] pixelByteArray = new byte[bitmap.PixelHeight * nStride];

                    bitmap.CopyPixels(pixelByteArray, nStride, 0);

                    int B = pixelByteArray[((int)dx + (int)dy * bitmap.PixelWidth) * 4 + 0];
                    int G = pixelByteArray[((int)dx + (int)dy * bitmap.PixelWidth) * 4 + 1];
                    int R = pixelByteArray[((int)dx + (int)dy * bitmap.PixelWidth) * 4 + 2];
                    int A = pixelByteArray[((int)dx + (int)dy * bitmap.PixelWidth) * 4 + 3];

                    if (A > 0)
                    {
                        ++hits;

                        if (selected == null)
                        {
                            DebugTextbox.Text = "Selected: " + zone.name;
                            selected = zone;
                        }
                        else
                        {
                            if (Mouse.LeftButton == MouseButtonState.Pressed)
                            {
                                int i = zone.ID - 1;
                                int j = selected.ID - 1;

                                if (zone.adj[j])
                                {
                                    DebugTextbox.Text = "Already Exists: " + selected.name + " -> " + zone.name;
                                }
                                else
                                {
                                    zone.adj[j] = true;
                                    selected.adj[i] = true;
                                    zone.adjLine[j] = createLine(zone, selected);
                                    selected.adjLine[i] = new Line();

                                    canvas.Children.Add(zone.adjLine[j]);
                                    canvas.Children.Add(selected.adjLine[i]);

                                    DebugTextbox.Text = "Connection Added: " + selected.name + " -> " + zone.name;
                                }
                            }
                            else
                            {
                                int i = zone.ID - 1;
                                int j = selected.ID - 1;

                                if (zone.adj[j])
                                {
                                    zone.adj[j] = false;
                                    selected.adj[i] = false;

                                    canvas.Children.Remove(zone.adjLine[j]);
                                    canvas.Children.Remove(selected.adjLine[i]);

                                    DebugTextbox.Text = "Connection Removed: " + selected.name + " -> " + zone.name;
                                }
                                else
                                {
                                    DebugTextbox.Text = "No connection: " + selected.name + " -> " + zone.name;
                                }
                            }
                            selected = null;
                        }
                    }
                }
            }

            if (hits == 0)
            {
                selected = null;
                DebugTextbox.Text = "";
            }
            if (hits > 1)
            {
                DebugTextbox.Text = "ERROR: multiple hits";
            }
        }

        // Load the Zones from the ZoneList text file
        // Note: copy pasta from the GamePage.xaml.cs from Game of Sounds
        private void LoadZones()
        {
            StreamReader zoneMetadata = new StreamReader(string.Format("{0}/ZoneList.txt", ZoneLocation));
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

                var uriSource = new Uri(string.Format("{0}/{1}", ZoneLocation, newZone.filename), UriKind.Relative);
                var zoneBitmapImage = new BitmapImage();
                zoneBitmapImage.CreateOptions = BitmapCreateOptions.None; // force bitmap load right now
                
                zoneBitmapImage.BeginInit();
                zoneBitmapImage.UriSource = uriSource;
                zoneBitmapImage.EndInit();

                var writeableZoneBitmapImage = new WriteableBitmap(zoneBitmapImage);
                zoneImage.Source = writeableZoneBitmapImage;

                // Add the image to the struct 
                newZone.zoneImage = zoneImage;

                // Grab these after the image has loaded
                newZone.originalWidth = zoneBitmapImage.PixelWidth;
                newZone.originalHeight = zoneBitmapImage.PixelHeight;

                // Scale and position for the 1/4 scale image
                newZone.zoneImage.Height = newZone.originalHeight * Scale;
                newZone.zoneImage.Width = newZone.originalWidth * Scale;

                Canvas.SetTop(newZone.zoneImage, newZone.offsetY * Scale);
                Canvas.SetLeft(newZone.zoneImage, newZone.offsetX * Scale);

                // Add the Zone to the dictionary
                zoneDict.Add(newZone.ID, newZone);
                canvas.Children.Add(newZone.zoneImage);
            }
        }

        // Load the adjacency matrix
        void CreateAdjacencyMatrix()
        {
            int count = zoneDict.Count;

            foreach (var zonePair in zoneDict)
            {
                zonePair.Value.adj = new bool[count];
                zonePair.Value.adjLine = new Line[count];
            }

            // Load an existing file
            if (File.Exists(string.Format("{0}/ZoneMatrix.txt", ZoneLocation)))
            {
                using (var fin = new StreamReader(string.Format("{0}/ZoneMatrix.txt", ZoneLocation)))
                {
                    for (int i = 0; i < zoneDict.Count; ++i)
                    {
                        Zone zone;
                        zoneDict.TryGetValue(i + 1, out zone);

                        string line = fin.ReadLine();
                        string[] values = line.Split(' ');

                        for (int j = 0; j < zoneDict.Count; ++j)
                        {
                            if (values[j] == "1")
                            {
                                Zone zone2;
                                zoneDict.TryGetValue(j + 1, out zone2);

                                zone.adj[j] = true;
                                zone.adjLine[j] = createLine(zone, zone2);

                                canvas.Children.Add(zone.adjLine[j]);
                            }
                            else
                            {
                                zone.adj[j] = false;
                            }
                        }
                    }
                }
            }
        }

        // Save the graph
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (var fout = new StreamWriter(string.Format("{0}/ZoneMatrix.txt", ZoneLocation)))
            {
                for (int i = 0; i < zoneDict.Count; ++i)
                {
                    Zone zone;
                    zoneDict.TryGetValue(i+1, out zone);

                    for (int j = 0; j < zoneDict.Count; ++j)
                    {
                        fout.Write("{0} ", zone.adj[j] ? "1" : "0");
                    }
                    fout.WriteLine();
                }
            }
        }


        Line createLine(Zone zone1, Zone zone2)
        {
            var line = new Line();

            line.X1 = (zone1.offsetX + zone1.centerX) * Scale;
            line.Y1 = (zone1.offsetY + zone1.centerY) * Scale;
            line.X2 = (zone2.offsetX + zone2.centerX) * Scale;
            line.Y2 = (zone2.offsetY + zone2.centerY) * Scale;
            line.Stroke = Brushes.Black;
            line.StrokeThickness = 5.0;
            return line;
        }
    }
}
