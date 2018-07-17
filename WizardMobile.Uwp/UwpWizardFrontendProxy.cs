using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WizardMobile.Core;
using Windows.UI.Core;
using WizardMobile.Uwp.Common;

// this class acts as an adapater that exposes a synchronous interface to the engine but implements UWP multithreading protocol under the hood
// since the engine and the real front end (GamePage.xaml.cs) reside on different threads, this class serves an in between
namespace WizardMobile.Uwp
{
    class UwpWizardFrontendProxy: IWizardFrontend
    {
        public UwpWizardFrontendProxy(GamePage principalFrontend)
        {
            _principalFrontend = principalFrontend;
            _dispatcher = _principalFrontend.Dispatcher;
        }

        // the proxy passes all calls through to the actual frontend implementation (GamePage)
        private GamePage _principalFrontend;
        private CoreDispatcher _dispatcher;

        public async Task DisplayStartGame()
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayStartGame());
        }

        public async Task DisplayStartRound(int roundNum)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayStartRound(roundNum));
        }

        public async Task DisplayStartTrick(int trickNum)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayStartTrick(trickNum));
        }

        public async Task DisplayTurnInProgress(Player player)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayTurnInProgress(player));
        }

        public async Task DisplayTurnTaken(Card cardPlayed, Player player)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayTurnTaken(cardPlayed, player));
        }

        public async Task DisplayPlayerBid(int bid, Player player)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayPlayerBid(bid, player));
        }

        public async Task DisplayDealInProgess(int seconds)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayDealInProgess(seconds));
        }

        public async Task DisplayDealDone(Player dealer, Card trumpCard)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayDealDone(dealer, trumpCard));
        }

        public async Task DisplayTrickWinner(Player winner, Card winningCard)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayTrickWinner(winner, winningCard));
        }

        public async Task DisplayRoundScores(GameContext gameContext)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayRoundScores(gameContext));
        }

        public async Task DisplayBidOutcome(int roundNum, int totalBids)
        {
            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => _principalFrontend.DisplayBidOutcome(roundNum, totalBids));
        }

        public async Task<Card> PromptPlayerCardSelection(Player player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerCardSelection(player));
        }

        public async Task<int> PromptPlayerBid(Player player)
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerBid(player));
        }

        public async Task<List<string>> PromptPlayerCreation()
        {
            return await _dispatcher.RunAsyncWithResult(CoreDispatcherPriority.Normal, async () => await _principalFrontend.PromptPlayerCreation());
        }

    }
}
