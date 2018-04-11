using Common;
using GridTerrain;
using System;

namespace Campus
{
    /// <summary>
    /// Game controller that runs during the PlacingContruction game state.
    /// </summary>
    public class PlacingConstructionController : GameStateController
    {
        private GridMesh _terrain;

        public PlacingConstructionController(GridMesh terrain)
        {
            _terrain = terrain;
        }

        public override void TransitionIn(object context)
        {
            
        }

        public override void TransitionOut()
        {
            
        }

        public override void Update()
        {
            
        }
    }
}
