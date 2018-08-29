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

            CenterCardGroup = new StackCardGroup(this, new NormalizedPosition(50, 45), 0);
            LeftCenterCardGroup = new TaperedStackCardGroup(this, new NormalizedPosition(40, 45), 0);
            RightCenterCardGroup = new TaperedStackCardGroup(this, new NormalizedPosition(60, 45), 0);
            DiscardCardGroup = new AdjacentCardGroup(this, new NormalizedPosition(50, 65), 0);
            Player1CardGroup = new InteractiveAdjacentCardGroup(this, new NormalizedPosition(50, 90), 0);
            Player1StagingCardGroup = new StackCardGroup(this, new NormalizedPosition(40, 80), 0);
            Player2CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(10, 50), CardGroup.Orientation.DEGREES_90);
            Player2StagingCardGroup = new StackCardGroup(this, new NormalizedPosition(20, 40), CardGroup.Orientation.DEGREES_90);
            Player3CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(50, 10), CardGroup.Orientation.DEGREES_180);
            Player3StagingCardGroup = new StackCardGroup(this, new NormalizedPosition(60, 20), CardGroup.Orientation.DEGREES_180);
            Player4CardGroup = new AdjacentCardGroup(this, new NormalizedPosition(90, 50), CardGroup.Orientation.DEGREES_270);
            Player4StagingCardGroup = new StackCardGroup(this, new NormalizedPosition(80, 60), CardGroup.Orientation.DEGREES_270);

            // bind callbacks to UI elements
            player_creation_input.KeyDown += this.OnPlayerCreationInputKeyDown;
            player_bid_input.KeyDown += this.OnPlayerBidInputKeyDown;

            _gamePageController = new GamePageController(this, this.Dispatcher);
            _animationsCompletedHandlers = new Queue<Action>();

            // the size of a given card needs to only be fetched once and cached
            // all cards are the same size so the fetched size applies to all cards
            var imageSizeTask = GetCardImageSize();
            imageSizeTask.ContinueWith(sizeTask => this._cardBitmapSize = sizeTask.Result);

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
            var x = game_canvas_storyboard.GetCurrentState();
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
        public InteractiveAdjacentCardGroup Player1CardGroup { get; private set; }
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

        // callback that ensures that the storyboard clears out itself after each animation group completes
        private void OnGameCanvasStoryboardCompleted(object sender, object eventArgs)
        {
            var x = game_canvas_storyboard.GetCurrentState();
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
    }
}
