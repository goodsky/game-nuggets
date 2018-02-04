using UnityEngine;

public class CheckerboardTerrain : MonoBehaviour
{
    private TerrainData _terrain;

    private int _width;
    private int _height;

    int _squareCountX;
    int _squareCountY;

    void Start ()
    {
        var terrainComponent = TerrainHelper.InitializeTerrain(this.gameObject);

        _terrain = terrainComponent.terrainData;

        _width = _terrain.heightmapWidth;
        _height = _terrain.heightmapHeight;

        _squareCountX = _width - 1;
        _squareCountY = _height - 1;

        for (int i = 0; i < _squareCountX / 2; ++i)
            for (int j = 0; j < _squareCountY / 2; ++j)
                SetSquareHeight(i * 2, j * 2, (i + j) % 2 == 0 ? 0.0f : 0.01f);
    }

    void OnApplicationQuit()
    {
        TerrainHelper.FlattenTerrain(_terrain);
    }

    private void SetSquareHeight(int x, int y, float height)
    {
        if (x < 0 || x > _squareCountX || y < 0 || y > _squareCountX)
            throw new System.ArgumentOutOfRangeException(string.Format("Attempted to set a height outside of range! ({0}, {1}) is outside of ({2}, {3})", x, y, _squareCountX, _squareCountY));

        var heights = new float[2, 2];
        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                heights[i, j] = height;

        _terrain.SetHeights(x, y, heights);
    }
}
