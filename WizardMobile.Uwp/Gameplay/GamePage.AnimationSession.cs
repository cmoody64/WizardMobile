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
        // each session creates and manages its own storyboard, but adds images to the canvas
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
                var addAnimationRequests = _gamePage.CardGroups[location].AddWithAnimation(cardName);
                if (delay > 0)
                    foreach (var animRequest in addAnimationRequests)
                        animRequest.Delay += delay;

                var addAnimations = addAnimationRequests.Aggregate(new List<DoubleAnimation>(), (animList, animRequest) =>
                {
                    animList.AddRange(AnimationHelper.ComposeImageAnimations(animRequest));
                    return animList;
                });

                _storyboard.Children.AddRange(addAnimations);
            }

            // adds multiple cards at the same time arranged in the order specified by the cardNames list
            public void AddCards(IEnumerable<string> cardNames, CardLocation location, double delay = 0.0)
            {
                foreach(var cardName in cardNames)
                    AddCard(cardName, location, delay);
            }

            public void RemoveCard(string cardName, CardLocation location, double delay = 0.0)
            {
                var removeAnimationRequests = _gamePage.CardGroups[location].RemoveWithAnimation(cardName);
                if (delay > 0)
                    foreach (var animRequest in removeAnimationRequests)
                        animRequest.Delay += delay;

                var removeAnimations = removeAnimationRequests.Aggregate(new List<DoubleAnimation>(), (animList, anim) =>
                {
                    animList.AddRange(AnimationHelper.ComposeImageAnimations(anim));
                    return animList;
                });

                _storyboard.Children.AddRange(removeAnimations);
            }

            public void TranslateCard(string cardName, CardLocation source, CardLocation destination, AnimationBehavior animBehavior)
            {
                
            }

            private string StoryboardKey => $"game_canvas_storyboard_{_sessionId}";

            private static int sessionCount = 0;
        }
    }
}
