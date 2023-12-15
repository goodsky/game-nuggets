﻿using Common;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Campus
{
    /// <summary>
    /// Runtime calculation of a mesh's grid footprint.
    /// You can't have a hole in the middle of a square of your building mesh.
    /// That'd be silly.
    /// </summary>
    public class FootprintCreator : MonoBehaviour, IDisposable
    {
        private MeshCollider _meshCollider;

        /// <summary>
        /// Calculate the grid footprint of the mesh.
        /// </summary>
        /// <param name="model">The model with a mesh.</param>
        /// <param name="gridSize">The size of the grid squares. The footprint will be returned in this size.</param>
        /// <returns>The footprint of the mesh in a [X, Z] array.</returns>
        public bool[,] CalculateFootprint(GameObject model, float gridSize)
        {
            if (_meshCollider == null)
            {
                _meshCollider = gameObject.AddComponent<MeshCollider>();
                transform.position = new Vector3(0, 0, 0);
            }

            MeshFilter meshFilter = model.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                GameLogger.FatalError("Model did not have a mesh filter!? Name = '{0}'", model.name);
                return new bool[0, 0];
            }

            Mesh mesh = meshFilter.sharedMesh;
            _meshCollider.sharedMesh = mesh;

            // FYI: FBX need to be exported with Z-forward Y-up to make sense in our world.
            var bounds = mesh.bounds;
            //Assert.AreApproximatelyEqual(0.0f, bounds.size.x % gridSize, string.Format("Building Mesh '{0}' does not fit prettily into a {1}x{1} grid. x-size: {2}", _meshCollider.sharedMesh.name, gridSize, bounds.size.x));
            //Assert.AreApproximatelyEqual(0.0f, bounds.size.z % gridSize, string.Format("Building Mesh '{0}' does not fit prettily into a {1}x{1} grid. z-size: {2}", _meshCollider.sharedMesh.name, gridSize, bounds.size.z));

            int gridSizeX = Mathf.RoundToInt(bounds.size.x / gridSize);
            int gridSizeZ = Mathf.RoundToInt(bounds.size.z / gridSize);
            var footprint = new bool[gridSizeX, gridSizeZ];

            float height = 50f;
            float castDistance = height * 2;
            for (int x = 0; x < gridSizeX; ++x)
            {
                for (int z = 0; z < gridSizeZ; ++z)
                {
                    var ray = new Ray(new Vector3(
                        x * gridSize + gridSize / 2, 
                        height,
                        z * gridSize + gridSize / 2), 
                        Vector3.down);

                    // NB: If this starts returning false on a model, make sure the "Generate Colliders" bool is set to true on the model's Unity import.
                    RaycastHit hit;
                    bool isSolid = _meshCollider.Raycast(ray, out hit, castDistance);
                    footprint[x, z] = isSolid;
                }
            }

            return footprint;
        }

        /// <summary>
        /// Dispose the FootprintCreator game object.
        /// </summary>
        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}
