using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.Gameplay
{
    public sealed partial class GamePage
    {
        public class CardGroup
        {
            public CardGroup(GamePage parent, Point origin, double orientationDegress, LayoutType layoutType)
            {
                _parent = parent;
                _layoutType = layoutType;
                Origin = origin;
                OrientationDegress = orientationDegress;
            }

            public enum LayoutType
            {
                STACK,
                ADJACENT,
                TAPERED_STACK
            }

            public Point Origin { get; }
            public double OrientationDegress { get; }

            private GamePage _parent;
            private LayoutType _layoutType;

            public IEnumerable<ImageAnimationRequest> AddWithAnimation(string cardName)
            {
                return null;
            }

            public IEnumerable<ImageAnimationRequest> RemoveWithAnimation(string cardName)
            {
                return null;
            }
        }
    }
}
