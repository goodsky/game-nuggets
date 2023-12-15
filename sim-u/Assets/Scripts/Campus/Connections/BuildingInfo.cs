using Common;

namespace Campus
{
    /// <summary>
    /// Representation of a single building. 
    /// Keeps track of the buildings real position and footprint on the terrain.
    /// As well as data that the Campus Manager and Connections Manager may need.
    /// </summary>
    public class BuildingInfo : PathDestination
    {
        public Point3 FootprintOrigin { get; set; }

        public bool[,] Footprint { get; set; }

        public Point2[] EntryPoints { get; set; }

        public int UtilitiesCostPerQuarter { get; set; }

        public int SmallClassroomCount { get; set; }

        public int MediumClassroomCount { get; set; }

        public int LargeClassroomCount { get; set; }

        public int LaboratoryCount { get; set; }
    }
}
