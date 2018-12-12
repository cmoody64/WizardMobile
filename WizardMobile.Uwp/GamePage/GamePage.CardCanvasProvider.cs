using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Core;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using WizardMobile.Uwp.Common;
using Windows.UI.Xaml.Media.Animation;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage;
using WizardMobile.Uwp.WizardFrontend;

namespace WizardMobile.Uwp.GamePage
{
    public sealed partial class GamePage: ICardCanvasProvider
    {
        public void InitializeCanvasFacade()
        {
            game_canvas.Loaded += (sender, args) => _cardBitmapDecodePixelHeight = (int)(game_canvas.ActualHeight * .2);
            game_canvas.SizeChanged += OnCanvasSizeChange;                       
        }

        /*************************** ICanvasFacade implementation *******************************/
        public void AddCard(UniqueDisplayCard card, NormalizedPosition canvasPositon, double orientationDegrees, int zIndex)
        {
            Image image = CreateCardImage(card);

            SetUiElementNormalizedCanvasPosition(image, canvasPositon, true);
            SetCardImageAngle(image, orientationDegrees);
            Canvas.SetZIndex(image, zIndex);

            image.PointerReleased += (sender, args) => FireCardClickedEvent(card);
            image.PointerEntered += (sender, args) => FireCardPointerEnteredEvent(card);
            image.PointerExited += (sender, args) => FireCardPointerExitedEvent(card);
            game_canvas.Children.Add(image);
        }

        public void RemoveCard(UniqueDisplayCard card)
        {
            Image elementToRemove = this.FindName(card.Id) as Image;
            game_canvas.Children.Remove(elementToRemove);
            UnregisterElementCanvasPosition(elementToRemove);
        }

        public void UpdateCard
        (
            UniqueDisplayCard cardToUpdate,
            NormalizedPosition canvasPositon = null,
            double? orientationDegrees = null,
            int? zIndex = null,
            bool? dimmed = null
        )
        {
            // check if the card bitmap needs an update
            Image imageToUpdate = this.FindName(cardToUpdate.Id) as Image;
            var originalBitmapFilepath = (imageToUpdate.Source as BitmapImage).UriSource.AbsolutePath;
            // if the original element already contains the updated bitmap, no change is needed, otherwise, replace the bitmap
            if (!originalBitmapFilepath.Contains(cardToUpdate.DisplayKey))
            {
                var bitmapImage = RetrieveCardBitmap(cardToUpdate.DisplayKey);
                imageToUpdate.Source = bitmapImage;
            }

            // update the position if provided
            if (canvasPositon != null)
            {
                // update position. if already equal, this is a noop
                SetUiElementNormalizedCanvasPosition(imageToUpdate, canvasPositon, true);
            }

            // update the orientnation if provided
            if (orientationDegrees.HasValue)
            {
                // update the orientation. if already equal, this is a noop
                SetCardImageAngle(imageToUpdate, orientationDegrees.Value);
            }

            if (zIndex.HasValue)
            {
                Canvas.SetZIndex(imageToUpdate, zIndex.Value);
            }

            if (dimmed.HasValue)
            {
                imageToUpdate.Opacity = dimmed.Value ? 0.5 : 1.0;
            }

        }

        public async Task<NormalizedSize> GetNormalizedCardImageSize()
        {
            Size size = await GetCardImageSize();
            return NormalizeSize(size);
        }

        private List<DoubleAnimation> animationQueue;


        /************************************** helpers **********************************************/
        // translates a high level normalized canvas position (0 -> 100) to actual canvas position (0 -> actual dimension)
        // NOTE optionally takes into acount image size so that it seems like the image is centered on pos
        private Point DenormalizePosition(NormalizedPosition pos, Size? boundingRectSize = null)
        {
            double x = pos.NormalizedX * game_canvas.ActualWidth / CanvasNormalization.MAX_X;
            double y = pos.NormalizedY * game_canvas.ActualHeight / CanvasNormalization.MAX_Y;

            // optionally shift x and y so that it seems like the point is centered around a given image
            if (boundingRectSize.HasValue)
            {
                x -= boundingRectSize.Value.Width / 2;
                y -= boundingRectSize.Value.Height / 2;
            }

            return new Point(x, y);
        }

        private NormalizedSize NormalizeSize(Size size)
        {
            double width = (size.Width / game_canvas.ActualWidth) * CanvasNormalization.MAX_X;
            double height = (size.Height / game_canvas.ActualHeight) * CanvasNormalization.MAX_Y;
            return new NormalizedSize(width, height);
        }

