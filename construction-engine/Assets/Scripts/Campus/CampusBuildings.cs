using GameData;
using GridTerrain;
using System.Collections.Generic;
using UnityEngine;

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

        public void Build(BuildingData building, Point3 location)
        {
            _buildings.Add(building);
            CampusFactory.GenerateBuilding(
                        building,
                        Game.Campus.transform,
                        _terrain.Convert.GridToWorld(location) + new Vector3(0f, 0.01f, 0f) /* Place just above the grass*/,
                        Quaternion.identity);

            _terrain.Editor.SetTakenGrid(location.x, location.z, building.Footprint);
        }
    }
}
