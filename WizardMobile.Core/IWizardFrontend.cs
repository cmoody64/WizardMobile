using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public interface IWizardFrontend
    {
        void DisplayStartGame();
        void DisplayStartRound(int roundNum);
        void DisplayStartTrick(int trickNum);
        void DisplayTurnInProgress(Player player);
        void DisplayTurnTaken(Card cardPlayed, Player player);
        void DisplayPlayerBid(int bid, Player player);
        void DisplayDealInProgess(int seconds);
        void DisplayDealDone(Player dealer, Card trumpCard);
        void DisplayTrickWinner(Player winner, Card winningCard);
        void DisplayRoundScores(GameContext gameContext);
        void DisplayBidOutcome(int roundNum, int totalBids);
        Card PromptPlayerCardSelection(Player player);
        int PromptPlayerBid(Player player);
        List<Player> PromptPlayerCreation();
    }
}
