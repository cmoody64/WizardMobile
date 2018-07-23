using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public interface IWizardFrontend
    {
        Task DisplayStartGame();
        Task DisplayStartRound(int roundNum);
        Task DisplayStartTrick(int trickNum);
        Task DisplayTurnInProgress(Player player);
        Task DisplayTurnTaken(Card cardPlayed, Player player);
        Task DisplayPlayerBid(int bid, Player player);
        Task DisplayShuffle();
        Task DisplayDeal(GameContext gameContext, List<Player> players);
        Task DisplayTrickWinner(Player winner, Card winningCard);
        Task DisplayRoundScores(GameContext gameContext);
        Task DisplayBidOutcome(int roundNum, int totalBids);
        Task<Card> PromptPlayerCardSelection(Player player);
        Task<int> PromptPlayerBid(Player player);
        Task<List<string>> PromptPlayerCreation();
    }
}
