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
        private class CardGroup
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

            public void AddWithAnimation(string cardName) { }

            public void RemoveWithAnimation(string cardName) { }

            // moves a card from this card group to the provided card group
            public void TransferWithAnimation(CardGroup otherCardGroup) { }

            // NOTE an attempt will be made to apply the provided beavior, however, if this animation will result in a misaligned card
            // at the destination card group, the behavior will be overriden. i.e. the number of rotations provided does not align with the
            // destination group (rotations round up to nearest alignment)
            public void TransferWithAnimation(CardGroup otherCardgroup, AnimationBehavior animationBehavior) { }

            private Image GetCardImage(string cardImageKey, Point position, double angle = 0)
            {
                var bitmapImage = _parent.game_canvas.Resources[cardImageKey] as BitmapImage;
                var image = new Image();

                image.Source = bitmapImage;

                Canvas.SetLeft(image, position.X);
                Canvas.SetTop(image, position.Y);

                image.RenderTransform = new RotateTransform { Angle = angle };
                image.RenderTransformOrigin = new Point(0.5, 0.5);

                return image;
            }

            private static readonly AnimationBehavior DEFAULT_INTERGROUP_ANIMATION_BEHAVIOR = new AnimationBehavior
            {
                DelaySeconds = 0.5,
                Rotations = 0.5,
                DurationSeconds = 0.5
            };
        }
    }
}
