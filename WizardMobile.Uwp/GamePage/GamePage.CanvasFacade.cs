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

            SetUiElementNormalizedCanvasPosition(image, canvasPositon, _cardBitmapSize);
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
                SetUiElementNormalizedCanvasPosition(imageToUpdate, canvasPositon, _cardBitmapSize);
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

        public void QueueAnimationRequest(NamedAnimationRequest animationRequest)
        {
            FrameworkElement targetElement = FindName(animationRequest.TargetElementName) as FrameworkElement
                ?? throw new ArgumentNullException($"{animationRequest.TargetElementName} didn't map to XAML element");
            Point? destination = null;
            if(animationRequest.Destination != null)
            {
                destination = DenormalizePosition(animationRequest.Destination, _cardBitmapSize);
                // this does not set the position, it only registers the destination position of the image for dynamic repositioning on size change
                RegisterElementCanvasPosition(targetElement, animationRequest.Destination, _cardBitmapSize);
            }
            var inflatedReq = AnimationHelper.InflateAnimationRequest(animationRequest, targetElement, destination);
            List<DoubleAnimation> animations = AnimationHelper.ComposeImageAnimations(inflatedReq);

            animationQueue.AddRange(animations);
        }

        public void QueueAnimationRequests(IEnumerable<NamedAnimationRequest> animations)
        {
            foreach (var animation in animations)
                QueueAnimationRequest(animation);
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
            var animEndvalue = animation.To ?? 0.0;

            // set the end property of the animation to the end property of the image
            if (targetProperty == AnimationPropertyStrings.CANVAS_TOP)
                Canvas.SetTop(element, animEndvalue);
            else if (targetProperty == AnimationPropertyStrings.CANVAS_LEFT)
                Canvas.SetLeft(element, animEndvalue);
            else if (targetProperty == AnimationPropertyStrings.ANGLE)
                ((RotateTransform)element.RenderTransform).Angle = animEndvalue;
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
        private void SetUiElementNormalizedCanvasPosition(UIElement element, NormalizedPosition position, Size? boundingRectSize = null)
        {
            var denormalizedPosition = DenormalizePosition(position, boundingRectSize);
            Canvas.SetLeft(element, denormalizedPosition.X);
            Canvas.SetTop(element, denormalizedPosition.Y);
            RegisterElementCanvasPosition(element, position, boundingRectSize);
        }

        private Dictionary<UIElement, Tuple<NormalizedPosition, Size?>> _normalizedCanvasPositionRegistry = new Dictionary<UIElement, Tuple<NormalizedPosition, Size?>>();
        private void RegisterElementCanvasPosition(UIElement el, NormalizedPosition pos, Size? size)
        {
            _normalizedCanvasPositionRegistry[el] = new Tuple<NormalizedPosition, Size?>(pos, size);
        }
        private bool UnregisterElementCanvasPosition(UIElement el)
        {
            return _normalizedCanvasPositionRegistry.Remove(el);
        }
        private void OnCanvasSizeChange(object sender, SizeChangedEventArgs args)
        {
            var positionRegistry = _normalizedCanvasPositionRegistry.ToList();
            foreach (KeyValuePair<UIElement, Tuple<NormalizedPosition, Size?>> posRegistryEntry in positionRegistry)
            {
                var element = posRegistryEntry.Key;
                var pos = posRegistryEntry.Value.Item1;
                var size = posRegistryEntry.Value.Item2;
                SetUiElementNormalizedCanvasPosition(element, pos, size);
            }
        }
    }
}
