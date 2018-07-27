using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using WizardMobile.Core;
using WizardMobile.Uwp.Gameplay.Game

namespace WizardMobile.Uwp.Gameplay
{
    class GamePageController: IWizardFrontend
    {
        public GamePageController(IWizardComponentProvider componentProvider, CoreDispatcher uiDispatcher)
        {
            // since engine runs certain functionality on a separate worker thread, the calls that the engine make to the frontend
            // must be marshalled through the proxy frontend which implements multithreading protocol
            // this relationship does not extend two ways - this class can make calls directly to the engine
            // this is because the engine and this class both live on the same thread, the engine only does work on a different thread
            UwpWizardFrontendProxy _proxyFrontend = new UwpWizardFrontendProxy(this, uiDispatcher);
            _engine = new WizardEngine(_proxyFrontend);
            _engine.Run();
        }

        private IWizardComponentProvider _componentProvider;
        private WizardEngine _engine;

        /*************** IWizardFrontend implementation ********************/
        public Task<bool> DisplayStartGame()
        {
            _componentProvider.SetMessageBoxText("Game Starting");
            return Task.FromResult(true);

        }

        public Task<bool> DisplayStartRound(int roundNum)
        {
            _componentProvider.SetMessageBoxText($"Round {roundNum} Starting");
            return Task.FromResult(true);
        }

        public Task<bool> DisplayStartTrick(int trickNum)
        {
            _componentProvider.SetMessageBoxText($"Trick {trickNum} Starting");
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
            var animationSession = _componentProvider.CreateAnimationSession();
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            const int shuffleCount = 5;
            animationSession.AddCards(Enumerable.Repeat(BACK_OF_CARD_KEY, shuffleCount), GamePage.CardLocations.LEFT_CENTER);
            animationSession.AddCards(Enumerable.Repeat(BACK_OF_CARD_KEY, shuffleCount), GamePage.CardLocations.RIGHT_CENTER);

            for (int i = 0; i < shuffleCount; i++)
            {
                animationSession.TransferCard(BACK_OF_CARD_KEY, GamePage.CardLocations.LEFT_CENTER, GamePage.CardLocations.RIGHT_CENTER, 0.3 /*delay*/);
            }

            animationSession.Completed += (sender, eventArgs) => taskCompletionSource.SetResult(true);
            animationSession.Begin();

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
