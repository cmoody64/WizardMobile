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
    public sealed partial class GamePage: IWizardComponentProvider, ICanvasFacade
    {
        private GamePageController _gamePageController;

        private void InitializeWizardComponentProvider()
        {
            animationQueue = new List<DoubleAnimation>();
            _gamePageController = new GamePageController(this, this.Dispatcher);
            _animationsCompletedHandlers = new Queue<Action>();

            CenterShuffleCardGroup = new StackCardGroup(this, new NormalizedPosition(50, 50), 0);
            LeftShuffleCardGroup = new StackCardGroup(this, new NormalizedPosition(40, 50), 0);
            RightShuffleCardGroup = new StackCardGroup(this, new NormalizedPosition(60, 50), 0);
            DiscardCardGroup = new AdjacentCardGroup(this, new NormalizedPosition(62, 50), 0);
            Player1CardGroup = new InteractiveAdjacentCardGroup(this, new NormalizedPosition(50, 90), 0);
            Player2CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(4, 50), CardGroup.Orientation.DEGREES_90);
            Player3CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(50, 3), CardGroup.Orientation.DEGREES_180);
            Player4CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(96, 50), CardGroup.Orientation.DEGREES_270);
            DeckCardGroup = new StackCardGroup(this, new NormalizedPosition(29, 50), 0);
            TrumpCardGroup = new StackCardGroup(this, new NormalizedPosition(37, 50), 0);
            CollapsedDiscardCardGroup = new StackCardGroup(this, GetRelativeNormalizedPosition(DiscardCardGroup.Origin, 0, 0), 0);
            OffScreenPlayer1CardGroup = new StackCardGroup(this, GetRelativeNormalizedPosition(Player1CardGroup.Origin, 0, 20), 0);
            OffScreenPlayer2CardGroup = new StackCardGroup(this, GetRelativeNormalizedPosition(Player2CardGroup.Origin, -20, 0), CardGroup.Orientation.DEGREES_90);
            OffScreenPlayer3CardGroup = new StackCardGroup(this, GetRelativeNormalizedPosition(Player3CardGroup.Origin, 0, -20), CardGroup.Orientation.DEGREES_180);
            OffScreenPlayer4CardGroup = new StackCardGroup(this, GetRelativeNormalizedPosition(Player4CardGroup.Origin, 20, 0), CardGroup.Orientation.DEGREES_270);

            // bind callbacks to UI elements
            player_creation_input.KeyDown += this.OnPlayerCreationInputKeyDown;
            player_bid_input.KeyDown += this.OnPlayerBidInputKeyDown;
            game_canvas_storyboard.Completed += OnAnimationsCompleted;

            // the size of a given card needs to only be fetched once and cached
            // all cards are the same size so the fetched size applies to all cards
            var imageSizeTask = GetCardImageSize();
            imageSizeTask.ContinueWith(sizeTask => this._cardBitmapSize = sizeTask.Result);

            // set position of UI elements using method that binds them to a responsive canvas position
            SetUiElementNormalizedCanvasPosition(player_creation_input, new NormalizedPosition(50, 50));
            SetUiElementNormalizedCanvasPosition(player_bid_input, new NormalizedPosition(62, 50));
            SetUiElementNormalizedCanvasPosition(player_bid_error_message, new NormalizedPosition(62, 58));

            SetUiElementNormalizedCanvasPosition(player1_name, GetRelativeNormalizedPosition(Player1CardGroup.Origin, -5, -20));
            SetUiElementNormalizedCanvasPosition(player1_status, GetRelativeNormalizedPosition(Player1CardGroup.Origin, -5, -16));
            SetUiElementNormalizedCanvasPosition(player2_name, GetRelativeNormalizedPosition(Player2CardGroup.Origin, 8, 0));
            SetUiElementNormalizedCanvasPosition(player2_status, GetRelativeNormalizedPosition(Player2CardGroup.Origin, 8, 4));
            SetUiElementNormalizedCanvasPosition(player3_name, GetRelativeNormalizedPosition(Player3CardGroup.Origin, -2, 12));
            SetUiElementNormalizedCanvasPosition(player3_status, GetRelativeNormalizedPosition(Player3CardGroup.Origin, -2, 16));
            SetUiElementNormalizedCanvasPosition(player4_name, GetRelativeNormalizedPosition(Player4CardGroup.Origin, -15, -6));
            SetUiElementNormalizedCanvasPosition(player4_status, GetRelativeNormalizedPosition(Player4CardGroup.Origin, -15, -2));
            SetUiElementNormalizedCanvasPosition(game_message_box, GetRelativeNormalizedPosition(CenterShuffleCardGroup.Origin, 0, -17));
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

        public void SetPlayerName(PlayerOrdinal player, string name) => PlayerEnumToNameElement(player).Text = name;

        public void SetPlayerStatus(PlayerOrdinal player, string status) => PlayerEnumToStatusElement(player).Text = status;

        public void SetAllPersonasVisibility(bool isVisible)
        {
            var visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            player1_name.Visibility = visibility;
            player1_status.Visibility = visibility;
            player2_name.Visibility = visibility;
            player2_status.Visibility = visibility;
            player3_name.Visibility = visibility;
            player3_status.Visibility = visibility;
            player4_name.Visibility = visibility;
            player4_status.Visibility = visibility;
        }

        public Task<bool> RunQueuedAnimations()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            game_canvas_storyboard.Children.AddRange(animationQueue);
            animationQueue.Clear();

            QueueAnimationsCompletedHandler(() =>
            {
                game_canvas_storyboard.Stop();

                foreach (DoubleAnimation anim in game_canvas_storyboard.Children)
                    ApplyAnimationEndValue(anim);

                game_canvas_storyboard.Children.Clear();

                taskCompletionSource.SetResult(true);
            });

            game_canvas_storyboard.Begin();
            return taskCompletionSource.Task;
        }

        private void QueueAnimationsCompletedHandler(Action handler) => _animationsCompletedHandlers.Enqueue(handler);
        private Queue<Action> _animationsCompletedHandlers;
        private void OnAnimationsCompleted(object sender, object eventArgs)
        {
            game_canvas_storyboard.Stop();

            foreach (DoubleAnimation anim in game_canvas_storyboard.Children)
                ApplyAnimationEndValue(anim);

            game_canvas_storyboard.Children.Clear();
            // run all queued animations completed handlers
            while (_animationsCompletedHandlers.Count > 0)
            {
                var handler = _animationsCompletedHandlers.Dequeue();
                handler();
            }
        }
        


        public void OnPlayerCreationInputEntered(Action<string> playerCreationInputEnteredHandler)
        {
            _playerCreationInputEnteredHandler = playerCreationInputEnteredHandler;
        }

        public void OnPlayerBidInputEntered(Action<int> playerBidEnteredHandler)
        {
            _playerBidEnteredHandler = playerBidEnteredHandler;
        }


        public StackCardGroup CenterShuffleCardGroup { get; private set; }
        public StackCardGroup LeftShuffleCardGroup { get; private set; }
        public StackCardGroup RightShuffleCardGroup { get; private set; }
        public AdjacentCardGroup DiscardCardGroup { get; private set; }
        public StackCardGroup CollapsedDiscardCardGroup { get; private set; }
        public InteractiveAdjacentCardGroup Player1CardGroup { get; private set; }
        public StackCardGroup Player1StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player2CardGroup { get; private set; }
        public StackCardGroup Player2StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player3CardGroup { get; private set; }
        public StackCardGroup Player3StagingCardGroup { get; private set; }
        public AdjacentCardGroup Player4CardGroup { get; private set; }
        public StackCardGroup Player4StagingCardGroup { get; private set; }
        public StackCardGroup DeckCardGroup { get; private set; }
        public StackCardGroup TrumpCardGroup { get; private set; }
        public StackCardGroup OffScreenPlayer1CardGroup { get; private set; }
        public StackCardGroup OffScreenPlayer2CardGroup { get; private set; }
        public StackCardGroup OffScreenPlayer3CardGroup { get; private set; }
        public StackCardGroup OffScreenPlayer4CardGroup { get; private set; }

        private Action<string> _playerCreationInputEnteredHandler = (string s) => {};
        private Action<int> _playerBidEnteredHandler = (int i) => {};

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
                    player_bid_input.Text = "";
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

        // for performance reasons, this is determined once during initialization and cached
        private Size _cardBitmapSize;
        private int _cardBitmapDecodePixelHeight;

        // since all card images are the same size, it is only necessary to read the size of a single image
        // to know the default card image size
        // NOTE this is not lazy loaded / cached because the screen size may change causing the image size to change
        private async Task<Size> GetCardImageSize()
        {
            return await GetImageSourceSize("back_of_card");
        }

        private async Task<Size> GetImageSourceSize(string imageResourceKey)
        {
            var bitmapSource = game_canvas.Resources[imageResourceKey] as BitmapImage;
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(bitmapSource.UriSource);
            var properties = await file.Properties.GetImagePropertiesAsync();
            var originalWidth = properties.Width;
            var originalHeight = properties.Height;

            double scaledHeight = _cardBitmapDecodePixelHeight;
            double scaleFactor = scaledHeight / originalHeight;
            double scaledWidth = scaleFactor * originalWidth;

            return new Size(scaledWidth, scaledHeight);
        }


        // ***************************************** Helpers ********************************************/
        private static NormalizedPosition GetRelativeNormalizedPosition(NormalizedPosition relativeTo, double xOffset, double yOffset)
        {
            return new NormalizedPosition(relativeTo.NormalizedX + xOffset, relativeTo.NormalizedY + yOffset);
        }

        private TextBlock PlayerEnumToNameElement(PlayerOrdinal player)
        {
            switch(player)
            {
                case PlayerOrdinal.PLAYER1: return player1_name;
                case PlayerOrdinal.PLAYER2: return player2_name;
                case PlayerOrdinal.PLAYER3: return player3_name;
                case PlayerOrdinal.PLAYER4: return player4_name;
                default: throw new ArgumentOutOfRangeException($"PlayerEnum value out of range");
            }
        }

        private TextBlock PlayerEnumToStatusElement(PlayerOrdinal player)
        {
            switch (player)
            {
                case PlayerOrdinal.PLAYER1: return player1_status;
                case PlayerOrdinal.PLAYER2: return player2_status;
                case PlayerOrdinal.PLAYER3: return player3_status;
                case PlayerOrdinal.PLAYER4: return player4_status;
                default: throw new ArgumentOutOfRangeException($"PlayerEnum value out of range");
            }
        }
    }
}
