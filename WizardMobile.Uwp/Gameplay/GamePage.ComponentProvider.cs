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
        private WizardEngine _engine;
        private void InitializeWizardComponentProvider()
        {
            _gamePageController = new GamePageController(this, this.Dispatcher);

            // bind callbacks to UI elements
            player_creation_input.KeyDown += this.OnPlayerCreationInputKeyDown;
            game_canvas_storyboard.Completed += this.OnGameCanvasStoryboardCompleted;
        }


        public Task<bool> DisplayShuffle()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            int shuffleAnimationCount = 6;

            for (int i = 0; i < shuffleAnimationCount; i++)
            {
                Point rightPosition = new Point(RIGHT_STACK_STARTING_POINT.X, RIGHT_STACK_STARTING_POINT.Y + 5 * i);
                Image rightCard = CreateCardImage(BACK_OF_CARD_KEY, rightPosition);
                var rightCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = rightCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    Duration = 0.2,
                    Delay = i * .1
                });
                game_canvas.Children.Add(rightCard);
                game_canvas_storyboard.Children.AddRange(rightCardAnimations);

                Point leftPosition = new Point(LEFT_STACK_STARTING_POINT.X, LEFT_STACK_STARTING_POINT.Y + 5 * i);
                Image leftCard = CreateCardImage(BACK_OF_CARD_KEY, leftPosition);
                var leftCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = leftCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    Duration = 0.2,
                    Delay = .05 + i * .1
                });
                game_canvas.Children.Add(leftCard);
                game_canvas_storyboard.Children.AddRange(leftCardAnimations);
            }

            game_canvas_storyboard.Begin();
            game_canvas_storyboard.Completed += (sender, eventArgs) => taskCompletionSource.SetResult(true);

            return taskCompletionSource.Task;
        }

        public Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            game_canvas.Children.Clear(); // clear out dummy cards from shuffle animation

            var faceUpHand = players.Find(player => player.GetType() == typeof(HumanPlayer)).Hand; // TODO this seems pretty hacky, better way to find human player at runtime?
            for (int i = 0; i < gameContext.CurRound.RoundNum; i++)
            {
                // iterate through all AI players and deal cards face  down
                for (int j = 0; j < players.Count - 1; j++)
                {
                    Image aiPlayercard = CreateCardImage(BACK_OF_CARD_KEY, CENTER_STACK_STARTING_POINT);
                    game_canvas.Children.Add(aiPlayercard);
                    game_canvas_storyboard.Children.AddRange(AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                    {
                        Image = aiPlayercard,
                        Destination = CENTER_STACK_STARTING_POINT,
                        Duration = 0.2,
                        Delay = 0.5 * i + .125 * j,
                        Rotations = j == 1 || j == 3 ? 3.25 : 3 // extra quarter rotation for positions 1 and 3 so that they end up at a 90 deg. angle
                    }));
                }

                // deal Human players hand face up
                Image humanPlayerCard = CreateCardImage(faceUpHand[i].ToString(), CENTER_STACK_STARTING_POINT);
                game_canvas.Children.Add(humanPlayerCard);
                game_canvas_storyboard.Children.AddRange(AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = humanPlayerCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    Duration = 0.2,
                    Delay = 0.5 * i + 1,
                    Rotations = 3
                }));

            }

            game_canvas_storyboard.Begin();
            game_canvas_storyboard.Completed += (sender, eventArgs) => taskCompletionSource.SetResult(true);

            return taskCompletionSource.Task;
        }


        /*************************** ICanvasFacade implementation *******************************/
        public void AddToCanvas(UniqueCard card, Point position, double orientationDegrees)
        {
            Image image = CreateCardImage(card, position);

            Canvas.SetLeft(image, position.X);
            Canvas.SetTop(image, position.Y);
            ((RotateTransform)image.RenderTransform).Angle = orientationDegrees;
            game_canvas.Children.Add(image);
        }

        public void RemoveFromCanvas(UniqueCard card)
        {
            Image elementToRemove = this.FindName(card.Id) as Image;
            game_canvas.Children.Remove(elementToRemove);
        }
         
        public void ReplaceCard(UniqueCard cardToReplace, string newCardName)
        {
            Image elementToReplace = this.FindName(cardToReplace.Id) as Image;
            elementToReplace.Source = game_canvas.Resources[cardToReplace.Name] as BitmapImage;
            cardToReplace.Name = newCardName;
        }

        public void QueueAnimation(DoubleAnimation animation)
        {
            animation.Completed += OnAnimationCompleted;
            animationQueue.Add(animation);
        }

        public void QueueAnimations(IEnumerable<DoubleAnimation> animations)
        {
            foreach (var animation in animations)
                QueueAnimation(animation);
        }

        private List<DoubleAnimation> animationQueue;
        private void OnAnimationCompleted(object sender, object args)
        {

            var animation = sender as DoubleAnimation;
            string imageName = animation.GetValue(Storyboard.TargetNameProperty) as string;
            Image image = FindName(imageName) as Image;

            var targetProperty = animation.GetValue(Storyboard.TargetPropertyProperty) as DependencyProperty;
            var animEndvalue = animation.To ?? 0.0;

            game_canvas_storyboard.Pause();

            // set the end property of the animation to the end property of the image
            if (targetProperty == Canvas.TopProperty)
                Canvas.SetTop(image, animEndvalue);
            else if (targetProperty == Canvas.LeftProperty)
                Canvas.SetLeft(image, animEndvalue);
            else if (targetProperty == RotateTransform.AngleProperty)
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

        public StackCardGroup CenterCardGroup { get; }
        public TaperedStackCardGroup LeftCenterCardGroup { get; }
        public TaperedStackCardGroup RightCenterCardGroup { get; }
        public AdjacentCardGroup DiscardCardGroup { get; }
        public AdjacentCardGroup Player1CardGroup { get; }
        public AdjacentCardGroup Player1StagingCardGroup { get; }
        public AdjacentCardGroup Player2CardGroup { get; }
        public AdjacentCardGroup Player2StagingCardGroup { get; }
        public AdjacentCardGroup Player3CardGroup { get; }
        public AdjacentCardGroup Player3StagingCardGroup { get; }
        public AdjacentCardGroup Player4CardGroup { get; }
        public AdjacentCardGroup Player4StagingCardGroup { get; }


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
        }

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
        private static readonly Point LEFT_STACK_STARTING_POINT = new Point(-300, 50);
        private static readonly Point RIGHT_STACK_STARTING_POINT = new Point(300, 50);
        private static readonly Point CENTER_STACK_STARTING_POINT = new Point(0, 50);

    }
}
