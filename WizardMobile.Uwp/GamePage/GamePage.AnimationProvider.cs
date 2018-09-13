using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.GamePage
{
    public sealed partial class GamePage: IAnimationProvider
    {
        public void QueueAnimationRequest(NamedAnimationRequest animationRequest)
        {
            FrameworkElement targetElement = FindName(animationRequest.TargetElementName) as FrameworkElement
                ?? throw new ArgumentNullException($"{animationRequest.TargetElementName} didn't map to XAML element");
            Point? destination = null;
            if (animationRequest.Destination != null)
            {
                destination = DenormalizePosition(animationRequest.Destination, _cardBitmapSize);
                // this does not set the position, it only registers the destination position of the image for dynamic repositioning on size change
                RegisterElementCanvasPosition(targetElement, animationRequest.Destination, _cardBitmapSize);
            }
            var inflatedReq = AnimationHelper.InflateAnimationRequest(animationRequest, targetElement, destination);
            List<DoubleAnimation> animations = AnimationHelper.ComposeImageAnimations(inflatedReq);

            animationQueue.AddRange(animations);
        }

        public void QueueAnimationRequests(IEnumerable<NamedAnimationRequest> animations)
        {
            foreach (var animation in animations)
                QueueAnimationRequest(animation);
        }

        public Task<bool> RunQueuedAnimations()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            game_canvas_storyboard.Children.AddRange(animationQueue);
            animationQueue.Clear();

            QueueAnimationsCompletedHandler(() =>
            {
                game_canvas_storyboard.Stop();

                foreach (DoubleAnimation anim in game_canvas_storyboard.Children)
                    ApplyAnimationEndValue(anim);

                game_canvas_storyboard.Children.Clear();
                taskCompletionSource.SetResult(true);
            });

            game_canvas_storyboard.Begin();
            return taskCompletionSource.Task;
        }

        private void QueueAnimationsCompletedHandler(Action handler) => _animationsCompletedHandlers.Enqueue(handler);
        private Queue<Action> _animationsCompletedHandlers;
        private void OnAnimationsCompleted(object sender, object eventArgs)
        {
            game_canvas_storyboard.Stop();

            foreach (DoubleAnimation anim in game_canvas_storyboard.Children)
                ApplyAnimationEndValue(anim);

            game_canvas_storyboard.Children.Clear();
            // run all queued animations completed handlers
            while (_animationsCompletedHandlers.Count > 0)
            {
                var handler = _animationsCompletedHandlers.Dequeue();
                handler();
            }
        }
    }
}
