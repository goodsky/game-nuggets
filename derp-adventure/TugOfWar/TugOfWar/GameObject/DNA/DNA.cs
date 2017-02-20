using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TugOfWar.GameObject.DNA
{
    class DNA
    {
        public int TextureID;
        
        // Here we will have all of the modifiers for DNA to change the evolutionary algorithm

        public DNA(int texID)
        {
            TextureID = texID;
        }
    }
}
