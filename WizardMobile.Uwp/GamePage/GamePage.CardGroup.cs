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

namespace WizardMobile.Uwp
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

            public void Add(string cardName) { }

            public void Remove(string cardName) { }

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
        }
    }
}
