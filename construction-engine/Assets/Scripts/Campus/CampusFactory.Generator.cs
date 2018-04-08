using GridTerrain;
using UnityEngine;

namespace Campus
{
    public static partial class CampusFactory
    {
        /// <summary>
        /// Instantiates the campus terrain game object.
        /// </summary>
        /// <param name="parent">Parent of the terrain.</param>
        /// <param name="args">GridMesh arguments to generate a terrain.</param>
        /// <returns>The terrain grid mesh.</returns>
        public static GridMesh GenerateTerrain(Transform parent, GridMeshArgs args)
        {
            var terrain = new GameObject("Campus Terrain");
            terrain.transform.SetParent(parent, false);

            var filter = terrain.AddComponent<MeshFilter>();
            var collider = terrain.AddComponent<MeshCollider>();
            var renderer = terrain.AddComponent<MeshRenderer>();

            filter.mesh = new Mesh();
            filter.mesh.name = "grid-mesh";

            var gridMesh =  new GridMesh(filter.mesh, collider, renderer, args);

            var editableTerrain = terrain.AddComponent<EditableTerrain>();
            editableTerrain.Initialize(gridMesh);

            return gridMesh;
        }
    }
}
