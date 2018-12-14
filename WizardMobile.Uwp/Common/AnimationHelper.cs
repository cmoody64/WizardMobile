using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using WizardMobile.Uwp.GamePage;

namespace WizardMobile.Uwp.Common
{
    public static class AnimationHelper
    {

        // creates the animation objects associated with translating / rotating a single card
        public static List<DoubleAnimation> ComposeImageAnimations(InflatedAnimationRequest animReq)
        {
            var targetElement = animReq.TargetElement ?? throw new ArgumentNullException("ImageAnimationRequest.Image may not be null");
            var duration = animReq.Duration;
            var delay = animReq.Delay;

            var animations = new List<DoubleAnimation>();
            Point curLocation = new Point((double)targetElement.GetValue(Canvas.LeftProperty), (double)targetElement.GetValue(Canvas.TopProperty));
            var destination = animReq.Destination;

            // position animations (Canvas.Left and Canvas.Top)
            if (destination.HasValue && destination.Value.X != curLocation.X)
            {
                var leftPropAnimation = new DoubleAnimation();
                leftPropAnimation.From = curLocation.X;
                leftPropAnimation.To = destination.Value.X;
                leftPropAnimation.Duration = TimeSpan.FromSeconds(duration);
                leftPropAnimation.BeginTime = TimeSpan.FromSeconds(delay);

                leftPropAnimation.EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 4
                };

                Storyboard.SetTargetName(leftPropAnimation, targetElement.Name);
                Storyboard.SetTargetProperty(leftPropAnimation, AnimationProperties.CANVAS_LEFT);

                animations.Add(leftPropAnimation);
            }

            if (destination.HasValue && destination.Value.Y != curLocation.Y)
            {
                var topPropAnimation = new DoubleAnimation();
                topPropAnimation.From = curLocation.Y;
                topPropAnimation.To = destination.Value.Y;
                topPropAnimation.Duration = TimeSpan.FromSeconds(duration);
                topPropAnimation.BeginTime = TimeSpan.FromSeconds(delay);

                topPropAnimation.EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 4
                };

                Storyboard.SetTargetName(topPropAnimation, targetElement.Name);
                Storyboard.SetTargetProperty(topPropAnimation, AnimationProperties.CANVAS_TOP);                

                animations.Add(topPropAnimation);
            }

            // rotation animations
            var rotations = animReq.Rotations;
            if (rotations != 0 && targetElement.RenderTransform != null && targetElement.RenderTransform.GetType() == typeof(RotateTransform))
            {
                var rotationAnimation = new DoubleAnimation();
                double curAngle = ((RotateTransform)targetElement.RenderTransform).Angle;
                var finalAngle = curAngle + 360 * rotations;
                rotationAnimation.From = curAngle;
                rotationAnimation.To = finalAngle;
                rotationAnimation.Duration = TimeSpan.FromSeconds(duration);
                rotationAnimation.BeginTime = TimeSpan.FromSeconds(delay);

                rotationAnimation.EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 4
                };

                Storyboard.SetTargetName(rotationAnimation, targetElement.Name);
                Storyboard.SetTargetProperty(rotationAnimation, AnimationProperties.ANGLE);

                animations.Add(rotationAnimation);
            }

            foreach(var animBehavior in animReq.AdditionalBehaviors)
            {
                var animation = new DoubleAnimation();
                animation.By = animBehavior.Value;
                animation.Duration = TimeSpan.FromSeconds(duration);
                animation.BeginTime = TimeSpan.FromSeconds(delay);
                animation.EnableDependentAnimation = true;
                animation.EasingFunction = new ExponentialEase()
                {
                    EasingMode = EasingMode.EaseOut,
                    Exponent = 4
                };
                Storyboard.SetTargetName(animation, targetElement.Name);
                Storyboard.SetTargetProperty(animation, animBehavior.Key);
                animations.Add(animation);
            }

            return animations;
        }

        public static InflatedAnimationRequest InflateAnimationRequest(NamedAnimationRequest animRequest, FrameworkElement targetElement,Point? destination)
        {
            return new InflatedAnimationRequest
            {
                Destination = destination,
                Delay = animRequest.Delay,
                Duration = animRequest.Duration,
                Rotations = animRequest.Rotations,
                TargetElement = targetElement,
                AdditionalBehaviors = animRequest.AdditionalBehaviors
            };
        }
    }

    // description of animation without providing concrete details about an animation instance
    // tells how while excluding the what / where
    public class AnimationBehavior
    {
        public double Rotations { get; set; }
        public double Duration { get; set; } // length of animation in seconds
        public double Delay { get; set; } // seconds before animation begins
        public Dictionary<string, double> AdditionalBehaviors = new Dictionary<string, double>(); // maps property string to animation "by" value
    }

    // Instead of containing a named reference to a xaml element, this request contains an inflated element
    // which represents a full image object (bitmap, position, etc...)
    // used in layers that deal directly with the canvas / resource map (e.g. GamePage)
    // also contains a Destination point member that corresponds directly to a canvas position
    public class InflatedAnimationRequest: AnimationBehavior
    {
        public Point? Destination { get; set; }
        public FrameworkElement TargetElement { get; set; }
    }

    // extends animation behavior by providing enough details to produce an instance of an animation.
    // used in layers where the concept of an animatable FrameworkElement is present, meaning that the layer contains
    // references to FrameworkElements but not FrameworkElement objects (e.g. CardGroup layer)
    // also contains a higher-level CanvasPosition member describing the normalized position on an abstract canvas
    public class NamedAnimationRequest : AnimationBehavior
    {
        public NormalizedPosition Destination { get; set; }
        public string TargetElementName { get; set; }
        public bool IsCenteredAtDestination = true;
    }
}
