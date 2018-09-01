using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.GamePage
{
    internal static class CanvasNormalization
    {
        public static readonly int MAX_X = 100;
        public static readonly int MAX_Y = 100;
    }

    // represents a normalized point on a canvas with a size of 100, 100
    // e.g. 0, 0 => top right; 50, 50 center
    public class NormalizedPosition
    {
        // normalizes x and y so that they are inside of the canvas range (0->100)
        public NormalizedPosition(double x, double y)
        {
            //if (x > CanvasNormalization.MAX_X)
            //    NormalizedX = CanvasNormalization.MAX_X;
            //else if (x < 0)
            //    NormalizedX = 0;
            //else
            //    NormalizedX = x;

            //if (y > CanvasNormalization.MAX_Y)
            //    NormalizedY = CanvasNormalization.MAX_Y;
            //else if (y < 0)
            //    NormalizedY = 0;
            //else
            //    NormalizedY = y;

            NormalizedX = x;
            NormalizedY = y;
        }

        public double NormalizedX { get; }
        public double NormalizedY { get; }

        public override bool Equals(object obj)
        {
            var other = obj as NormalizedPosition;
            return other?.NormalizedX == NormalizedX && other?.NormalizedY == NormalizedY;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class NormalizedSize
    {
        public NormalizedSize(double width, double height)
        {
            //if (width > CanvasNormalization.MAX_X)
            //    NormalizedWidth = CanvasNormalization.MAX_X;
            //else if (width < 0)
            //    NormalizedWidth = 0;
            //else
            //    NormalizedWidth = width;

            //if (height > CanvasNormalization.MAX_Y)
            //    NormalizedHeight = CanvasNormalization.MAX_Y;
            //else if (height < 0)
            //    NormalizedHeight = 0;
            //else
            //    NormalizedHeight = height;
            NormalizedWidth = width;
            NormalizedHeight = height;
        }

        public double NormalizedWidth { get; }
        public double NormalizedHeight { get; }

        public override bool Equals(object obj)
        {
            var other = obj as NormalizedSize;
            return other?.NormalizedWidth == NormalizedWidth
                && other?.NormalizedHeight == NormalizedHeight;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
