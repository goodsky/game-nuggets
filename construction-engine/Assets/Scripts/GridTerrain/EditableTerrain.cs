using Common;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Unity Behavior for terrain that can be edited.
    /// </summary>
    public class EditableTerrain : Selectable
    {
        /// <summary>Object to place at the cursor's position.</summary>
        public GameObject CursorPrefab;

        /// <summary>Size of each grid square in world units.</summary>
        public float GridSize = 1.0f;

        /// <summary>Size of each vertical grid square in world units.</summary>
        public float GridHeight = 0.5f;

        /// <summary>Count of grid tiles you can dig down from the start.</summary>
        public int UndergroundGridCount = 5;

        private GameObject _cursor;

        private GridTerrainData _terrain;
        private SafeTerrainEditor _editor;
        private TerrainCollider _collider;

        // Editing State  -----------
        private EditingStates _state;

        Point3 _gridSelection;

        float _mouseDragStartY;
        int _mouseDragHeightChange;

        /// <summary>
        /// Unity Start method
        /// </summary>
        protected override void Start()
        {
            base.Start();

            OnMouseDown = Clicked;

            var terrainComponent = GetComponent<Terrain>();

            _terrain = new GridTerrainData(
                terrainComponent, 
                new GridTerrainArgs()
                {
                    GridSize = GridSize,
                    GridHeightSize = GridHeight,
                    UndergroundGridCount = UndergroundGridCount
                });

            _editor = new SafeTerrainEditor(_terrain);

            _collider = GetComponent<TerrainCollider>();

            _state = EditingStates.Selecting;
            _gridSelection = Point3.Null;

            _cursor = Instantiate(CursorPrefab);
        }

        /// <summary>
        /// Unity Update method
        /// </summary>
        protected override void Update()
        {
            base.Update();

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
        /// Called when the terrain is clicked.
        /// </summary>
        /// <param name="mouse"></param>
        private void Clicked(MouseButton mouse)
        {
            if (mouse == MouseButton.Left)
            {
                _state = EditingStates.Editing;
                _mouseDragStartY = Input.mousePosition.y;
                _mouseDragHeightChange = 0;
            }
            else if (mouse == MouseButton.Right)
            {
                GameLogger.Info("Selected ({0}); Point Heights ({1}, {2}, {3}, {4})",
                   _gridSelection,
                   _terrain.GetPointHeight(_gridSelection.x, _gridSelection.z),
                   _terrain.GetPointHeight(_gridSelection.x + 1, _gridSelection.z),
                   _terrain.GetPointHeight(_gridSelection.x, _gridSelection.z + 1),
                   _terrain.GetPointHeight(_gridSelection.x + 1, _gridSelection.z + 1));
            }
        }

        /// <summary>
        /// Move the terrain editing cursor.
        /// </summary>
        private void UpdateCursorSelection()
        {
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

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
        private void UpdateTerrainHeight()
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

                if (_editor.SafeSetHeight(_gridSelection.x, _gridSelection.z, gridHeight))
                {
                    _cursor.transform.position = _terrain.ConvertGridCenterToWorld(
                        new Point3(
                            _gridSelection.x,
                            gridHeight,
                            _gridSelection.z));
                }
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