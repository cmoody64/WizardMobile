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
        // todo should be inner class like location
        // TODO only component provider should be able to create instances of this class
        // each session creates and manages its own storyboard, but adds images to the canvas
        // no global storyboard - prevents animation leaks
        public class AnimationSession
        {
            public AnimationSession(GamePage gamePage)
            {
                _gamePage = gamePage;
                _sessionId = sessionCount++;

                _storyboard = new Storyboard();
                // hook storyboard into gamepage XAML and give it a unique key (so that multiple sessions / storyboards can be present at once)
                gamePage.Resources[this.StoryboardKey] = _storyboard;
            }

            private GamePage _gamePage;
            private Storyboard _storyboard;
            private int _sessionId;

            public void Begin()
            {
                _storyboard.Begin();
                _storyboard.Completed += (sender, eventArgs) =>
                {
                    // when session animations are complete, clean up storyboard and remove it from gamepage canvas
                    _storyboard.Stop();
                    _storyboard.Children.Clear();
                    _gamePage.Resources.Remove(this.StoryboardKey);
                    this.Completed(this, null);
                };
            }

            public event EventHandler Completed;

            public void AddCard(string cardName, CardLocation location, double delay = 0.0)
            {

            }

            // adds multiple cards at the same time arranged in the order specified by the cardNames list
            public void AddCards(IEnumerable<string> cardNames, CardLocation location, double delay = 0.0)
            {
                
            }

            public void RemoveCard(string cardName, CardLocation location, double delay = 0.0)
            {

            }

            public void TranslateCard(string cardName, CardLocation source, CardLocation destination, AnimationBehavior animBehavior)
            {

            }

            private string StoryboardKey => $"game_canvas_storyboard_{_sessionId}";

            private static int sessionCount = 0;
        }
    }
}
