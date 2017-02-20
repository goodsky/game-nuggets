using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace rogue_bot.GameObjects.Weapons
{
    class Weapon : GameObject
    {
        // A 4 dimensional Array to hold the 4 directions of attack animation
        Texture2D[] image = new Texture2D[4];
        
        // Power, Speed and Range are the 3 weapon parameters
        double Power, Speed, Range;

        // Each weapon has a name and a description
        string Name, Description;

        public Weapon(double x, double y, double width, double height)
            : base(x, y, width, height)
        {

        }
    }
}
