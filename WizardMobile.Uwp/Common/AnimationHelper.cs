using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;


namespace WizardMobile.Uwp.Common
{
    public static class AnimationHelper
    {

        // creates the animation objects associated with translating / rotating a single card
        public static List<DoubleAnimation> ComposeImageAnimations(InflatedAnimationRequest animReq)
        {
            var image = animReq.Image ?? throw new ArgumentNullException("ImageAnimationRequest.Image may not be null");
            var duration = animReq.Duration;
            var delay = animReq.Delay;

            var animations = new List<DoubleAnimation>();
            Point curLocation = new Point((double)image.GetValue(Canvas.LeftProperty), (double)image.GetValue(Canvas.TopProperty));
            var destination = animReq.Destination;

            // position animations (Canvas.Left and Canvas.Top)
            if (destination.X != curLocation.X)
            {
                var leftPropAnimation = new DoubleAnimation();
                leftPropAnimation.From = curLocation.X;
                leftPropAnimation.To = destination.X;
                leftPropAnimation.Duration = TimeSpan.FromSeconds(duration);
                leftPropAnimation.BeginTime = TimeSpan.FromSeconds(delay);

                Storyboard.SetTarget(leftPropAnimation, image);
                Storyboard.SetTargetProperty(leftPropAnimation, "(Canvas.Left)");

                //TODO pause and restart storyboard before setting new props
                //leftPropAnimation.Completed += (sender, eventArgs) => leftPropAnimation.SetValue(Canvas.LeftProperty, destination.X);

                animations.Add(leftPropAnimation);
            }

            if (destination.Y != curLocation.Y)
            {
                var topPropAnimation = new DoubleAnimation();
                topPropAnimation.From = curLocation.Y;
                topPropAnimation.To = destination.Y;
                topPropAnimation.Duration = TimeSpan.FromSeconds(duration);
                topPropAnimation.BeginTime = TimeSpan.FromSeconds(delay);

                Storyboard.SetTarget(topPropAnimation, image);
                Storyboard.SetTargetProperty(topPropAnimation, "(Canvas.Top)");

                //TODO pause and restart storyboard before setting new props
                //topPropAnimation.Completed += (sender, eventArgs) => topPropAnimation.SetValue(Canvas.TopProperty, destination.Y);

                animations.Add(topPropAnimation);
            }

            // rotation animations
            var rotations = animReq.Rotations;
            if (rotations != 0 && image.RenderTransform != null && image.RenderTransform.GetType() == typeof(RotateTransform))
            {
                var rotationAnimation = new DoubleAnimation();
                double curAngle = ((RotateTransform)image.RenderTransform).Angle;
                var finalAngle = curAngle + 360 * rotations;
                rotationAnimation.From = curAngle;
                rotationAnimation.To = finalAngle;
                rotationAnimation.Duration = TimeSpan.FromSeconds(duration);
                rotationAnimation.BeginTime = TimeSpan.FromSeconds(delay);                

                Storyboard.SetTarget(rotationAnimation, image);
                Storyboard.SetTargetProperty(rotationAnimation, "(Image.RenderTransform).(RotateTransform.Angle)");

                //rotationAnimation.Completed += (sender, eventArgs) => ((RotateTransform)image.RenderTransform).Angle = finalAngle;

                animations.Add(rotationAnimation);
            }

            return animations;
        }

        public static InflatedAnimationRequest InflateAnimationRequest(AnimationRequest animRequest, Image image)
        {
            return new InflatedAnimationRequest
            {
                Destination = animRequest.Destination,
                Delay = animRequest.Delay,
                Duration = animRequest.Duration,
                Rotations = animRequest.Duration,
                Image = image
            };
        }
    }

    // identical to Animation request but instead of a Guid reference to an image, the image has been inflated
    // to represent a full image object (bitmap, position, etc...)
    // used in layers that deal directly with the canvas / resource map (e.g. GamePage)
    public class InflatedAnimationRequest: AnimationBehavior
    {
        public Point Destination { get; set; }
        public Image Image { get; set; }
    }

    // extends animation behavior by providing enough details to produce an instance of an animation.
    // used in layers where the concept of a UniqueImage is present, meaning that the layer contains
    // references to images but not image objects (e.g. CardGroup layer)
    public class AnimationRequest: AnimationBehavior
    {
        public Point Destination { get; set; }
        public string ImageGuid { get; set; }
    }

    // description of animation without providing concrete details about an animation instance
    // tells how while excluding the what / where
    public class AnimationBehavior
    {
        public double Rotations { get; set; }
        public double Duration { get; set; } // length of animation in seconds
        public double Delay { get; set; } // seconds before animation begins
    }
}
