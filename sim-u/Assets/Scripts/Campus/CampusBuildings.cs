using Campus.GridTerrain;
using GameData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Campus
{
    /// <summary>
    /// Collection of all buildings on campus.
    /// </summary>
    public class CampusBuildings
    {
        private GridMesh _terrain;
        private List<BuildingData> _buildings = new List<BuildingData>();

        public CampusBuildings(GridMesh terrain)
        {
            _terrain = terrain;
        }

        /// <summary>
        /// Build a building at the location.
        /// Registers the taken grid location with the Safe Terrain Editor.
        /// </summary>
        /// <param name="building">The building to construct.</param>
        /// <param name="location">The location of the building.</param>
        public void Build(BuildingData building, Point3 location)
        {
            if (Application.isEditor)
            {
                var footprint = building.Footprint;
                var validTerrain = _terrain.Editor.CheckFlatAndFree(location.x, location.z, footprint.GetLength(0), footprint.GetLength(1));
                for (int x = 0; x < footprint.GetLength(0); ++x)
                    for (int z = 0; z < footprint.GetLength(1); ++z)
                        Assert.IsFalse(footprint[x, z] && !validTerrain[x, z], string.Format("Placing building {0} at an invalid location!!!", building.Name));
            }

            _buildings.Add(building);
            CampusFactory.GenerateBuilding(
                        building,
                        Game.Campus.transform,
                        _terrain.Convert.GridToWorld(location) + new Vector3(0f, 0.01f, 0f) /* Place just above the grass*/,
                        Quaternion.identity);

            _terrain.Editor.SetAnchoredGrid(location.x, location.z, building.Footprint);
        }
    }
}
