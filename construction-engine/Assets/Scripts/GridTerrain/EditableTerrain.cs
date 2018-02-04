using System;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Unity Behavior for terrain that can be edited.
    /// </summary>
    public class EditableTerrain : MonoBehaviour
    {
        /// <summary>Object to place at the cursor's position.</summary>
        public GameObject CursorPrefab;

        private GameObject _cursor;

        private GridTerrainData _terrain;
        private SafeTerrainEditor _editor;
        private TerrainCollider _collider;

        // Editing State  -----------
        private EditingStates _state;

        Point3 _gridSelection;

        float _mouseDragStartY;
        int _mouseDragHeightChange;

        void Start()
        {
            var terrainComponent = GetComponent<Terrain>();

            _terrain = new GridTerrainData(
                terrainComponent, 
                new GridTerrainArgs()
                {
                    GridSize = 10.0f,
                    GridHeightSize = 4.0f,
                    UndergroundGridCount = 4
                });

            _editor = new SafeTerrainEditor(_terrain);

            _collider = GetComponent<TerrainCollider>();

            _state = EditingStates.Selecting;
            _gridSelection = Point3.Null;

            _cursor = Instantiate(CursorPrefab);
        }

        /// <summary>
        /// Game Loop.
        /// </summary>
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

        /// <summary>
        /// Move the terrain editing cursor.
        /// </summary>
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
                var newGridSelection = _terrain.ConvertWorldToGrid(hit.point);

                if (newGridSelection != _gridSelection)
                {
                    if (!_cursor.activeSelf)
                        _cursor.SetActive(true);

                    _gridSelection = newGridSelection;
                    _cursor.transform.position = _terrain.ConvertGridCenterToWorld(newGridSelection);
                }
            }
            else
            {
                if (_cursor.activeSelf)
                {
                    _cursor.SetActive(false);
                    _gridSelection = Point3.Null;
                }
            }
        }

        /// <summary>
        /// Move terrain up and down.
        /// </summary>
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

                var gridHeight = Utils.Clamp(_gridSelection.y + _mouseDragHeightChange, 0, _terrain.GridCountY);

                _terrain.SetHeight(_gridSelection.x, _gridSelection.z, gridHeight);

                _cursor.transform.position = _terrain.ConvertGridCenterToWorld(
                    new Point3(
                        _gridSelection.x,
                        gridHeight,
                        _gridSelection.z));
            }
        }

        /// <summary>
        /// Flatten the terrain when you are done.
        /// </summary>
        void OnApplicationQuit()
        {
            _terrain.Flatten();
        }

        /// <summary>
        /// Editing state machine steps
        /// </summary>
        private enum EditingStates
        {
            Selecting,
            Editing
        }
    }
}