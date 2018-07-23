using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core
{
    public interface IWizardFrontend
    {
        Task<bool> DisplayStartGame();
        Task<bool> DisplayStartRound(int roundNum);
        Task<bool> DisplayStartTrick(int trickNum);
        Task<bool> DisplayTurnInProgress(Player player);
        Task<bool> DisplayTurnTaken(Card cardPlayed, Player player);
        Task<bool> DisplayPlayerBid(int bid, Player player);
        Task<bool> DisplayShuffle();
        Task<bool> DisplayDeal(GameContext gameContext, List<Player> players);
        Task<bool> DisplayTrickWinner(Player winner, Card winningCard);
        Task<bool> DisplayRoundScores(GameContext gameContext);
        Task<bool> DisplayBidOutcome(int roundNum, int totalBids);
        Task<Card> PromptPlayerCardSelection(Player player);
        Task<int> PromptPlayerBid(Player player);
        Task<List<string>> PromptPlayerCreation();
    }
}
