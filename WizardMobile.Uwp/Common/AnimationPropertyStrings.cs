using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.Common
{
    public static class AnimationPropertyStrings
    {
        public static string CANVAS_LEFT => "(Canvas.Left)";
        public static string CANVAS_TOP => "(Canvas.Top)";
        public static string ANGLE => "(FrameworkElement.RenderTransform).(RotateTransform.Angle)";
        public static string OPACITY => "(FrameworkElement.Opacity)";
        public static string FONT_SIZE => "(TextBlock.FontSize)";
    }
}
