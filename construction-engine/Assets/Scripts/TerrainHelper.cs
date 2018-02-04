using UnityEngine;
using UnityEngine.Assertions;

static class TerrainHelper
{
    // I want each set of 4 pixels to correspond to a 10x10 world unit square.
    private static readonly float GridSize = 10.0f;
    private static readonly float HalfGridSize = GridSize / 2.0f;

    // I am prescribing a lot on the terrain height today
    // Specific expectations to make sure the editing and precision works out
    private static readonly float ExpectedHeight = 64.0f;

    // Minimum height and Maximum Height the terrain can get to
    private static readonly float MinHeight = -16.0f;
    private static readonly float MaxHeight = 48.0f;

    // The starting heightmap value (between 0.0 - 1.0)
    private static readonly float StartHeightMap = -MinHeight / (MaxHeight - MinHeight);

    // The number of world units per height step
    private static readonly float GridHeightStep = 4.0f;

    // The number of discrete heightmap steps between max and min
    // private static readonly float HeightmapCount = 16.0f; // ExpectedHeight / GridHeightStep;

    // The heightmap value between steps
    private static readonly float HeightmapStep = 0.0625f; // 1.0f / HeightmapCount;

    public static Terrain InitializeTerrain(GameObject obj)
    {
        var terrain = obj.GetComponent<Terrain>();

        Assert.IsNotNull(terrain, "Checkerboard Terrain was put on a non-terrain object");

        var data = terrain.terrainData;
        var scale = data.heightmapScale;

        Assert.AreApproximatelyEqual(scale.x, GridSize, "The X ratio of the terrain does not correspond to a 10x10 square!!! Actual Scale: " + scale.x);
        Assert.AreApproximatelyEqual(scale.z, GridSize, "The Z ratio of the terrain does not correspond to a 10x10 square!!! Actual Scale: " + scale.z);
        Assert.AreApproximatelyEqual(scale.y, ExpectedHeight, "I require a terrain height of 500 units today. Sorry!");

        // I am very particular about the y axis when it comes to my editable terrain
        // If this gains momentum I'll probably have to revisit this
        var t = obj.transform;
        t.position = new Vector3(t.position.x, MinHeight, t.position.z);

        FlattenTerrain(terrain.terrainData, StartHeightMap);

        return terrain;
    }

    public static void FlattenTerrain(TerrainData terrain, float newHeight = 0.0f)
    {
        var width = terrain.heightmapWidth;
        var height = terrain.heightmapHeight;

        float[,] resetHeights = new float[width, height];

        for (int i = 0; i < width; ++i)
            for (int j = 0; j < height; ++j)
                resetHeights[i, j] = newHeight;

        terrain.SetHeights(0, 0, resetHeights);
    }

    // Convert world X & Z coordinates to the grid integer
    public static int WorldToGrid(float point)
    {
        return (int)(point / GridSize);
    }

    // Convert grid integers to world X & Z coordinates
    public static float GridToWorld(int grid)
    {
        return grid * GridSize + HalfGridSize;
    }

    // Convert world Y coordinates into the grid height
    public static int WorldToGridHeight(float point)
    {
        return (int)((point - MinHeight) / GridHeightStep);
    }

    // Convert grid height to world Y coordinates
    public static float GridHeightToWorld(int grid)
    {
        return grid * GridHeightStep + MinHeight;
    }

    // Convert heightmap values to grid height
    public static int HeightmapToGridHeight(float heightmap)
    {
        return (int)(heightmap / HeightmapStep);
    }

    // Convert grid height to heightmap value
    public static float GridHeightToHeightmap(int grid)
    {
        return grid * HeightmapStep;
    }
}
