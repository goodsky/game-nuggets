﻿using System.Xml.Serialization;
using UnityEngine;

namespace GameData
{
    public class TerrainData
    {
        [XmlElement("TerrainMaterial")]
        public string TerrainMaterialName { get; set; }

        [XmlElement("SubmaterialSquareSize")]
        public int SubmaterialSquareSize { get; set; }

        [XmlElement("SubmaterialEmptyGrassIndex")]
        public int SubmaterialEmptyGrassIndex { get; set; }

        [XmlElement("SubmaterialPathsIndex")]
        public int SubmaterialPathsIndex { get; set; }

        [XmlElement("SubmaterialRoadsIndex")]
        public int SubmaterialRoadsIndex { get; set; }

        [ResourceLoader(ResourceType.Materials, ResourceCategory.Terrain, nameof(TerrainMaterialName))]
        public Material TerrainMaterial { get; set; }

        [ResourceLoader(ResourceType.Prefabs, ResourceCategory.Terrain, resourceName: "terrain-skirt")]
        public GameObject TerrainSkirt { get; set; }

        [XmlElement("GridCountX")]
        public int GridCountX { get; set; }

        [XmlElement("GridCountZ")]
        public int GridCountZ { get; set; }

        [XmlElement("GridCountY")]
        public int GridCountY { get; set; }

        [XmlElement("StartingHeight")]
        public int StartingHeight { get; set; }
    }
}