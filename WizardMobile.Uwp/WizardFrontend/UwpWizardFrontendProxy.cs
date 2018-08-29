using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Core;
using Windows.UI.Core;
using WizardMobile.Uwp.Common;
using WizardMobile.Uwp.GamePage;

// this class acts as an adapater that exposes a synchronous interface to the engine but implements UWP multithreading protocol under the hood
// since the engine and the real front end (GamePage.xaml.cs) reside on different threads, this class serves an in between
namespace WizardMobile.Uwp.WizardFrontend
{
    class UwpWizardFrontendProxy: IWizardFrontend
    {
        public UwpWizardFrontendProxy(GamePageController principalFrontend, CoreDispatcher dispatcher)
        {
            _principalFrontend = principalFrontend;
            _dispatcher = dispatcher;
        }

        // the proxy passes all calls through to the actual frontend implementation (GamePage)
        private GamePageController _principalFrontend;
        private CoreDispatcher _dispatcher;

        public async Task<bool> DisplayStartGame()
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayStartGame());
        }

        public async Task<bool> DisplayStartRound(int roundNum)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayStartRound(roundNum));
        }

        public async Task<bool> DisplayEndRound(int roundNum)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayEndRound(roundNum));
        }

        public async Task<bool> DisplayStartTrick(int trickNum)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayStartTrick(trickNum));
        }

        public async Task<bool> DisplayEndTrick(int trickNum)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayEndTrick(trickNum));
        }

        public async Task<bool> DisplayTrumpCardSelected(Card trumpCard)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayTrumpCardSelected(trumpCard));
        }

        public async Task<bool> DisplayTurnInProgress(Player player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayTurnInProgress(player));
        }

        public async Task<bool> DisplayTurnTaken(Card cardPlayed, Player player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayTurnTaken(cardPlayed, player));
        }

        public async Task<bool> DisplayPlayerBid(int bid, Player player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayPlayerBid(bid, player));
        }

        public async Task<bool> DisplayShuffle(IReadonlyDeck deck)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayShuffle(deck));
        }

        public async Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayDeal(gameContext, players));
        }

        public async Task<bool> DisplayTrickWinner(Player winner, Card winningCard)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayTrickWinner(winner, winningCard));
        }

        public async Task<bool> DisplayRoundScores(GameContext gameContext)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayRoundScores(gameContext));
        }

        public async Task<bool> DisplayBidOutcome(int roundNum, int totalBids)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.DisplayBidOutcome(roundNum, totalBids));
        }

        public async Task<Card> PromptPlayerCardSelection(HumanPlayer player, IReadOnlyList<Card> playableCards)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerCardSelection(player, playableCards));
        }

        public async Task<int> PromptPlayerBid(HumanPlayer player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerBid(player));
        }

        public async Task<List<string>> PromptPlayerCreation()
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerCreation());
        }
    }
}
