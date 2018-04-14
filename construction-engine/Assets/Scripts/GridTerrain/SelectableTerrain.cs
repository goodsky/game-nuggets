using Common;
using System;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Behaviour to forward terrain clicks to the game state manager.
    /// </summary>
    public class SelectableTerrain : Selectable
    {
        public GridMesh Terrain;

        private Vector3 _mousePosition;
        private Point3 _mouseGridLocation;

        /// <summary>
        /// Unity's start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _mousePosition = Vector3.zero;
            _mouseGridLocation = Point3.Null;

            OnMouseDown = Clicked;
        }

        /// <summary>
        /// Unity's update method.
        /// </summary>
        protected override void Update()
        {
            if (_mousePosition.Equals(Input.mousePosition))
                return;

            _mousePosition = Input.mousePosition;
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(_mousePosition);

            RaycastHit hit;
            if (Terrain.Collider.Raycast(mouseRay, out hit, float.MaxValue))
            {
                var newMouseLocation = Terrain.Convert.WorldToGrid(hit.point);
                if (newMouseLocation != _mouseGridLocation)
                {
                    _mouseGridLocation = newMouseLocation;

                    var args = new TerrainSelectionUpdateArgs(_mouseGridLocation);
                    Game.State.SelectionUpdateTerrain(args);
                }
            }
            else
            {
                if (_mouseGridLocation != Point3.Null)
                {
                    _mouseGridLocation = Point3.Null;

                    var args = new TerrainSelectionUpdateArgs(_mouseGridLocation);
                    Game.State.SelectionUpdateTerrain(args);
                }
            }
        }

        /// <summary>
        /// Called when the terrain is clicked.
        /// </summary>
        /// <param name="mouse">The mouse button that was clicked.</param>
        private void Clicked(MouseButton mouse)
        {
            var args = new TerrainClickedArgs(mouse, _mouseGridLocation);
            Game.State.ClickedTerrain(args);
        }
    }

    /// <summary>
    /// Terrain selection update event argument.
    /// </summary>
    public class TerrainSelectionUpdateArgs : EventArgs
    {
        public Point3 SelectionLocation { get; private set; }

        public TerrainSelectionUpdateArgs(Point3 selectionLocation)
        {
            SelectionLocation = selectionLocation;
        }
    }

    /// <summary>
    /// Terrain clicked event argument.
    /// </summary>
    public class TerrainClickedArgs : EventArgs
    {
        public MouseButton Button { get; private set; }
        public Point3 ClickLocation { get; private set; }

        public TerrainClickedArgs(MouseButton button, Point3 clickLocation)
        {
            Button = button;
            ClickLocation = clickLocation;
        }
    }
}
