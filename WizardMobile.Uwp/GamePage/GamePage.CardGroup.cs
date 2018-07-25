using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace WizardMobile.Uwp
{
    public sealed partial class GamePage
    {
        private class CardGroup
        {
            public CardGroup(GamePage parent, Point origin, double orientationDegress)
            {
                parent = _parent;
                Origin = origin;
                OrientationDegress = orientationDegress;
            }

            public Point Origin { get; }
            public double OrientationDegress { get; }

            private GamePage _parent;

            public void Add(string cardName) { }

            public void Remove(string cardName) { }
        }
    }
}
