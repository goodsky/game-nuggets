using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniversitySim.ScreenElements;
using UniversitySim.Utilities;

namespace UniversitySim.Framework
{
    /// <summary>
    /// The translucent ghost of what you want to construct that pops up after clicking on the toolbox.
    /// This can be either a ghost building or a ghost path depending on what you are building
    /// </summary>
    abstract class GhostConstruction : ScreenElement
    {
        // The information about the building we are building
        protected Data basedata;
        
        /// <summary>
        /// This constructor will be used by ghost construction implementations to pass in the building data for the building we are building
        /// </summary>
        /// <param name="data"></param>
        public GhostConstruction(Data data) : base(Constants.GUI_DEPTH - 1) 
        {
            if (data == null)
            {
                Logger.Log(LogLevel.Error, "GhostConstruction", string.Format("Null Data object passed into ghost construction. Probably an invalid cast in the catalog data."));
                return;
            }

            this.basedata = data;

            this.Size = new Pair<int>(this.basedata.Image.Width, this.basedata.Image.Height);
            this.Type = ElementType.Building;
            this.Clickable = true;
        }

        /// <summary>
        /// If something else gets the focus from me...
        /// Then there is nothing left to live for!!!
        /// </summary>
        /// <param name="other"></param>
        public override void UnClicked(ScreenElement other)
        {
            base.UnClicked(other);

            // GameScreen.BeginBuilding(null); // This causes an infinite loop :(
        }
    }
}
