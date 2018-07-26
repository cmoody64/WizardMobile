using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Animation;
using WizardMobile.Uwp.Common;

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
                // TODO hook storyboard into gamepage XAML
            }

            private GamePage _gamePage;
            private Storyboard _storyboard;
            // TODO add different animation methods

            public void Begin()
            {
                _storyboard.Begin();
                _storyboard.Completed += (sender, eventArgs) =>
                {
                    // TODO remove storyboard from gamepage canvas
                    _storyboard.Children.Clear();
                    this.Completed(this, null);
                };
            }

            public event EventHandler Completed;

            public void AddCard(string cardName, CardGroupName cardGroup, double delay)
            {

            }

            public void RemoveCard(string cardName, CardGroupName cardGroup, double delay)
            {

            }

            // default animation behavior
            public void TransferCard(string cardName, CardGroupName source, CardGroupName destination)
            {

            }

            // custom animation behavior
            public void TransferCard(string cardName, CardGroupName source, CardGroupName destination, AnimationBehavior animBehavior)
            {

            }
        }
    }
}
