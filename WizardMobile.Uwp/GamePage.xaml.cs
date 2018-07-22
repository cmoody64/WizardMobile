using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using WizardMobile.Core;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp
{
    public sealed partial class GamePage : Page, IWizardFrontend
    {
        public GamePage()
        {
            this.InitializeComponent();

            UwpWizardFrontendProxy _proxyFrontend = new UwpWizardFrontendProxy(this);

            // since engine runs certain functionality on a separate worker thread, the calls that the engine make to the frontend
            // must be marshalled through the proxy frontend which implements multithreading protocol
            // this relationship does not extend two ways - this class can make calls directly to the engine
            // this is because the engine and this class both live on the same thread, the engine only does work on a different thread
            _engine = new WizardEngine(_proxyFrontend);
            _engine.Run();
        }

        private WizardEngine _engine;


        /*************** IWizardFrontend implementation ********************/
        public Task DisplayStartGame()
        {
            game_message_box.Text = "Game Starting";
            return Task.CompletedTask;
    
        }

        public Task DisplayStartRound(int roundNum)
        {
            game_message_box.Text = $"Round {roundNum} Starting";
            return Task.CompletedTask;
        }

        public Task DisplayStartTrick(int trickNum)
        {
            game_message_box.Text = $"Trick {trickNum} Starting";
            return Task.CompletedTask;
        }

        public Task DisplayTurnInProgress(Player player)
        {
            return Task.CompletedTask;
        }

        public Task DisplayTurnTaken(Card cardPlayed, Player player)
        {
            return Task.CompletedTask;
        }

        public Task DisplayPlayerBid(int bid, Player player)
        {
            return Task.CompletedTask;
        }

        public Task DisplayDealInProgess(int seconds)
        {
            var cardBackImage = GetCardImage(BACK_OF_CARD_KEY, new Point(0, 0), 45);
            game_canvas.Children.Add(cardBackImage);

            var cardBackAnimations = ComposeImageAnimations(cardBackImage, 2 /*duration*/, new Point(300, 300) /*destination*/,  1.25/*rotations*/);
            game_canvas_storyboard.Children.AddRange(cardBackAnimations);
            game_canvas_storyboard.Begin();
            game_canvas_storyboard.Completed += (s, e) =>
            {
                game_canvas_storyboard.Stop();
                game_canvas_storyboard.Children.Clear();
            };

            return Task.CompletedTask;
        }

        public Task DisplayDealDone(Player dealer, Card trumpCard)
        {
            Task.Yield();
            return Task.CompletedTask;
        }

        public Task DisplayTrickWinner(Player winner, Card winningCard)
        {
            throw new NotImplementedException();
        }

        public Task DisplayRoundScores(GameContext gameContext)
        {
            throw new NotImplementedException();
        }

        public Task DisplayBidOutcome(int roundNum, int totalBids)
        {
            throw new NotImplementedException();
        }

        public Task<Card> PromptPlayerCardSelection(Player player)
        {
            throw new NotImplementedException();
        }

        public Task<int> PromptPlayerBid(Player player)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> PromptPlayerCreation()
        {
            game_message_box.Text = "Player Creation";
            player_creation_input.Visibility = Visibility.Visible;

            TaskCompletionSource<List<string>> cardTaskCompletionSource = new TaskCompletionSource<List<string>>();
            var x = this.player_creation_input.Visibility;
            player_creation_input.KeyDown += (object sender, KeyRoutedEventArgs e) =>
            {
                var textInput = player_creation_input.Text;
                if (e.Key == Windows.System.VirtualKey.Enter && textInput.Length > 0)
                {
                    cardTaskCompletionSource.SetResult(new List<string>() { textInput });
                    player_creation_input.Visibility = Visibility.Collapsed;
                }

            };

            return cardTaskCompletionSource.Task;
        }

        // TODO remove getcardimage and animate image to separate ImageHelper class? how would that class know about the game_canvas_resources??
        // TODO implement z index??
        private Image GetCardImage(string cardImageKey, Point position, double angle)
        {
            var bitmapImage = game_canvas.Resources[cardImageKey] as BitmapImage;
            var image = new Image();

            image.Source = bitmapImage;

            Canvas.SetLeft(image, position.X);
            Canvas.SetTop(image, position.Y);

            image.RenderTransform = new RotateTransform { Angle = angle };
            image.RenderTransformOrigin = new Point(0.5, 0.5);

            return image;
        }

        private List<DoubleAnimation> ComposeImageAnimations(Image image, double duration, Point destination, double rotations = 0)
        {            
            var animations = new List<DoubleAnimation>();
            Point curLocation = new Point((double)image.GetValue(Canvas.LeftProperty), (double)image.GetValue(Canvas.TopProperty));

            // rotation animations
            if (rotations != 0 && image.RenderTransform != null && image.RenderTransform.GetType() == typeof(RotateTransform))
            {
                var rotationAnimation = new DoubleAnimation();
                double curAngle = ((RotateTransform)image.RenderTransform).Angle;
                var finalAngle = curAngle + 360 * rotations;
                rotationAnimation.From = curAngle;
                rotationAnimation.To = finalAngle;
                rotationAnimation.Duration = TimeSpan.FromSeconds(duration);

                Storyboard.SetTarget(rotationAnimation, image);
                Storyboard.SetTargetProperty(rotationAnimation, "(Image.RenderTransform).(RotateTransform.Angle)");
                animations.Add(rotationAnimation);                
            }

            // position animations (Canvas.Left and Canvas.Top)
            if (destination.X != curLocation.X)
            {
                var leftPropAnimation = new DoubleAnimation();
                leftPropAnimation.From = curLocation.X;
                leftPropAnimation.To = destination.X;
                leftPropAnimation.Duration = TimeSpan.FromSeconds(duration);

                Storyboard.SetTarget(leftPropAnimation, image);
                Storyboard.SetTargetProperty(leftPropAnimation, "(Canvas.Left)");

                leftPropAnimation.Completed += (sender, eventArgs) => leftPropAnimation.SetValue(Canvas.LeftProperty, destination.X);

                animations.Add(leftPropAnimation);
            }

            if(destination.Y != curLocation.Y)
            {
                var topPropAnimation = new DoubleAnimation();
                topPropAnimation.From = curLocation.Y;
                topPropAnimation.To = destination.Y;
                topPropAnimation.Duration = TimeSpan.FromSeconds(duration);                

                Storyboard.SetTarget(topPropAnimation, image);
                Storyboard.SetTargetProperty(topPropAnimation, "(Canvas.Top)");

                topPropAnimation.Completed += (sender, eventArgs) => topPropAnimation.SetValue(Canvas.TopProperty, destination.Y);

                animations.Add(topPropAnimation);
            }

            return animations;
        }

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
    }
}
