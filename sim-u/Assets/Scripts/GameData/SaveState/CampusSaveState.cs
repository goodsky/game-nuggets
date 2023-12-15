﻿using Campus;
using Campus.GridTerrain;
using System;

namespace GameData
{
    /// <summary>
    /// This isn't elegant. But it's the minimum amount of information to recreate a campus state.
    /// </summary>
    [Serializable]
    public class CampusSaveState
    {
        public TerrainSaveState Terrain { get; set; }

        public BuildingSaveState[] Buildings { get; set; }

        public bool[,] PathGrids { get; set; }

        public bool[,] RoadVertices { get; set; }

        public ParkingLotSaveState[] ParkingLots { get; set; }
    }

    [Serializable]
    public class TerrainSaveState
    {
        public int CountX { get; set; }

        public int CountY { get; set; }

        public int CountZ { get; set; }

        public int MaxDepth { get; set; }

        public int[,] VertexHeight { get; set; }

        public GridData[,] GridData { get; set; }

        public bool[,] GridAnchored { get; set; }
    }

    [Serializable]
    public class BuildingSaveState
    {
        public string BuildingDataName { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }

        public int PositionZ { get; set; }

        public BuildingRotation Rotation { get; set; }
    }

    [Serializable]
    public class ParkingLotSaveState
    {
        public int StartX { get; set; }

        public int StartZ { get; set; }

        public int EndX { get; set; }

        public int EndZ { get; set; }
    }
}
