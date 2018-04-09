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
        /// <param name="mesh">Returns the grid mesh that manages the terrain.</param>
        /// <returns>The terrain game object.</returns>
        public static GameObject GenerateTerrain(Transform parent, GridMeshArgs args, out GridMesh mesh)
        {
            var terrain = new GameObject("Campus Terrain");
            terrain.transform.SetParent(parent, false);

            var filter = terrain.AddComponent<MeshFilter>();
            var collider = terrain.AddComponent<MeshCollider>();
            var renderer = terrain.AddComponent<MeshRenderer>();
            var selectable = terrain.AddComponent<SelectableTerrain>();

            filter.mesh = new Mesh();
            filter.mesh.name = "grid-mesh";

            mesh = new GridMesh(filter.mesh, collider, renderer, args);

            selectable.Terrain = mesh;
            mesh.Selectable = selectable;

            return terrain;
        }
    }
}
