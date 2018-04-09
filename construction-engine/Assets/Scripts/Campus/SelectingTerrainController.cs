using Common;
using GridTerrain;
using System;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the SelectingTerrain game state.
    /// </summary>
    public class SelectingTerrainController : GameStateController
    {
        private GridMesh _terrain;
        private GridCursor _cursor;

        private Point3 _mouseLocation;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to edit.</param>
        public SelectingTerrainController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursor = GridCursor.Create(terrain, Resources.Load<Material>("Terrain/cursor"));
            _cursor.Deactivate();
        }

        /// <summary>
        /// Transition to SelectingTerrain state.
        /// </summary>
        /// <param name="context">Not used.</param>
        public override void TransitionIn(object context)
        {
            _cursor.Activate();
            _mouseLocation = Point3.Null;
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

            _mouseLocation = Point3.Null;
        }

        /// <summary>
        /// Update during the SelectingTerrain state.
        /// </summary>
        public override void Update()
        {
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            if (_terrain.Collider.Raycast(mouseRay, out hit, float.MaxValue))
            {
                var newMouseLocation = _terrain.Convert.WorldToGrid(hit.point);

                if (newMouseLocation != _mouseLocation)
                {
                    if (!_cursor.IsActive)
                        _cursor.Activate();

                    _mouseLocation = newMouseLocation;
                    _cursor.Place(_mouseLocation.x, _mouseLocation.z);
                }
            }
            else
            {
                if (_cursor.IsActive)
                {
                    _cursor.Deactivate();
                    _mouseLocation = Point3.Null;
                }
            }

            // Debugging
            if (Application.isEditor && Input.GetMouseButtonDown(1))
            {
                GameLogger.Info("Selected ({0}); Point Heights ({1}, {2}, {3}, {4})",
                       _mouseLocation,
                       _terrain.GetVertexHeight(_mouseLocation.x, _mouseLocation.z),
                       _terrain.GetVertexHeight(_mouseLocation.x + 1, _mouseLocation.z),
                       _terrain.GetVertexHeight(_mouseLocation.x, _mouseLocation.z + 1),
                       _terrain.GetVertexHeight(_mouseLocation.x + 1, _mouseLocation.z + 1));

                if (Input.GetKey(KeyCode.LeftControl))
                {
                    int material = _terrain.GetMaterial(_mouseLocation.x, _mouseLocation.z);
                    _terrain.SetMaterial(_mouseLocation.x, _mouseLocation.z, (material + 1) % _terrain.MaterialCount);
                }
            }
        }
    }
}
