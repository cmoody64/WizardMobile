using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Uwp.Gameplay
{
    // represents a normalized point on a canvas with a size of 100, 100
    // e.g. 0, 0 => top right; 50, 50 center
    public class CanvasPosition
    {
        // normalizes x and y so that they are inside of the canvas range (0->100)
        public CanvasPosition(double x, double y)
        {
            if (x > NORMALIZED_WIDTH)
                NormalizedX = NORMALIZED_WIDTH;
            else if (x < 0)
                NormalizedX = 0;
            else
                NormalizedX = x;

            if (y > NORMALIZED_HEIGHT)
                NormalizedY = NORMALIZED_HEIGHT;
            else if (y < 0)
                NormalizedY = 0;
            else
                NormalizedY = y;
        }

        public double NormalizedX { get; }
        public double NormalizedY { get; }

        private readonly static int NORMALIZED_WIDTH = 100;
        private readonly static int NORMALIZED_HEIGHT = 100;
    }
}
