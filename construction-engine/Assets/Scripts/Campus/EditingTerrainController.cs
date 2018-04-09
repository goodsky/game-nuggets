using Common;
using GridTerrain;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the SelectingTerrain game state.
    /// </summary>
    public class EditingTerrainController : GameStateController
    {
        private SafeTerrainEditor _editor;
        private GridMesh _terrain;
        private GridCursor _cursor;

        private Point3 _editingGridLocation;
        private float _mouseDragStartY;
        private int _mouseDragHeightChange;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to edit.</param>
        public EditingTerrainController(GridMesh terrain)
        {
            _editor = new SafeTerrainEditor(terrain);
            _terrain = terrain;
            _cursor = GridCursor.Create(terrain, Resources.Load<Material>("Terrain/cursor"));
            _cursor.Deactivate();
        }

        /// <summary>
        /// Transition to SelectingTerrain state.
        /// </summary>
        /// <param name="context">The grid coordinate to edit.</param>
        public override void TransitionIn(object context)
        {
            if (!(context is Point3))
                GameLogger.Error("EditingTerrainController was given incorrect context.");

            _editingGridLocation = (Point3)context;

            _cursor.Activate();
            _cursor.Place(_editingGridLocation.x, _editingGridLocation.z);

            _mouseDragStartY = Input.mousePosition.y;
            _mouseDragHeightChange = 0;
        }

        /// <summary>
        /// Transition out of SelectingTerrain state.
        /// </summary>
        public override void TransitionOut()
        {
            if (_cursor != null)
            {
                _cursor.Deactivate();
            }
        }

        /// <summary>
        /// Update during the SelectingTerrain state.
        /// </summary>
        public override void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                Game.State.StopDoing();
                return;
            }

            int newHeightChange = (int)((Input.mousePosition.y - _mouseDragStartY) / 25.0f);
            if (newHeightChange != _mouseDragHeightChange)
            {
                _mouseDragHeightChange = newHeightChange;

                var gridHeight = Utils.Clamp(_editingGridLocation.y + _mouseDragHeightChange, 0, _terrain.CountY);

                if (_editor.SafeSetHeight(_editingGridLocation.x, _editingGridLocation.z, gridHeight))
                {
                    _cursor.Place(_editingGridLocation.x, _editingGridLocation.z);
                }
            }
        }
    }
}
