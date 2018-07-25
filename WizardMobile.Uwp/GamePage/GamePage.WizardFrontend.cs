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

namespace WizardMobile.Uwp
{
    public sealed partial class GamePage: IWizardFrontend
    {
        private WizardEngine _engine;
        private void InitializeWizardFrontend()
        {
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

        /*************** IWizardFrontend implementation ********************/
        public Task<bool> DisplayStartGame()
        {
            game_message_box.Text = "Game Starting";
            return Task.FromResult(true);

        }

        public Task<bool> DisplayStartRound(int roundNum)
        {
            game_message_box.Text = $"Round {roundNum} Starting";
            return Task.FromResult(true);
        }

        public Task<bool> DisplayStartTrick(int trickNum)
        {
            game_message_box.Text = $"Trick {trickNum} Starting";
            return Task.FromResult(true);
        }

        public Task<bool> DisplayTurnInProgress(Player player)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DisplayTurnTaken(Card cardPlayed, Player player)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DisplayPlayerBid(int bid, Player player)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DisplayShuffle()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            int shuffleAnimationCount = 6;

            for (int i = 0; i < shuffleAnimationCount; i++)
            {
                Point rightPosition = new Point(RIGHT_STACK_STARTING_POINT.X, RIGHT_STACK_STARTING_POINT.Y + 5 * i);
                Image rightCard = GetCardImage(BACK_OF_CARD_KEY, rightPosition);
                var rightCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = rightCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    DurationSeconds = 0.2,
                    DelaySeconds = i * .1
                });
                game_canvas.Children.Add(rightCard);
                game_canvas_storyboard.Children.AddRange(rightCardAnimations);

                Point leftPosition = new Point(LEFT_STACK_STARTING_POINT.X, LEFT_STACK_STARTING_POINT.Y + 5 * i);
                Image leftCard = GetCardImage(BACK_OF_CARD_KEY, leftPosition);
                var leftCardAnimations = AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = leftCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    DurationSeconds = 0.2,
                    DelaySeconds = .05 + i * .1
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
                    Image aiPlayercard = GetCardImage(BACK_OF_CARD_KEY, CENTER_STACK_STARTING_POINT);
                    game_canvas.Children.Add(aiPlayercard);
                    game_canvas_storyboard.Children.AddRange(AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                    {
                        Image = aiPlayercard,
                        Destination = CENTER_STACK_STARTING_POINT,
                        DurationSeconds = 0.2,
                        DelaySeconds = 0.5 * i + .125 * j,
                        Rotations = j == 1 || j == 3 ? 3.25 : 3 // extra quarter rotation for positions 1 and 3 so that they end up at a 90 deg. angle
                    }));
                }

                // deal Human players hand face up
                Image humanPlayerCard = GetCardImage(faceUpHand[i].ToString(), CENTER_STACK_STARTING_POINT);
                game_canvas.Children.Add(humanPlayerCard);
                game_canvas_storyboard.Children.AddRange(AnimationHelper.ComposeImageAnimations(new ImageAnimationRequest
                {
                    Image = humanPlayerCard,
                    Destination = CENTER_STACK_STARTING_POINT,
                    DurationSeconds = 0.2,
                    DelaySeconds = 0.5 * i + 1,
                    Rotations = 3
                }));

            }

            game_canvas_storyboard.Begin();
            game_canvas_storyboard.Completed += (sender, eventArgs) => taskCompletionSource.SetResult(true);

            return taskCompletionSource.Task;
        }

        public Task<bool> DisplayTrickWinner(Player winner, Card winningCard)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisplayRoundScores(GameContext gameContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisplayBidOutcome(int roundNum, int totalBids)
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


        // TODO implement z index??        
        //private Image GetCardImage(string cardImageKey, Point position, double angle = 0)
        //{
        //    var bitmapImage = game_canvas.Resources[cardImageKey] as BitmapImage;
        //    var image = new Image();

        //    image.Source = bitmapImage;

        //    Canvas.SetLeft(image, position.X);
        //    Canvas.SetTop(image, position.Y);

        //    image.RenderTransform = new RotateTransform { Angle = angle };
        //    image.RenderTransformOrigin = new Point(0.5, 0.5);

        //    return image;
        //}

        // callback that ensures that the storyboard clears out itself after each animation group completes
        private void OnGameCanvasStoryboard_Completed(object sender, object eventArgs)
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
