using System;
using System.Collections.Generic;
using UnityEngine;

enum EditingStates // state machine
{
    Selecting,
    Editing
}

public class EditableTerrain : MonoBehaviour // the editable terrain
{
    public GameObject CursorPrefab;

    private GameObject _cursor;

    private TerrainData _terrain;
    private TerrainCollider _collider;

    int _gridSizeX;
    int _gridSizeY;

    // Points in the grid that are anchored and cannot be edited
    private bool[,] _gridAnchored;

    // Editing State  -----------
    int _gridSelectedX;
    int _gridSelectedY;
    int _gridSelectedHeight;

    float _mouseDragStartY;
    int _mouseDragHeightChange;

    private EditingStates _state;

    void Start()
    {
        var terrainComponent = TerrainHelper.InitializeTerrain(this.gameObject);

        _terrain = terrainComponent.terrainData;
        _collider = GetComponent<TerrainCollider>();

        _gridSizeX = _terrain.heightmapWidth - 1;
        _gridSizeY = _terrain.heightmapHeight - 1;

        _state = EditingStates.Selecting;
        _cursor = Instantiate(CursorPrefab);

        var width = _terrain.heightmapWidth;
        var height = _terrain.heightmapHeight;
        _gridAnchored = new bool[width, height];

        // Anchor all corners, otherwise start unanchored
        for (int i = 0; i < width; ++i)
            for (int j = 0; j < height; ++j)
                _gridAnchored[i, j] = (i == 0 || j == 0 || i == width - 1 || j == height - 1) ? true : false;
    }

    void Update()
    {
        if (_state == EditingStates.Selecting)
        {
            UpdateCursorSelection();
        }
        else if (_state == EditingStates.Editing)
        {
            UpdateTerrainHeight();
        }
    }

    void UpdateCursorSelection()
    {
        if (_cursor.activeSelf && Input.GetMouseButton(0))
        {
            _state = EditingStates.Editing;
            _mouseDragStartY = Input.mousePosition.y;
            _mouseDragHeightChange = 0;

            return;
        }

        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;
        if (_collider.Raycast(mouseRay, out hit, float.MaxValue))
        {
            var newGridSelectedX = TerrainHelper.WorldToGrid(hit.point.x);
            var newGridSelectedY = TerrainHelper.WorldToGrid(hit.point.z);

            if (newGridSelectedX != _gridSelectedX || newGridSelectedY != _gridSelectedY)
            {
                if (!_cursor.activeSelf)
                    _cursor.SetActive(true);

                _gridSelectedX = newGridSelectedX;
                _gridSelectedY = newGridSelectedY;

                var selectedHeights = _terrain.GetHeights(_gridSelectedX, _gridSelectedY, 2, 2);

                // average of 4 points in the square snapped to a grid height
                // note: added a small number to  avoid inprecision caused by truncation
                var averageHeight = (selectedHeights[0, 0] + selectedHeights[0, 1] + selectedHeights[1, 0] + selectedHeights[1, 1]) / 4.0f + 0.0001f;
                _gridSelectedHeight = TerrainHelper.HeightmapToGridHeight(averageHeight);

                _cursor.transform.position = new Vector3(
                    TerrainHelper.GridToWorld(_gridSelectedX),
                    TerrainHelper.GridHeightToWorld(_gridSelectedHeight),
                    TerrainHelper.GridToWorld(_gridSelectedY));
            }
        }
        else
        {
            if (_cursor.activeSelf)
            {
                _cursor.SetActive(false);
                _gridSelectedX = -1;
                _gridSelectedY = -1;
            }
        }
    }

    void UpdateTerrainHeight()
    {
        if (!Input.GetMouseButton(0))
        {
            _state = EditingStates.Selecting;
            return;
        }

        int newHeightChange = (int)((Input.mousePosition.y - _mouseDragStartY) / 25.0f);
        if (newHeightChange != _mouseDragHeightChange)
        {
            _mouseDragHeightChange = newHeightChange;

            var newHeight = TerrainHelper.GridHeightToHeightmap(_gridSelectedHeight + _mouseDragHeightChange);
            SetGridHeight(_gridSelectedX, _gridSelectedY, newHeight);

            _cursor.transform.position = new Vector3(
                    TerrainHelper.GridToWorld(_gridSelectedX),
                    TerrainHelper.GridHeightToWorld(_gridSelectedHeight + _mouseDragHeightChange),
                    TerrainHelper.GridToWorld(_gridSelectedY));
        }
    }

    private void SafeSetGridHeight(int x, int y, float heightmap)
    {
        var s = new Stack<Point>();
        var visited = new HashSet<Point>();

    }

    private void SetGridHeight(int x, int y, float heightmap)
    {
        if (x < 0 || x > _gridSizeX || y < 0 || y > _gridSizeX)
            throw new System.ArgumentOutOfRangeException(string.Format("Attempted to set a height outside of range! ({0}, {1}) is outside of ({2}, {3})", x, y, _gridSizeX, _gridSizeY));

        var heights = new float[2, 2];
        for (int i = 0; i < 2; ++i)
            for (int j = 0; j < 2; ++j)
                heights[i, j] = heightmap;

        _terrain.SetHeights(x, y, heights);
    }

    void OnApplicationQuit()
    {
        TerrainHelper.FlattenTerrain(_terrain);
    }
}
