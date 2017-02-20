using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniversitySim.Framework
{
    /// <summary>
    /// I'm lazy and sleepy.
    /// This class just loads miscellaneous (sp?) images and stores them for when you need them.
    /// </summary>
    class ImageCatalog
    {
        /// <summary>
        /// Hard-compiled list of images that we load in the lazy static global class of images
        /// </summary>
        private List<string> imageNames = new List<string>
        {
            "ScreenElements/DeleteBuildingButton",
            "ScreenElements/EditBuildingButton"
        };

        /// <summary>
        /// Map of loaded images
        /// </summary>
        private Dictionary<string, Texture2D> imageMap;

        /// <summary>
        /// Singleton design! The most lazy of all designs!!
        /// </summary>
        public static ImageCatalog Instance = new ImageCatalog();

        /// <summary>
        /// Load images from a hard-coded list compiled into the assembly.
        /// I'm not making a configuration because
        ///     a) I'm lazy and sleepy (see above)
        ///     b) When would I configure this? The images shouldn't change without some code change. right?
        /// </summary>
        /// <param name="contentMan"></param>
        public void LoadContent(ContentManager contentMan)
        {
            this.imageMap = new Dictionary<string, Texture2D>();

            foreach (string image in this.imageNames)
            {
                this.imageMap.Add(image, contentMan.Load<Texture2D>(image));
            }
        }

        /// <summary>
        /// Gets an image that has been stored in the image map.
        /// Warning: scary uncaught null reference exceptions can happen here if you're not careful
        /// </summary>
        /// <param name="imageName">Key name of the image. This is the full path of the image name, minus the extension, from the content project</param>
        /// <returns></returns>
        public Texture2D Get(string imageName)
        {
            return this.imageMap[imageName];
        }
    }
}
