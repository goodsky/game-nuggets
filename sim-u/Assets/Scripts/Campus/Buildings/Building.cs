using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    public class Building : MonoBehaviour
    {
        public Point3 GridPosition { get; private set; }
        public BuildingData Data { get; private set; }

        public void Initialize(Point3 gridPosition, BuildingData buildingData)
        {
            GridPosition = gridPosition;
            Data = buildingData;
        }

        public BuildingInfo ToBuildingInfo()
        {
            return new BuildingInfo()
            {
                Id = GridPosition.GetHashCode(),
                GridPosition = GridPosition,
                Footprint = Data.Footprint,
                SmallClassroomCount = Data.SmallClassrooms,
                MediumClassroomCount = Data.MediumClassrooms,
                LargeClassroomCount = Data.LargeClassrooms,
                LaboratoryCount = Data.Laboratories,
                IsConnectedToPaths = null,
            };
        }
    }
}
