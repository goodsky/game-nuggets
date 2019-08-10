using Campus.GridTerrain;
using Common;
using GameData;
using UnityEngine;

namespace Campus
{
    public static partial class CampusFactory
    {
        /// <summary>
        /// Instantiates the campus terrain game object.
        /// </summary>
        /// <param name="parent">Parent of the terrain.</param>
        /// <param name="args">CampusData used to generate a terrain.</param>
        /// <param name="mesh">Returns the grid mesh that manages the terrain.</param>
        /// <returns>The terrain game object.</returns>
        public static GridMesh GenerateTerrain(Transform parent, CampusData args)
        {
            var gridMeshArgs = new GridMeshArgs()
            {
                GridSquareSize = Constant.GridSize,
                GridStepSize = Constant.GridStepSize,
                SubmaterialSize = Constant.SubmaterialGridSize,
                GridMaterial = args.Terrain.TerrainMaterial,
            };

            if (args.SavedData != null)
            {
                TerrainSaveState saveData = args.SavedData.Campus.Terrain;
                gridMeshArgs.CountX = saveData.CountX;
                gridMeshArgs.CountY = saveData.CountY;
                gridMeshArgs.CountZ = saveData.CountZ;
                gridMeshArgs.MaxDepth = saveData.MaxDepth;

                gridMeshArgs.VertexHeights = saveData.VertexHeight;
                gridMeshArgs.GridData = saveData.GridData;
                gridMeshArgs.GridAnchored = saveData.GridAnchored;
            }
            else
            {
                gridMeshArgs.CountX = args.Terrain.DefaultGridCountX;
                gridMeshArgs.CountY = args.Terrain.DefaultGridCountY;
                gridMeshArgs.CountZ = args.Terrain.DefaultGridCountZ;
                gridMeshArgs.MaxDepth = args.Terrain.DefaultMaxDepth;
            }

            var terrainObject = new GameObject("Campus Terrain");
            terrainObject.transform.SetParent(parent, false);

            var filter = terrainObject.AddComponent<MeshFilter>();
            var collider = terrainObject.AddComponent<MeshCollider>();
            var renderer = terrainObject.AddComponent<MeshRenderer>();
            var selectable = terrainObject.AddComponent<SelectableTerrain>();
            var gridMesh = terrainObject.AddComponent<GridMesh>();

            filter.mesh = new Mesh();
            filter.mesh.name = "grid-mesh";

            renderer.receiveShadows = true;

            selectable.Terrain = gridMesh;
            gridMesh.Selectable = selectable;

            gridMesh.InitializeGridMesh(gridMeshArgs);
            return gridMesh;
        }

        /// <summary>
        /// Instantiates a campus building.
        /// </summary>
        /// <param name="buildingData">The building data.</param>
        /// <param name="parent">The parent transform of the building.</param>
        /// <param name="worldPosition">Position of the building to generate.</param>
        /// <param name="rotation">Rotation of the building to generate.</param>
        /// <returns>The campus building.</returns>
        public static Building GenerateBuilding(
            BuildingData buildingData,
            Transform parent,
            Point3 gridPosition,
            Vector3 worldPosition,
            Quaternion rotation)
        {
            var buildingObject = GameObject.Instantiate(buildingData.Model);
            buildingObject.name = buildingData.Name;
            buildingObject.transform.SetParent(parent, false);
            buildingObject.transform.position = worldPosition;
            buildingObject.transform.rotation = rotation;

            // var collider = buildingObject.AddComponent<MeshCollider>(); // Buildings used to be "selectable" but it gets in the way of the terrain.
            // collider.sharedMesh = buildingData.Mesh;

            var building = buildingObject.AddComponent<Building>();
            building.Initialize(gridPosition, buildingData);

            return building;
        }
    }
}
