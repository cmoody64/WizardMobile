using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.Gameplay
{
    public sealed partial class GamePage
    {
        // todo should be inner class like CardGroup
        // TODO only component provider should be able to create instances of this class
        // each session creates and manages its own storyboard, but adds images to the canvas
        // no global storyboard - prevents animation leaks
        public class AnimationSession
        {
            public AnimationSession(GamePage gamePage)
            {
                _gamePage = gamePage;
            }

            private GamePage _gamePage;
            // TODO add different animation methods

            void Begin()
            {

            }
        }
    }
}
