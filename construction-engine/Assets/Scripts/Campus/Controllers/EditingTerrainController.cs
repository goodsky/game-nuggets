using Common;
using GridTerrain;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the SelectingTerrain game state.
    /// </summary>
    public class EditingTerrainController : GameStateMachine.Controller
    {
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
            _terrain = terrain;
            _cursor = GridCursor.Create(terrain, Resources.Load<Material>("Terrain/cursor_terrain2"));
            _cursor.Deactivate();
        }

        /// <summary>
        /// Transition to SelectingTerrain state.
        /// </summary>
        /// <param name="context">The grid coordinate to edit.</param>
        public override void TransitionIn(object context)
        {
            var args = context as TerrainClickedArgs;
            if (args == null)
                GameLogger.FatalError("EditingTerrainController was given incorrect context.");

            _editingGridLocation = args.ClickLocation;

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

            int newHeightChange = (int)((Input.mousePosition.y - _mouseDragStartY) / 10.0f);
            if (newHeightChange != _mouseDragHeightChange)
            {
                _mouseDragHeightChange = newHeightChange;

                var gridHeight = Utils.Clamp(_editingGridLocation.y + _mouseDragHeightChange, 0, _terrain.CountY);

                if (_terrain.Editor.SafeSetHeight(_editingGridLocation.x, _editingGridLocation.z, gridHeight))
                {
                    _cursor.Place(_editingGridLocation.x, _editingGridLocation.z);
                }
            }
        }
    }
}
