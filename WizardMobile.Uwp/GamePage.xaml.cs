using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            _engine = new WizardEngine(this);
            _engine.Run();
        }

        private WizardEngine _engine;

        public void DisplayStartGame()
        {
            game_message_box.Text = "Game Starting";
        }

        public void DisplayStartRound(int roundNum)
        {
            game_message_box.Text = $"Round {roundNum} Starting";
        }

        public void DisplayStartTrick(int trickNum)
        {
            game_message_box.Text = $"Trick {trickNum} Starting";
        }

        public void DisplayTurnInProgress(Player player)
        {
            throw new NotImplementedException();
        }

        public void DisplayTurnTaken(Card cardPlayed, Player player)
        {
            throw new NotImplementedException();
        }

        public void DisplayPlayerBid(int bid, Player player)
        {
            throw new NotImplementedException();
        }

        public void DisplayDealInProgess(int seconds)
        {
            throw new NotImplementedException();
        }

        public void DisplayDealDone(Player dealer, Card trumpCard)
        {
            throw new NotImplementedException();
        }

        public void DisplayTrickWinner(Player winner, Card winningCard)
        {
            throw new NotImplementedException();
        }

        public void DisplayRoundScores(GameContext gameContext)
        {
            throw new NotImplementedException();
        }

        public void DisplayBidOutcome(int roundNum, int totalBids)
        {
            throw new NotImplementedException();
        }

        public Card PromptPlayerCardSelection(Player player)
        {
            throw new NotImplementedException();
        }

        public int PromptPlayerBid(Player player)
        {
            throw new NotImplementedException();
        }

        public List<Player> PromptPlayerCreation()
        {
            game_message_box.Text = $"prompting player creation";
            return null;
        }
    }
}
