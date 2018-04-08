using Common;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Unity Behavior for terrain that can be edited.
    /// </summary>
    public class EditableTerrain : Selectable
    {
        /// <summary>Singleton Terrain in the scene.</summary>
        public static EditableTerrain Singleton { get; private set; }

        /// <summary>
        /// The Grid Terrain
        /// </summary>
        public GridMesh _terrain;

        private SafeTerrainEditor _editor;

        // Editing State  -----------
        private EditingStates _state;

        private GridCursor _cursor;
        private Point3 _gridSelection;

        private float _mouseDragStartY;
        private int _mouseDragHeightChange;

        /// <summary>
        /// Initialize the editable terrain.
        /// </summary>
        /// <param name="terrain">The world terrain.</param>
        public void Initialize(GridMesh terrain)
        {
            Singleton = this;
            OnMouseDown = Clicked;

            _terrain = terrain;
            _editor = new SafeTerrainEditor(_terrain);

            _cursor = GridCursor.Create(_terrain, Resources.Load<Material>("Terrain/cursor"));
            _cursor.Deactivate();
            _gridSelection = Point3.Null;
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

            if (_cursor != null)
            {
                _cursor.Deactivate();
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
                       _terrain.GetVertexHeight(_gridSelection.x, _gridSelection.z),
                       _terrain.GetVertexHeight(_gridSelection.x + 1, _gridSelection.z),
                       _terrain.GetVertexHeight(_gridSelection.x, _gridSelection.z + 1),
                       _terrain.GetVertexHeight(_gridSelection.x + 1, _gridSelection.z + 1));

                    if (Input.GetKey(KeyCode.LeftControl))
                    {
                        int material = _terrain.GetMaterial(_gridSelection.x, _gridSelection.z);
                        _terrain.SetMaterial(_gridSelection.x, _gridSelection.z, (material + 1) % GridMaterials.GetAll().Length);
                    }
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
            if (_terrain.Collider.Raycast(mouseRay, out hit, float.MaxValue))
            {
                var newGridSelection = _terrain.Convert.WorldToGrid(hit.point);

                if (newGridSelection != _gridSelection)
                {
                    if (!_cursor.IsActive)
                        _cursor.Activate();

                    _gridSelection = newGridSelection;
                    _cursor.Place(_gridSelection.x, _gridSelection.z);
                }
            }
            else
            {
                if (_cursor.IsActive)
                {
                    _cursor.Deactivate();
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
                    _cursor.Place(_gridSelection.x, _gridSelection.z);
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