        private void ApplyAnimationEndValue(DoubleAnimation animation)
        {
            var imageName = Storyboard.GetTargetName(animation);
            var targetProperty = Storyboard.GetTargetProperty(animation);

            var element = FindName(imageName) as FrameworkElement;

            var animationCompletedCallback = AnimationProperties.AnimationCompletedCallbacks[targetProperty];
            animationCompletedCallback(animation, element);
        }

        private static void SetCardImageAngle(Image cardImage, double angle)
        {
            cardImage.RenderTransform = new RotateTransform { Angle = angle };
            cardImage.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        public event Action<UniqueDisplayCard> CardClicked;
        public event Action<UniqueDisplayCard> CardPointerEntered;
        public event Action<UniqueDisplayCard> CardPointerExited;
        private void FireCardClickedEvent(UniqueDisplayCard card) => CardClicked?.Invoke(card);
        private void FireCardPointerEnteredEvent(UniqueDisplayCard card) => CardPointerEntered?.Invoke(card);
        private void FireCardPointerExitedEvent(UniqueDisplayCard card) => CardPointerExited?.Invoke(card);

        private Image CreateCardImage(UniqueDisplayCard card)
        {
            var bitmapImage = RetrieveCardBitmap(card.DisplayKey);
            var image = new Image();
            image.Source = bitmapImage;
            image.Name = card.Id;
            return image;
        }

        private BitmapImage RetrieveCardBitmap(string bitmapKey)
        {
            var bitmapImage = game_canvas.Resources[bitmapKey] as BitmapImage;
            // scale down and maintain aspect ratio
            bitmapImage.DecodePixelHeight = (int)(game_canvas.ActualHeight * .20);
            return bitmapImage;
        }


        /******************************************     Dynamic Canvas Resizing     ***********************************************************/
        // cache size changed handlers so that it may be unsubscribed if size ever changes, preventing an update to stale size
        private Dictionary<FrameworkElement, SizeChangedEventHandler> _elementSizeChangedHandlers = new Dictionary<FrameworkElement, SizeChangedEventHandler>();
        private void SetUiElementNormalizedCanvasPosition(FrameworkElement element, NormalizedPosition position, bool centered = false)
        {
            Action positionSetter = () =>
            {
                Size? boundingRect = centered ? new Size?(new Size(element.ActualWidth, element.ActualHeight)) : null;
                var denormalizedPosition = DenormalizePosition(position, boundingRect);
                Canvas.SetLeft(element, denormalizedPosition.X);
                Canvas.SetTop(element, denormalizedPosition.Y);
                RegisterElementCanvasPosition(element, position, centered);
            };

            positionSetter();
            // if the element should be centered, attach a listener that recenters the elemenet on size change to guarentee that the element is always centered
            if (centered)
            {
                // check if a position setter was previously subscribed to this event. If so, it must be removed so that it doesn't update to a stale position
                if (_elementSizeChangedHandlers.ContainsKey(element))
                    element.SizeChanged -= _elementSizeChangedHandlers[element];
                _elementSizeChangedHandlers[element] = (object sender, SizeChangedEventArgs args) => positionSetter();
                element.SizeChanged += _elementSizeChangedHandlers[element];
            }
        }

        private Dictionary<FrameworkElement, Tuple<NormalizedPosition, bool>> _normalizedCanvasPositionRegistry = new Dictionary<FrameworkElement, Tuple<NormalizedPosition, bool>>();
        private void RegisterElementCanvasPosition(FrameworkElement el, NormalizedPosition pos, bool centered)
        {
            _normalizedCanvasPositionRegistry[el] = new Tuple<NormalizedPosition, bool>(pos, centered);
        }
        private bool UnregisterElementCanvasPosition(FrameworkElement el)
        {
            return _normalizedCanvasPositionRegistry.Remove(el);
        }
        private void OnCanvasSizeChange(object sender, SizeChangedEventArgs args)
        {
            var positionRegistry = _normalizedCanvasPositionRegistry.ToList();
            foreach (KeyValuePair<FrameworkElement, Tuple<NormalizedPosition, bool>> posRegistryEntry in positionRegistry)
            {
                var element = posRegistryEntry.Key;
                var pos = posRegistryEntry.Value.Item1;
                var size = posRegistryEntry.Value.Item2;
                SetUiElementNormalizedCanvasPosition(element, pos, size);
            }
        }
    }
}
