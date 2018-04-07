using Common;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Unity Behavior for terrain that can be edited.
    /// </summary>
    public class EditableTerrain : Selectable
    {
        public static EditableTerrain Singleton { get; private set; }

        /// <summary>Object to place at the cursor's position.</summary>
        public GameObject CursorPrefab;

        /// <summary>Size of each grid square in world units.</summary>
        public float GridSize = 1.0f;

        /// <summary>Size of each vertical grid square in world units.</summary>
        public float GridHeight = 0.5f;

        /// <summary>Count of grid tiles you can dig down from the start.</summary>
        public int UndergroundGridCount = 5;

        /// <summary>Should the terrain be editable at start.</summary>
        public bool EditingAtStart = false;

        private GameObject _cursor;

        private IGridTerrain _terrain;
        private SafeTerrainEditor _editor;

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

            Singleton = this;
            OnMouseDown = Clicked;

            var terrainComponent = GetComponent<Terrain>();
            if (terrainComponent != null)
            {
                _terrain = new GridTerrainData(
                    terrainComponent,
                    new GridTerrainArgs()
                    {
                        GridSize = GridSize,
                        GridHeightSize = GridHeight,
                        UndergroundGridCount = UndergroundGridCount
                    });
            }

            var customTerrainComponent2 = GetComponent<GridTerrainData2>();
            if (customTerrainComponent2 != null)
            {
                _terrain = customTerrainComponent2;
            }

            var customTerrainComponent3 = GetComponent<GridTerrainData3>();
            if (customTerrainComponent3 != null)
            {
                _terrain = customTerrainComponent3;
            }

            _editor = new SafeTerrainEditor(_terrain);

            _cursor = Instantiate(CursorPrefab);

            _state = EditingAtStart ? EditingStates.Editing : EditingStates.None;
            _gridSelection = Point3.Null;
            _cursor.SetActive(false);
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
        /// Start editing the terrain.
        /// </summary>
        public void StartEditing()
        {
            _state = EditingStates.Editing;
        }

        /// <summary>
        /// Stop editing the terrain.
        /// </summary>
        public void StopEditing()
        {
            _state = EditingStates.None;
            _gridSelection = Point3.Null;

            if (_cursor.gameObject != null)
            {
                _cursor.SetActive(false);
            }
        }

        /// <summary>
        /// Called when the terrain is clicked.
        /// </summary>
        /// <param name="mouse"></param>
        private void Clicked(MouseButton mouse)
        {
            if (_state == EditingStates.Selecting)
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
        }

        /// <summary>
        /// Move the terrain editing cursor.
        /// </summary>
        private void UpdateCursorSelection()
        {
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (_terrain.Raycast(mouseRay, out hit, float.MaxValue))
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

                var gridHeight = Utils.Clamp(_gridSelection.y + _mouseDragHeightChange, 0, _terrain.CountY);

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
            None,
            Selecting,
            Editing
        }
    }
}