using Common;

namespace Campus
{
    public class BuildingInfo : PathDestination
    {
        public Point3 GridPosition { get; set; }

        public bool[,] Footprint { get; set; }

        public int SmallClassroomCount { get; set; }

        public int MediumClassroomCount { get; set; }

        public int LargeClassroomCount { get; set; }

        public int LaboratoryCount { get; set; }
    }
}
