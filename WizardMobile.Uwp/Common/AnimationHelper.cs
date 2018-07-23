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
        public static List<DoubleAnimation> ComposeImageAnimations(ImageAnimationRequest animReq)
        {
            var image = animReq.Image ?? throw new ArgumentNullException("ImageAnimationRequest.Image may not be null");
            var duration = animReq.DurationSeconds;
            var delay = animReq.DelaySeconds;

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

                leftPropAnimation.Completed += (sender, eventArgs) => leftPropAnimation.SetValue(Canvas.LeftProperty, destination.X);

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

                topPropAnimation.Completed += (sender, eventArgs) => topPropAnimation.SetValue(Canvas.TopProperty, destination.Y);

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

                rotationAnimation.Completed += (sender, eventArgs) => ((RotateTransform)image.RenderTransform).Angle = finalAngle;

                animations.Add(rotationAnimation);
            }

            return animations;
        }
    }

    public class ImageAnimationRequest
    {
        public Image Image { get; set; }
        public double Rotations { get; set; }
        public Point Destination { get; set; }
        public double DurationSeconds { get; set; }
        public double DelaySeconds { get; set; }
    }
}
