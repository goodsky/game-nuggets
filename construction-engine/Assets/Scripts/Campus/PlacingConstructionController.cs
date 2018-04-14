using Common;
using GameData;
using GridTerrain;
using UnityEngine;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    public class PlacingConstructionController : GameStateController
    {
        private GridMesh _terrain;
        private GridCursor[,] _cursors;
        private Material _gridMaterial;

        private Point3 _mouseLocation;
        private BuildingData _building;

        /// <summary>
        /// Instantiates an instance of the controller.
        /// </summary>
        /// <param name="terrain">The terrain to place construction on.</param>
        public PlacingConstructionController(GridMesh terrain)
        {
            _terrain = terrain;
            _cursors = null;
            _gridMaterial = Resources.Load<Material>("Terrain/cursor");
            _building = null;

            OnTerrainClicked += Build;
        }

        /// <summary>
        /// The state controller is starting.
        /// </summary>
        /// <param name="context">The construction to place.</param>
        public override void TransitionIn(object context)
        {
            _building = context as BuildingData;
            if (_building == null)
                GameLogger.FatalError("PlacingConstructionController was not given a building data!");

            int xSize = _building.Footprint.GetLength(0);
            int zSize = _building.Footprint.GetLength(1);

            _cursors = new GridCursor[xSize, zSize];
            for (int x = 0; x < xSize; ++x)
            {
                for (int z = 0; z < zSize; ++z)
                {
                    if (_building.Footprint[x, z])
                        _cursors[x, z] = GridCursor.Create(_terrain, _gridMaterial);
                    else
                        _cursors[x, z] = null;
                }
            }

            _mouseLocation = Point3.Null;
        }

        /// <summary>
        /// The state controller is stopped.
        /// </summary>
        public override void TransitionOut()
        {
            if (_cursors != null)
            {
                foreach (var cursor in _cursors)
                {
                    if (cursor != null)
                    {
                        cursor.Deactivate();
                        UnityEngine.Object.Destroy(cursor);
                    }
                }
            }

            _mouseLocation = Point3.Null;
        }

        /// <summary>
        /// Called each step of this state.
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
                    _mouseLocation = newMouseLocation;

                    int xSize = _building.Footprint.GetLength(0);
                    int zSize = _building.Footprint.GetLength(1);

                    for (int x = 0; x < xSize; ++x)
                    {
                        for (int z = 0; z < zSize; ++z)
                        {
                            if (_cursors[x, z] != null)
                            {
                                int cursorX = _mouseLocation.x + x;
                                int cursorZ = _mouseLocation.z + z;

                                if (cursorX < _terrain.CountX && cursorZ < _terrain.CountZ)
                                {
                                    _cursors[x, z].Activate();
                                    _cursors[x, z].Place(cursorX, cursorZ);
                                }
                                else
                                {
                                    _cursors[x, z].Deactivate();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (_cursors[0, 0].IsActive)
                {
                    foreach (var cursor in _cursors)
                        if (cursor != null)
                            cursor.Deactivate();

                    _mouseLocation = Point3.Null;
                }
            }
        }

        /// <summary>
        /// Event handler for clicks on the terrain.
        /// </summary>
        /// <param name="sender">not used.</param>
        /// <param name="args">The terrain click arguments.</param>
        private void Build(object sender, TerrainClickedArgs args)
        {
            if (args.Button == MouseButton.Left)
            {
                CampusFactory.GenerateBuilding(
                    _building, 
                    Game.Campus.transform, 
                    _terrain.Convert.GridToWorld(_mouseLocation) + new Vector3(0f, 0.01f, 0f) /* Place just above the grass*/, 
                    Quaternion.identity);

                SelectionManager.UpdateSelection(SelectionManager.Selected.ToMainMenu());
            }
        }
    }
}
