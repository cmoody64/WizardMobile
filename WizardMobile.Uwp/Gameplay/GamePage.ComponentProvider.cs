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

namespace WizardMobile.Uwp.Gameplay
{
    public sealed partial class GamePage: IWizardComponentProvider, ICanvasFacade
    {
        private GamePageController _gamePageController;

        private void InitializeWizardComponentProvider()
        {
            animationQueue = new List<DoubleAnimation>();

            CenterCardGroup = new StackCardGroup(this, new CanvasPosition(50, 50), 0);
            LeftCenterCardGroup = new TaperedStackCardGroup(this, new CanvasPosition(40, 50), 0);
            RightCenterCardGroup = new TaperedStackCardGroup(this, new CanvasPosition(60, 50), 0);
            DiscardCardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 60), 0, true /*isFaceUp*/);
            Player1CardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 90), 0, true /*isFaceUp*/);
            Player1StagingCardGroup = new StackCardGroup(this, new CanvasPosition(40, 80), 0, true /*isFaceUp*/);
            Player2CardGroup = new AdjacentCardGroup(this, new CanvasPosition(10, 50), 90);
            Player2StagingCardGroup = new StackCardGroup(this, new CanvasPosition(20, 40), 90, true /*isFaceUp*/);
            Player3CardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 10), 0);
            Player3StagingCardGroup = new StackCardGroup(this, new CanvasPosition(60, 20), 0, true /*isFaceUp*/);
            Player4CardGroup = new AdjacentCardGroup(this, new CanvasPosition(90, 50), 270);
            Player4StagingCardGroup = new StackCardGroup(this, new CanvasPosition(80, 60), 270, true /*isFaceUp*/);

            // bind callbacks to UI elements
            player_creation_input.KeyDown += this.OnPlayerCreationInputKeyDown;
            player_bid_input.KeyDown += this.OnPlayerBidInputKeyDown;
            game_canvas_storyboard.Completed += this.OnGameCanvasStoryboardCompleted;
            game_canvas.Loaded += (sender, args) => _cardBitmapDecodePixelHeight = (int)(game_canvas.ActualHeight * .2);

            _gamePageController = new GamePageController(this, this.Dispatcher);
            _animationsCompletedHandlers = new Queue<Action>();

            // the size of a given card needs to only be fetched once and cached
            // all cards are the same size so the fetched size applies to all cards
            var imageSizeTask = GetImageSourceSize("back_of_card");
            imageSizeTask.ContinueWith(sizeTask => this._cardBitmapSize = sizeTask.Result);            
        }


        /*************************** ICanvasFacade implementation *******************************/
        public void AddToCanvas(UniqueCard card, CanvasPosition canvasPositon, double orientationDegrees)
        {
            Image image = CreateCardImage(card);
            Point position = CanvasPositionToPoint(canvasPositon, _cardBitmapSize);
            SetCardImagePosition(image, position);
            SetCardImageAngle(image, orientationDegrees);
            game_canvas.Children.Add(image);
        }

        public void RemoveFromCanvas(UniqueCard card)
        {
            Image elementToRemove = this.FindName(card.Id) as Image;
            game_canvas.Children.Remove(elementToRemove);
        }
         
        public void ReplaceCardBitmap(UniqueCard cardToReplace, string newCardName)
        {
            Image elementToReplace = this.FindName(cardToReplace.Id) as Image;
            var bitmapImage = RetrieveCardBitmap(cardToReplace.Name);
            elementToReplace.Source = bitmapImage;
        }

        public void QueueAnimationRequest(AnimationRequest animationRequest)
        {
            Image targetImage = FindName(animationRequest.ImageGuid) as Image;
            Point destination = CanvasPositionToPoint(animationRequest.Destination, _cardBitmapSize);
            var inflatedReq = AnimationHelper.InflateAnimationRequest(animationRequest, targetImage, destination);
            List<DoubleAnimation> animations = AnimationHelper.ComposeImageAnimations(inflatedReq);

            // make sure each animation is properly cleaned up by assigning the completed handler 
            animations.ForEach(animation => animation.Completed += OnAnimationCompleted);
            
            animationQueue.AddRange(animations);
        }

        public void QueueAnimationRequests(IEnumerable<AnimationRequest> animations)
        {
            foreach (var animation in animations)
                QueueAnimationRequest(animation);
        }

        private List<DoubleAnimation> animationQueue;
        private void OnAnimationCompleted(object sender, object args)
        {
            
            var animation = sender as DoubleAnimation;
            var imageName = Storyboard.GetTargetName(animation);
            var targetProperty = Storyboard.GetTargetProperty(animation);

            var image = FindName(imageName) as Image;            
            var animEndvalue = animation.To ?? 0.0;

            game_canvas_storyboard.Pause();

            // set the end property of the animation to the end property of the image
            if (targetProperty == "(Canvas.Top)")
                Canvas.SetTop(image, animEndvalue);
            else if (targetProperty == "(Canvas.Left)")
                Canvas.SetLeft(image, animEndvalue);
            else if (targetProperty == "(Image.RenderTransform).(RotateTransform.Angle)")
                ((RotateTransform)image.RenderTransform).Angle = animEndvalue;

            // remove the animation from the storyboard
            game_canvas_storyboard.Children.Remove(animation);

            game_canvas_storyboard.Resume();
        }


        /*************************** IWizardComponentProvider implementation *******************************/
        public void SetMessageBoxText(string message)
        {
            game_message_box.Text = message;
        }

        public void SetPlayerCreationInputVisibility(bool isVisible)
        {
            player_creation_input.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetHumanPlayerBidInputVisibility(bool isVisible)
        {
            player_bid_input.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void BeginAnimations()
        {
            game_canvas_storyboard.Children.AddRange(animationQueue);
            animationQueue.Clear();
            game_canvas_storyboard.Begin();
        }


        public void OnPlayerCreationInputEntered(Action<string> playerCreationInputEnteredHandler)
        {
            _playerCreationInputEnteredHandler = playerCreationInputEnteredHandler;
        }

        public void OnPlayerBidInputEntered(Action<int> playerBidEnteredHandler)
        {
            _playerBidEnteredHandler = playerBidEnteredHandler;
        }

        public void QueueAnimationsCompletedHandler(Action action)
        {
            _animationsCompletedHandlers.Enqueue(action);
        }

        public StackCardGroup CenterCardGroup { get; private set; }
        public TaperedStackCardGroup LeftCenterCardGroup { get; private set; }
        public TaperedStackCardGroup RightCenterCardGroup { get; private set; }
        public AdjacentCardGroup DiscardCardGroup { get; private set; }
        public AdjacentCardGroup Player1CardGroup { get; private set; }
        public StackCardGroup Player1StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player2CardGroup { get; private set; }
        public StackCardGroup Player2StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player3CardGroup { get; private set; }
        public StackCardGroup Player3StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player4CardGroup { get; private set; }
        public StackCardGroup Player4StagingCardGroup { get; private set; }

        private Action<string> _playerCreationInputEnteredHandler = (string s) => {};
        private Action<int> _playerBidEnteredHandler = (int i) => {};
        private Queue<Action> _animationsCompletedHandlers;


        /************************************** event handlers **********************************************/
        private void OnPlayerCreationInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textInput = player_creation_input.Text;
            if (e.Key == Windows.System.VirtualKey.Enter && textInput.Length > 0)
            {
                this._playerCreationInputEnteredHandler(textInput);
            }
        }

        private void OnPlayerBidInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textInput = player_bid_input.Text;
            if (e.Key == Windows.System.VirtualKey.Enter && textInput.Length > 0)
            {
                if(int.TryParse(textInput, out int bid))
                {
                    player_bid_input.Visibility = Visibility.Collapsed;
                    player_bid_error_message.Visibility = Visibility.Collapsed;
                    this._playerBidEnteredHandler(bid);
                }
                else
                {
                    player_bid_error_message.Visibility = Visibility.Visible;
                }
                
            }
        }

        // callback that ensures that the storyboard clears out itself after each animation group completes
        private void OnGameCanvasStoryboardCompleted(object sender, object eventArgs)
        {
            game_canvas_storyboard.Stop();
            game_canvas_storyboard.Children.Clear();
            // run all queued animations completed handlers
            while (_animationsCompletedHandlers.Count > 0)
            {
                var handler = _animationsCompletedHandlers.Dequeue();
                handler();
            }              
        }


        /************************************** helpers **********************************************/
        // translates a high level normalized canvas position (0 -> 100) to actual canvas position (0 -> actual dimension)
        // NOTE optionally takes into acount image size so that it seems like the image is centered on pos
        private Point CanvasPositionToPoint(CanvasPosition pos, Size? imageSize = null)
        {
            double x = pos.NormalizedX * game_canvas.ActualWidth / CanvasPosition.NORMALIZED_WIDTH;
            double y = pos.NormalizedY * game_canvas.ActualHeight / CanvasPosition.NORMALIZED_HEIGHT;

            // optionally shift x and y so that it seems like the point is centered around a given image
            if(imageSize.HasValue)
            {
                x -= imageSize.Value.Width / 2;
                y -= imageSize.Value.Height / 2;
            }

            return new Point(x, y);
        }

        // for performance reasons, this is determined once during initialization and cached
        private Size _cardBitmapSize;
        private int _cardBitmapDecodePixelHeight;


        // TODO implement z index??
        private Image SetupCardImage(UniqueCard card, Point position, double angle = 0)
        {
            var image = CreateCardImage(card);
            SetCardImagePosition(image, position);
            SetCardImageAngle(image, angle);

            return image;
        }

        private Image CreateCardImage(UniqueCard card)
        {
            var bitmapImage = RetrieveCardBitmap(card.Name);
            var image = new Image();
            image.Source = bitmapImage;
            image.Name = card.Id;
            return image;
        }

        private BitmapImage RetrieveCardBitmap(string bitmapKey)
        {
            var bitmapImage = game_canvas.Resources[bitmapKey] as BitmapImage;
            // scale down and maintain aspect ratio
            bitmapImage.DecodePixelHeight = (int)(game_canvas.ActualHeight * .2);
            return bitmapImage;
        }

        private static void SetCardImagePosition(Image cardImage, Point position)
        {
            Canvas.SetLeft(cardImage, position.X);
            Canvas.SetTop(cardImage, position.Y);
        }

        private static void SetCardImageAngle(Image cardImage, double angle)
        {
            cardImage.RenderTransform = new RotateTransform { Angle = angle };
            cardImage.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        private async Task<Size> GetImageSourceSize(string cardKey)
        {
            var bitmapSource = game_canvas.Resources[cardKey] as BitmapImage;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(bitmapSource.UriSource);
            var properties = await file.Properties.GetImagePropertiesAsync();
            var originalWidth = properties.Width;
            var originalHeight = properties.Height;

            double scaledHeight = _cardBitmapDecodePixelHeight;
            double scaleFactor = scaledHeight / originalHeight;
            double scaledWidth = scaleFactor * originalWidth;

            return new Size(scaledWidth, scaledHeight);
        }
    }
}
