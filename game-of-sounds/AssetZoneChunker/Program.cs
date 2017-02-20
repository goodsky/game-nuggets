using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AssetZoneChunker
{
    class Program
    {
        // This is a tool used to auto-crop zone images for the Game Of Sounds
        // Filenames that start with '_' are ignored
        // The File Name is expected to be in this format <Zone Name>-<ID>.png
        static void Main(string[] args)
        {
            string InputFolder = args[0];
            string OutputFolder = args[1];

            StreamWriter log = new StreamWriter(OutputFolder + "/log.txt");

            StreamWriter fout = new StreamWriter(OutputFolder + "/ZoneList.txt");
            fout.WriteLine("// This is the Zone List metadata file: <ID> <Zone Name> <Zone Image Filename> <X> <Y> <Center X> <Center Y>");
            fout.WriteLine("// This file was created automatically by the AssetZoneChunker program. Try not to edit it manually.");

            int numZones = 0;

            var files = Directory.GetFiles(InputFolder);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);

                if (fileInfo.Name.StartsWith("_"))
                {
                    log.WriteLine("SKIPPING: {0}", file);
                    continue;
                }

                if (!fileInfo.Name.EndsWith(".png"))
                {
                    log.WriteLine("SKIPPING: {0}", file);
                    continue;
                }

                // Create all the names and ID's and stuff
                string name = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4); // remove .png from filename
                var nameSplit = name.Split('-');

                string ZoneName = nameSplit[0].Replace(' ', '_');
                string ZoneFileName = ZoneName + ".png";
                string ZoneId = nameSplit[1];

                Console.WriteLine("Working on {0}", ZoneName);

                log.WriteLine("Found zone: {0}", file);
                log.WriteLine("\t{0} - {1}", ZoneName, ZoneId);

                Image image = Image.FromFile(file);
                Thread.Sleep(500);
                Bitmap bitmap = new Bitmap(image);

                // Find the Boarders and the center location
                int minX = image.Width;
                int maxX = 0;
                int minY = image.Height;
                int maxY = 0;

                int centerX = -1;
                int centerY = -1;

                for (int x = 0; x < image.Width; ++x)
                {
                    for (int y = 0; y < image.Height; ++y)
                    {
                        if (bitmap.GetPixel(x, y).A != 0)
                        {
                            // The black pixel is the one that marks the center
                            // All the white pixels are the body
                            var p = bitmap.GetPixel(x, y);
                            if (p.R == 0 && p.G == 0 && p.B == 0)
                            {
                                centerX = x;
                                centerY = y;
                            }

                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        }
                    }
                }

                log.WriteLine("\tminx: {0} maxX: {1} minY: {2} maxY: {3}", minX, maxX, minY, maxY);
                log.WriteLine("\tcenterX: {0} centerY: {1}", centerX, centerY);
                if (centerX == -1 && centerY == -1)
                {
                    log.WriteLine("\tERROR*** Center not set.");
                    continue;
                }

                // Crop the image and save the result
                Rectangle boundingBox = new Rectangle(minX, minY, (maxX - minX), (maxY - minY));
                var croppedBitmap = bitmap.Clone(boundingBox, bitmap.PixelFormat);
                croppedBitmap.Save(OutputFolder + "/" + ZoneFileName);

                // oopsie, memory leak
                croppedBitmap.Dispose();
                bitmap.Dispose();
                image.Dispose();

                // Write the Metadata file
                fout.WriteLine("{0} {1} {2} {3} {4} {5} {6}", ZoneId, ZoneName, ZoneFileName, minX, minY, (centerX - minX), (centerY - minY));

                ++numZones;
            }

            log.WriteLine("Chunking Complete. Completed {0} Zones.", numZones);

            log.Close();
            fout.Close();
        }
    }
}
