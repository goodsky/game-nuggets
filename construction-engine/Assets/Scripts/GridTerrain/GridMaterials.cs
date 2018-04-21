using System.Collections.Generic;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Enum representing the 
    /// </summary>
    public enum GridMaterialType
    {
        GridGrass,
        GridPath
    }

    public static class GridMaterials
    {
        // Names of the material files on disk.
        private static readonly string[] s_materialNames = new string[]
        {
            "Terrain/grid-grass",
            "Terrain/grid-path"
        };

        // Mapping of material types to index in the materials array. -1 if not loaded.
        private static readonly Dictionary<GridMaterialType, int> s_materialMapping = new Dictionary<GridMaterialType, int>();

        // Array of loaded materials.
        private static Material[] s_allMaterials;

        /// <summary>
        /// Load all known grid materials at static constructor time.
        /// </summary>
        static GridMaterials()
        {
            var materials = new List<Material>();
            for (int i = 0; i < s_materialNames.Length; ++i)
            {
                GridMaterialType type = (GridMaterialType)i;
                var material = Resources.Load<Material>(s_materialNames[i]);
                if (material != null)
                {
                    s_materialMapping[type] = materials.Count;
                    materials.Add(material);
                }
                else
                {
                    s_materialMapping[type] = -1;
                }
            }

            s_allMaterials = materials.ToArray();
        }

        /// <summary>
        /// Get all loaded grid materials.
        /// </summary>
        /// <returns></returns>
        public static Material[] GetAll()
        {
            return s_allMaterials;
        }

        /// <summary>
        /// Get the id of a specific material.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetMaterialId(GridMaterialType type)
        {
            return s_materialMapping[type];
        }
    }
}
