using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Unity Behaviour that lives on each building.
    /// Each building keeps track of the configuration <see cref="BuildingData"/> as well as the
    /// location and rotation used when constructing it. 
    /// </summary>
    public class Building : MonoBehaviour
    {
        public Point3 Location { get; private set; }
        public BuildingRotation Rotation { get; private set; }

        public BuildingData Data { get; private set; }

        public void Initialize(Point3 location, BuildingRotation rotation, BuildingData buildingData)
        {
            Location = location;
            Rotation = rotation;
            Data = buildingData;
        }

        /// <summary>
        /// Return the "BuildingInfo" data.
        /// It has most of the same information, but
        /// it is used by the connection manager.
        /// </summary>
        public BuildingInfo ToBuildingInfo()
        {
            (Point3 footprintOrigin, bool[,] footprint) =
                BuildingRotationUtils.RotateFootprint(Data, Location, Rotation);

            return new BuildingInfo()
            {
                Id = footprintOrigin.GetHashCode(),
                FootprintOrigin = footprintOrigin,
                Footprint = footprint,
                SmallClassroomCount = Data.SmallClassrooms,
                MediumClassroomCount = Data.MediumClassrooms,
                LargeClassroomCount = Data.LargeClassrooms,
                LaboratoryCount = Data.Laboratories,
                IsConnectedToPaths = null,
            };
        }
    }
}
