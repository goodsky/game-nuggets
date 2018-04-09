using Common;
using UnityEngine;

namespace GridTerrain
{
    /// <summary>
    /// Behaviour to forward terrain clicks to the game state manager.
    /// </summary>
    public class SelectableTerrain : Selectable
    {
        public GridMesh Terrain;

        /// <summary>
        /// Unity's start method.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            OnMouseDown = Clicked;
        }

        /// <summary>
        /// Called when the terrain is clicked.
        /// </summary>
        /// <param name="mouse">The mouse button that was clicked.</param>
        private void Clicked(MouseButton mouse)
        {
            var mouseRay = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit hit;
            Point3 clickLocation; 
            if (Terrain.Collider.Raycast(mouseRay, out hit, float.MaxValue))
            {
                clickLocation = Terrain.Convert.WorldToGrid(hit.point);
            }
            else
            {                
                GameLogger.Warning("Raycast failed to hit terrain! {0}", mouseRay.ToString());
                clickLocation = new Point3(-1, -1, -1);
            }

            Game.State.ClickedTerrain(mouse, clickLocation);
        }
    }
}
