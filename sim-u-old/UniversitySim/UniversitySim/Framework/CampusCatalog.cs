using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.Utilities;

namespace UniversitySim.Framework
{
    /// <summary>
    /// Load the configuration files for information about buildings
    /// (note: in my world trees and sidewalks are buildings. They are built aren't they?)
    /// </summary>
    class CampusCatalog
    {
        // Catalog of buildings.
        // Loaded from Buildings.ini
        private Dictionary<string, Data> catalog;

        /// <summary>
        /// Create the campus catalog
        /// </summary>
        public CampusCatalog()
        {
            this.catalog = new Dictionary<string, Data>();

            Config config = new Config(@"Configs\Catalog.ini");
            foreach (var building in config.SectionNames())
            {
                try
                {
                    if (building.Equals(Constants.DEFAULT))
                    {
                        continue;
                    }

                    string typeString = config.GetStringValue(building, "type", null);
                    if (typeString == null)
                    {
                        Logger.Log(LogLevel.Error, "LoadingCatalog", string.Format("Failed to load {0} from Catalog.ini because it did not have a type defined.", building));
                    }

                    Data data = null;
                    DataType type = (DataType)Enum.Parse(typeof(DataType), typeString);

                    if (type == DataType.Building)
                    { 
                        data = this.LoadBuilding(config, building);
                    }
                    else if (type == DataType.Path)
                    {
                        data = this.LoadPath(config, building);
                    }

                    if (data != null)
                    {
                        Logger.Log(LogLevel.Info, "LoadingCatalog", string.Format("Loaded Element {0}- {1}", building, data));
                        this.catalog.Add(data.Key, data);
                    }
                    else
                    {
                        Logger.Log(LogLevel.Error, "LoadingCatalog", string.Format("null data element {0}", building));
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(LogLevel.Error, "LoadingCatalog", string.Format("Error loading building {0}. Exception: {1}", building, e));
                }
            }
        }

        /// <summary>
        /// Load a building from Config
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private BuildingData LoadBuilding(Config config, string key)
        {
            // Create the building data
            BuildingData data = new BuildingData();
            data.Key = key;
            data.Name = config.GetStringValue(key, "title", "");
            data.Type = DataType.Building;
            data.InToolbox = config.GetBoolValue(key, "toolbox", false);
            data.ImageName = config.GetStringValue(key, "image", "");
            data.IconImageName = config.GetStringValue(key, "icon", null);
            data.FootprintImageName = config.GetStringValue(key, "footprint", "");
            data.ImageOffsetY = config.GetIntValue(key, "imageOffsetY", 0);
            data.ImageOffsetX = config.GetIntValue(key, "imageOffsetX", 0);
            data.FootprintOffsetY = config.GetIntValue(key, "footprintOffsetY", 0);
            data.FootprintOffsetX = config.GetIntValue(key, "footprintOffsetX", 0);
            data.Cost = config.GetIntValue(key, "cost", 0);
            data.Specs = new Dictionary<BuildingStat, int>();

            // Load the specs into an enum and int pair
            var specsString = config.GetStringValue(key, "specs", "");
            if (!string.IsNullOrEmpty(specsString))
            {
                var specs = specsString.Split(';');
                foreach (var spec in specs)
                {
                    var nameValue = spec.Split(',');
                    if (nameValue.Length != 2)
                    {
                        string msg = string.Format("Invalid Catalog.ini file. Element {0} ({1}) did not have a name and value.", key, spec);
                        Logger.Log(LogLevel.Error, "LoadingCatalog", msg);
                        throw new Exception(msg);
                    }

                    try
                    {
                        BuildingStat stat = (BuildingStat)Enum.Parse(typeof(BuildingStat), nameValue[0]);
                        int value = Int32.Parse(nameValue[1]);
                        data.Specs.Add(stat, value);
                    }
                    catch (Exception e)
                    {
                        var msg = string.Format("Invalid Catalog.ini file. Element {0} ({1}) was an unrecognized BuildingStat-Int pair. ex: {2}", e.Message);
                        Logger.Log(LogLevel.Error, "LoadingCatalog", msg);
                        throw new Exception(msg);
                    }
                }
            }

            return data;
        }

        /// <summary>
        /// Load a path from Config
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        private PathData LoadPath(Config config, string key)
        {
            // Create the building data
            PathData data = new PathData();
            data.Key = key;
            data.Name = config.GetStringValue(key, "title", "");
            data.Type = DataType.Path;
            data.InToolbox = config.GetBoolValue(key, "toolbox", false);
            data.ImageName = config.GetStringValue(key, "image", "");
            data.IconImageName = config.GetStringValue(key, "icon", null);
            data.Cost = config.GetIntValue(key, "cost", 0);

            return data;
        }

        /// <summary>
        /// Load all of the image content for the buildings
        /// </summary>
        /// <param name="contentMan">XNA Content manager</param>
        public void LoadContent(ContentManager contentMan)
        {
            foreach (var element in this.catalog)
            {
                Texture2D image = contentMan.Load<Texture2D>(element.Value.ImageName);

                int size = image.Width * image.Height;
                element.Value.Image = image;

                Texture2D iconimage = contentMan.Load<Texture2D>(element.Value.IconImageName);
                element.Value.IconImage = iconimage;

                if (element.Value is BuildingData)
                {
                    BuildingData buildingData = element.Value as BuildingData;

                    Color[] imageData = new Color[size];
                    image.GetData<Color>(imageData, 0, size);
                    buildingData.ImageData = imageData;

                    Texture2D footimage = contentMan.Load<Texture2D>(buildingData.FootprintImageName);
                    buildingData.FootprintImage = footimage;

                    size = footimage.Width * footimage.Height;
                    Color[] footimageData = new Color[size];
                    footimage.GetData<Color>(footimageData, 0, size);

                    // I spent waaay too much time thinking about this, so it's entirely possible that I'm overlooking a simpler solution.
                    // I'm looping over the footprint image mask to determine which squares each building takes up. 
                    // You can specify an offset to the center of the building mask in buildings.ini (just like you can for the center of the image; note: these two centers should be the same)
                    // Using that center value, we find if the center is on a type0 (first grid piece lines up with the left side) or a type1 (first grid piece is one half width from the left side)
                    bool isType0 = buildingData.FootprintOffsetX % Constants.TILE_WIDTH == 0;
                    int worldYOffset = ((buildingData.FootprintOffsetX % Constants.TILE_WIDTH == 0) ^ (buildingData.FootprintOffsetY % Constants.TILE_HEIGHT == 0)) ? Constants.TILE_HEIGHT / 2 : 0;

                    int minFootprintX = (-footimage.Width + Constants.TILE_WIDTH / 2) / Constants.TILE_WIDTH;
                    int maxFootprintX = (footimage.Height - worldYOffset - Constants.TILE_HEIGHT / 2) / Constants.TILE_HEIGHT;
                    // int minFootprintY = 0;
                    int maxFootprintY = maxFootprintX - minFootprintX + 1;

                    buildingData.Footprint = new bool[maxFootprintY][];
                    for (int i = 0; i < maxFootprintY; ++i)
                        buildingData.Footprint[i] = new bool[maxFootprintY];

                    int startCenterX = buildingData.FootprintOffsetX + Constants.TILE_WIDTH / 2;
                    int startCenterY = -1 * buildingData.FootprintOffsetY + Constants.TILE_HEIGHT / 2 - worldYOffset;
                    var isometricCenterIndex = Geometry.ToIsometricGrid(new Pair<int>(startCenterX, startCenterY));
                    buildingData.FootprintIndexOffsetX = isometricCenterIndex.x - minFootprintX;
                    buildingData.FootprintIndexOffsetY = isometricCenterIndex.y;

                    for (int y = 0; y <= footimage.Height - Constants.TILE_HEIGHT; y += Constants.TILE_HEIGHT / 2)
                    {
                        int xStart = (isType0 ^ ((int)Math.Abs(y - buildingData.FootprintOffsetY) % Constants.TILE_HEIGHT == 0)) ? Constants.TILE_WIDTH / 2 : 0;
                        for (int x = xStart; x <= footimage.Width - Constants.TILE_WIDTH; x += Constants.TILE_WIDTH)
                        {
                            int qX = x + Constants.TILE_WIDTH / 2;
                            int qY = y + Constants.TILE_HEIGHT / 2 - worldYOffset;
                            var isometricQueryPosition = Geometry.ToIsometricGrid(new Pair<int>(qX, qY));
                            buildingData.Footprint[isometricQueryPosition.y][isometricQueryPosition.x - minFootprintX] = footimageData[qX + qY * footimage.Width] != Color.Transparent;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get an enumerable list of buildings
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Data> GetCatalog()
        {
            return this.catalog.Values;
        }

        public Data GetData(string key)
        {
            Data data = null;
            this.catalog.TryGetValue(key, out data);

            return data;
        }
    }

    /// <summary>
    /// Defines the different types of buildings you can build from the toolbox
    /// </summary>
    public enum DataType
    {
        Building,
        Path
    }

    /// <summary>
    /// Data base class.
    /// All Campus Elements will have data that inherits from this class.
    /// </summary>
    public abstract class Data
    {
        // Key of the catalog data entry in the config
        public string Key;

        // Name of the catalog data entry in the config
        public string Name;

        // Type of the catalog data entry in the config
        public DataType Type;

        // True if this data is something that you can build from the toolbox
        // This will change later to allow for unlocking of new things
        public bool InToolbox;

        // Cost to build this element. 
        // TODO: This value might have to have a sell value too the changes with age
        //       Or a "demolition" cost... there needs to be more details here
        public int Cost;

        // Name of the image file to represent this element on the screen
        public string ImageName;

        // Image icon to show in the building toolbox
        public string IconImageName;

        // Image file in a renderable format
        public Texture2D Image;

        // Image for the icon rendering in the toolbox
        public Texture2D IconImage;
    }

    /// <summary>
    /// Information about a building 
    /// </summary>
    public class BuildingData : Data
    {
        // Name of the image file representing the footprint of the building
        public string FootprintImageName;

        // Data of the image file in a queryable format
        public Color[] ImageData;

        // Footprint image for the building
        public Texture2D FootprintImage;

        // Data of the footprint image for the building boiled into a boolean array. aka Footprint mask.
        public bool[][] Footprint;

        // Dictionary of the stats of this building
        public Dictionary<BuildingStat, int> Specs;

        // Offset of the image when rendering in the Y axis
        public int ImageOffsetY;

        // Offset of the image when rendering in the X axis
        public int ImageOffsetX;

        // Offset of the footprint image to the center on the Y axis
        public int FootprintOffsetY;

        // Offset of the footprint image to the center on the X axis
        public int FootprintOffsetX;

        // Offset to the center of the Footprint image mask
        public int FootprintIndexOffsetX;

        // Offset to the center of the Footprint image mask
        public int FootprintIndexOffsetY;

        /// <summary>
        /// Print out all my information
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Building {0}", this.Name);
        }
    }

    /// <summary>
    /// Information about a Path 
    /// </summary>
    public class PathData : Data
    {
        /// <summary>
        /// Print out all my information
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}
