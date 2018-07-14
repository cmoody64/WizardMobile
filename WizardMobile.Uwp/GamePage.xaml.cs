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
using Windows.UI.Xaml.Navigation;
using WizardMobile.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WizardMobile.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            throw new NotImplementedException();
        }

        public Task DisplayDealDone(Player dealer, Card trumpCard)
        {
            throw new NotImplementedException();
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

        public Task<List<Player>> PromptPlayerCreation()
        {
            throw new NotImplementedException();
        }
    }
}
