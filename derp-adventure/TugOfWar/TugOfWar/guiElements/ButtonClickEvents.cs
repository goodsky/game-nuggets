using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TugOfWar.guiElements
{
    // This is IEEE Standard 32523532: Bad Programming.
    // Yeah, I'm putting all of my button events as static method calls in this class. In the short term, this is a great idea.
    // Waiting on Future Skyler to report in and let me know about that finicky long-term.
    class ButtonClickEvents
    {
        public static void EvoBlockBuildEvent(Button button)
        {
            if (button.Active)
            {
                Game.input.constructionState = button.ButtonArgs1;
            }
        }
    }
}
