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

namespace WizardMobile.Uwp.Gameplay
{
    public sealed partial class GamePage: IWizardComponentProvider, ICanvasFacade
    {
        private GamePageController _gamePageController;

        private void InitializeWizardComponentProvider()
        {
            animationQueue = new List<DoubleAnimation>();

            CenterCardGroup = new StackCardGroup(this, new CanvasPosition(0, 0), 0);
            LeftCenterCardGroup = new TaperedStackCardGroup(this, new CanvasPosition(20, 0), 0);
            RightCenterCardGroup = new TaperedStackCardGroup(this, new CanvasPosition(30, 20), 0);
            DiscardCardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 60), 0);
            Player1CardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 90), 0);
            Player1StagingCardGroup = new StackCardGroup(this, new CanvasPosition(40, 80), 0);
            Player2CardGroup = new AdjacentCardGroup(this, new CanvasPosition(10, 50), 90);
            Player2StagingCardGroup = new StackCardGroup(this, new CanvasPosition(20, 40), 90);
            Player3CardGroup = new AdjacentCardGroup(this, new CanvasPosition(50, 10), 0);
            Player3StagingCardGroup = new StackCardGroup(this, new CanvasPosition(60, 20), 0);
            Player4CardGroup = new AdjacentCardGroup(this, new CanvasPosition(90, 50), 270);
            Player4StagingCardGroup = new StackCardGroup(this, new CanvasPosition(80, 60), 270);

            // bind callbacks to UI elements
            player_creation_input.KeyDown += this.OnPlayerCreationInputKeyDown;
            game_canvas_storyboard.Completed += this.OnGameCanvasStoryboardCompleted;

            _gamePageController = new GamePageController(this, this.Dispatcher);
        }


        /*************************** ICanvasFacade implementation *******************************/
        public void AddToCanvas(UniqueCard card, CanvasPosition canvasPositon, double orientationDegrees)
        {
            Point position = CanvasPositionToPoint(canvasPositon);
            Image image = CreateCardImage(card, position);
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
            elementToReplace.Source = game_canvas.Resources[cardToReplace.Name] as BitmapImage;
        }

        public void QueueAnimationRequest(AnimationRequest animationRequest)
        {
            Image animationTargetImage = FindName(animationRequest.ImageGuid) as Image;
            Point destination = CanvasPositionToPoint(animationRequest.Destination);
            var inflatedReq = AnimationHelper.InflateAnimationRequest(animationRequest, animationTargetImage, destination);
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

        public void BeginAnimations()
        {
            game_canvas_storyboard.Children.AddRange(animationQueue);
            animationQueue.Clear();
            game_canvas_storyboard.Begin();
        }

        // TODO implement z index??

        private Image CreateCardImage(UniqueCard card, Point position, double angle = 0)
        {
            var bitmapImage = game_canvas.Resources[card.Name] as BitmapImage;
            // setting the height scales the image and maintains aspect ratio
            bitmapImage.DecodePixelHeight = (int)(game_canvas.ActualHeight * .2);

            var image = new Image();
            image.Source = bitmapImage;
            image.Name = card.Id;


            Canvas.SetLeft(image, position.X);
            Canvas.SetTop(image, position.Y);

            image.RenderTransform = new RotateTransform { Angle = angle };
            image.RenderTransformOrigin = new Point(0.5, 0.5);

            return image;
        }

        public event Action<string> PlayerCreationInputEntered;
        public event EventHandler AnimationsCompleted;

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


        /************************************** event handlers **********************************************/
        private void OnPlayerCreationInputKeyDown(object sender, KeyRoutedEventArgs e)
        {
            var textInput = player_creation_input.Text;
            if (e.Key == Windows.System.VirtualKey.Enter && textInput.Length > 0)
            {
                this.PlayerCreationInputEntered(textInput);
            }
        }

        // callback that ensures that the storyboard clears out itself after each animation group completes
        private void OnGameCanvasStoryboardCompleted(object sender, object eventArgs)
        {
            game_canvas_storyboard.Stop();
            game_canvas_storyboard.Children.Clear();
            this.AnimationsCompleted(this, null);
        }



        /************************************** helpers **********************************************/
        private Point CanvasPositionToPoint(CanvasPosition pos)
        {
            double x = pos.NormalizedX * game_canvas.ActualWidth / CanvasPosition.NORMALIZED_WIDTH;
            double y = pos.NormalizedY * game_canvas.ActualHeight / CanvasPosition.NORMALIZED_HEIGHT;
            return new Point(x, y);
        }

        private static readonly Point LEFT_STACK_STARTING_POINT = new Point(-300, 50);
        private static readonly Point RIGHT_STACK_STARTING_POINT = new Point(300, 50);
        private static readonly Point CENTER_STACK_STARTING_POINT = new Point(0, 50);

    }
}
