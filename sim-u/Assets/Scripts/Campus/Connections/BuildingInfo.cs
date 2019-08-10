using Common;

namespace Campus
{
    public class BuildingInfo : PathDestination
    {
        public Point3 GridPosition { get; set; }

        public bool[,] Footprint { get; set; }

        public int ClassroomCount { get; set; }

        public int LaboratoryCount { get; set; }
    }
}
