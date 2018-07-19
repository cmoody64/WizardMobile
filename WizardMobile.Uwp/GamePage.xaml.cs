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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

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
            ////Image img = new Image();
            //////img.Width = bitmapImage.DecodePixelWidth = 80; //natural px width of image source
            //////                                               // don't need to set Height, system maintains aspect ratio, and calculates the other
            //////                                               // dimension, so long as one dimension measurement is provided

            ////BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/cards/back_of_card.png"));
            ////img.Source = bitmapImage;
            ////game_canvas.Children.Add(img);

            //var ellipse1 = new Ellipse();
            //ellipse1.Fill = new SolidColorBrush(Windows.UI.Colors.SteelBlue);
            //ellipse1.Width = 200;
            //ellipse1.Height = 200;
            //game_canvas.Children.Add(ellipse1);

            //Image img = new Image();
            ////img.Source = back_of_card;
            ////game_canvas.Children.Add(img);

            //BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/cards/back_of_card.png"));
            //img.Source = bitmapImage;
            //game_canvas.Children.Add(img);
            Image cardBack = new Image();
            cardBack.Source = game_canvas.Resources[BACK_OF_CARD_KEY] as BitmapImage;
            //cardBack.RenderTransformOrigin = new Point(0.5, 0.5);

            PlaneProjection cardBackProjection = new PlaneProjection();
            cardBackProjection.RotationZ = 45;
            cardBack.Projection = cardBackProjection;

            game_canvas.Children.Add(cardBack);
            Canvas.SetTop(cardBack, 50);
            Canvas.SetLeft(cardBack, 50);
            // Canvas.SetZIndex(cardBack, 1);

            DoubleAnimation cardBackAnimation = new DoubleAnimation();
            cardBackAnimation.From = 50;
            cardBackAnimation.To = 100;
            cardBackAnimation.Duration = TimeSpan.FromSeconds(5);

            Storyboard.SetTarget(cardBackAnimation, cardBack);
            Storyboard.SetTargetProperty(cardBackAnimation, "(Canvas.Left)");


            game_canvas_storyboard.Children.Add(cardBackAnimation);
            game_canvas_storyboard.Begin();

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

            var imagePlaneProjection = new PlaneProjection();
            imagePlaneProjection.RotationZ = angle;
            image.Projection = imagePlaneProjection;

            return image;
        }

        private List<DoubleAnimation> AnimateImage(Image image, int duration, Point destination, double rotations = 0)
        {            
            var animations = new List<DoubleAnimation>();
            Point curLocation = new Point((double)image.GetValue(Canvas.LeftProperty), (double)image.GetValue(Canvas.TopProperty));


            // position animations (Canvas.Left and Canvas.Top)
            if(destination.X != curLocation.X)
            {
                var leftPropAnimation = new DoubleAnimation();
                leftPropAnimation.From = curLocation.X;
                leftPropAnimation.To = destination.X;
                leftPropAnimation.Duration = TimeSpan.FromSeconds(duration);

                Storyboard.SetTarget(leftPropAnimation, image);
                Storyboard.SetTargetProperty(leftPropAnimation, "(Canvas.Left)");

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

                animations.Add(topPropAnimation);
            }

            // rotation animations
            if (rotations != 0 && image.Projection != null && image.Projection.GetType() == typeof(PlaneProjection))
            {
                var rotationAnimation = new DoubleAnimation();
                double curAngle = ((PlaneProjection)image.Projection).RotationZ;
                rotationAnimation.From = curAngle;
                rotationAnimation.To = curAngle + 360 * rotations;
                rotationAnimation.Duration = TimeSpan.FromSeconds(duration);

                Storyboard.SetTarget(rotationAnimation, image);
                Storyboard.SetTargetProperty(rotationAnimation, "(Image.Projection).(PlaneProjection.RotationZ)");

                animations.Add(rotationAnimation);
            }

            return animations;
        }

        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
    }
}
