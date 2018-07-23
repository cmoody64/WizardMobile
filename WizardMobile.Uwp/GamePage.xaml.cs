using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
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

            // bind callbacks to UI elements
            game_canvas_storyboard.Completed += this.OnGameCanvasStoryboard_Completed;
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

        public Task DisplayShuffle()
        {
            int shuffleAnimationCount = 6;
            Point leftStackStartingPoint = new Point(-300, 50);
            Point rightStackStartingPoint = new Point(300, 50);
            Point centerStackEndPoint = new Point(0, 50);
            for (int i = 0; i < shuffleAnimationCount; i++)
            {
                Point rightPosition = new Point(rightStackStartingPoint.X, rightStackStartingPoint.Y + 5 * i);
                Image rightCard = GetCardImage(BACK_OF_CARD_KEY, rightPosition);
                var rightCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = rightCard,
                    Destination = centerStackEndPoint,
                    DurationSeconds = 0.2,
                    DelaySeconds = i * .1
                });
                game_canvas.Children.Add(rightCard);
                game_canvas_storyboard.Children.AddRange(rightCardAnimations);

                Point leftPosition = new Point(leftStackStartingPoint.X, leftStackStartingPoint.Y + 5 * i);
                Image leftCard = GetCardImage(BACK_OF_CARD_KEY, leftPosition);
                var leftCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = leftCard,
                    Destination = centerStackEndPoint,
                    DurationSeconds = 0.2,
                    DelaySeconds = .05 + i * .1
                });
                game_canvas.Children.Add(leftCard);
                game_canvas_storyboard.Children.AddRange(leftCardAnimations);
            }

            game_canvas_storyboard.Begin();
            return Task.CompletedTask;
        }

        public Task DisplayDeal(GameContext gameContext, List<Player> players)
        {            
            for(int i = 0; i < gameContext.CurRound.RoundNum; i++)
            {

            }

            game_canvas_storyboard.Begin();

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
        /*************** IWizardFrontend implementation end ********************/


        // TODO implement z index??
        private Image GetCardImage(string cardImageKey, Point position, double angle = 0)
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

        // callback that ensures that the storyboard clears out itself after each animation group completes
        private void OnGameCanvasStoryboard_Completed(object sender, object eventArgs)
        {
            game_canvas_storyboard.Stop();
            game_canvas_storyboard.Children.Clear();
        }

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
    }
}
