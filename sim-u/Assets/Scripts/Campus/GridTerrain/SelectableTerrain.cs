using Common;
using System;
using UnityEngine;

namespace Campus.GridTerrain
{
    /// <summary>
    /// Behaviour to forward terrain clicks to the game state manager.
    /// </summary>
    public class SelectableTerrain : Selectable
    {
        public GridMesh Terrain;

        private Vector3 _mousePosition;
        private Point3 _mouseGridLocation;
        private Point2 _mouseVertexLocation;

        /// <summary>
        /// Unity's start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _mousePosition = Vector3.zero;
            _mouseGridLocation = Point3.Null;
            _mouseVertexLocation = Point2.Null;

            OnMouseDown = Clicked;
        }

        /// <summary>
        /// Unity's update method.
        /// </summary>
        protected override void Update()
        {
            _mousePosition = Input.mousePosition;
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(_mousePosition);

            Point3 currentGridSelection;
            Point2 currentVertexSelection;

            RaycastHit hit;
            if (Terrain.Collider.Raycast(mouseRay, out hit, float.MaxValue))
            {
                currentGridSelection = Terrain.Convert.WorldToGrid(hit.point);
                currentVertexSelection = Terrain.Convert.WorldToGridVertex(hit.point);
            }
            else
            {
                currentGridSelection = Point3.Null;
                currentVertexSelection = Point2.Null;
            }

            if (currentGridSelection != _mouseGridLocation)
            {
                _mouseGridLocation = currentGridSelection;

                var args = new TerrainGridUpdateArgs(_mouseGridLocation);
                Game.State.UpdateTerrainGridSelection(args);
            }

            if (currentVertexSelection != _mouseVertexLocation)
            {
                _mouseVertexLocation = currentVertexSelection;

                var args = new TerrainVertexUpdateArgs(_mouseVertexLocation);
                Game.State.UpdateTerrainVertexSelection(args);
            }
        }

        /// <summary>
        /// Called when the terrain is clicked.
        /// </summary>
        /// <param name="mouse">The mouse button that was clicked.</param>
        private void Clicked(MouseButton mouse)
        {
            var args = new TerrainClickedArgs(mouse, _mouseGridLocation, _mouseVertexLocation);
            Game.State.ClickedTerrain(args);
        }
    }

    /// <summary>
    /// Terrain selection update event argument.
    /// </summary>
    public class TerrainGridUpdateArgs : EventArgs
    {
        public Point3 GridSelection { get; private set; }

        internal TerrainGridUpdateArgs(Point3 gridSelection)
        {
            GridSelection = gridSelection;
        }
    }

    /// <summary>
    /// Terrain vertex selection update event argument.
    /// </summary>
    public class TerrainVertexUpdateArgs : EventArgs
    {
        public Point2 VertexSelection { get; private set; }

        internal TerrainVertexUpdateArgs(Point2 vertexSelection)
        {
            VertexSelection = vertexSelection;
        }
    }

    /// <summary>
    /// Terrain clicked event argument.
    /// </summary>
    public class TerrainClickedArgs : EventArgs
    {
        public MouseButton Button { get; private set; }
        public Point3 GridSelection { get; private set; }
        public Point2 VertexSelection { get; private set; }

        internal TerrainClickedArgs(MouseButton button, Point3 gridSelection, Point2 vertexSelection)
        {
            Button = button;
            GridSelection = gridSelection;
            VertexSelection = vertexSelection;
        }
    }
}
