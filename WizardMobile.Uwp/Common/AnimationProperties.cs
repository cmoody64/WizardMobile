using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace WizardMobile.Uwp.Common
{
    public static class AnimationProperties
    {
        public static string CANVAS_LEFT => "(Canvas.Left)";
        public static string CANVAS_TOP => "(Canvas.Top)";
        public static string ANGLE => "(FrameworkElement.RenderTransform).(RotateTransform.Angle)";
        public static string SCALE_X => "(FrameworkElement.RenderTransform).(ScaleTransform.ScaleX)";
        public static string SCALE_Y => "(FrameworkElement.RenderTransform).(ScaleTransform.ScaleY)";
        public static string OPACITY => "(FrameworkElement.Opacity)";
        public static string FONT_SIZE => "(TextBlock.FontSize)";

        public static Dictionary<string, Action<DoubleAnimation, FrameworkElement>> AnimationCompletedCallbacks = new Dictionary<string, Action<DoubleAnimation, FrameworkElement>>
        {
            { CANVAS_LEFT, OnCanvasLeftAnimationComplete },
            { CANVAS_TOP, OnCanvasTopAnimationComplete },
            { ANGLE, OnAngleAnimationComplete },
            { SCALE_X, OnScaleXAnimationComplete },
            { SCALE_Y, OnScaleYAnimationComplete },            
            { OPACITY, OnOpacityAnimationComplete },
            { FONT_SIZE, OnFontSizeAnimationComplete }
        };

        private static void OnCanvasLeftAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {
            Canvas.SetLeft(element, animation.To ?? 0.0);
        }

        private static void OnCanvasTopAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {
            Canvas.SetTop(element, animation.To ?? 0.0);
        }

        private static void OnAngleAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {
            ((RotateTransform)element.RenderTransform).Angle = animation.To ?? 0.0;
        }

        private static void OnScaleXAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {
            
        }

        private static void OnScaleYAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {

        }

        private static void OnOpacityAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {
            element.Opacity += animation.By.Value;
        }

        private static void OnFontSizeAnimationComplete(DoubleAnimation animation, FrameworkElement element)
        {

        }
    }
}
