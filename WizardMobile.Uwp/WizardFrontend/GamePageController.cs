using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using WizardMobile.Core;
using WizardMobile.Uwp.Common;
using WizardMobile.Uwp.GamePage;

namespace WizardMobile.Uwp.WizardFrontend
{
    class GamePageController: IWizardFrontend
    {
        public GamePageController(IWizardComponentProvider componentProvider, CoreDispatcher uiDispatcher)
        {
            _componentProvider = componentProvider;
            _playerCardGroups = new Dictionary<string, CardGroup>();
            _playerOrdinals = new Dictionary<string, PlayerOrdinal>();

            // since engine runs certain functionality on a separate worker thread, the calls that the engine make to the frontend
            // must be marshalled through the proxy frontend which implements multithreading protocol
            // this relationship does not extend two ways - this class can make calls directly to the engine
            // this is because the engine and this class both live on the same thread, the engine only does work on a different thread
            UwpWizardFrontendProxy _proxyFrontend = new UwpWizardFrontendProxy(this, uiDispatcher);
            _engine = new WizardEngine(_proxyFrontend);
            _engine.Run();
        }

        private IWizardComponentProvider _componentProvider;
        private WizardEngine _engine;
        private Dictionary<string, CardGroup> _playerCardGroups; // maps player names to the corresponding hand cardGroup
        private Dictionary<string, PlayerOrdinal> _playerOrdinals; // maps player names to PlayerOrdinals used in the componentProvider

        /*************** IWizardFrontend implementation ********************/
        public async Task<bool> DisplayStartGame()
        {
            _componentProvider.SetMessageBoxText("Game Starting");
            _componentProvider.SetAllPersonasVisibility(true);
            await Task.Delay(2000);            
            return true;

        }

        public async Task<bool> DisplayStartRound(int roundNum)
        {
            _componentProvider.SetMessageBoxText($"Round {roundNum} Starting");      
            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayEndRound(int roundNum)
        {
            _componentProvider.SetMessageBoxText("Round Over");
            _componentProvider.RightCenterCardGroup.RemoveAll();
            _componentProvider.CenterCardGroup.RemoveAll();

            // clear all player statuses
            _componentProvider.SetPlayerStatus(PlayerOrdinal.PLAYER1, "");
            _componentProvider.SetPlayerStatus(PlayerOrdinal.PLAYER2, "");
            _componentProvider.SetPlayerStatus(PlayerOrdinal.PLAYER3, "");
            _componentProvider.SetPlayerStatus(PlayerOrdinal.PLAYER4, "");

            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayStartTrick(int trickNum)
        {
            _componentProvider.SetMessageBoxText($"Trick {trickNum} Starting");
            await Task.Delay(1000);
            return true;
        }

        public Task<bool> DisplayEndTrick(int trickNum)
        {
            // clean up trick and bid statuses
            _componentProvider.DiscardCardGroup.RemoveAll();
            return Task.FromResult(true);
        }

        public Task<bool> DisplayTrumpCardSelected(Card trumpCard)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
            _componentProvider.CenterCardGroup.Transfer
            (
                trumpCard,
                _componentProvider.RightCenterCardGroup,
                new AnimationBehavior { Duration = 0.3 }
            );
            _componentProvider.QueueAnimationsCompletedHandler(() =>
            {
                _componentProvider.RightCenterCardGroup.Flip(trumpCard);
                taskCompletionSource.SetResult(true);
            });
            _componentProvider.BeginAnimations();
            return taskCompletionSource.Task;
        }

        public async Task<bool> DisplayTurnInProgress(Player player)
        {
            _componentProvider.SetMessageBoxText($"{player.Name}'s turn");
            await Task.Delay(1000);
            return true;
        }

        public Task<bool> DisplayTurnTaken(Card cardPlayed, Player player)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            var sourceCardGroup = _playerCardGroups[player.Name];

            if (player is AIPlayer)
                sourceCardGroup.Flip(cardPlayed);

            sourceCardGroup.Transfer
            (
                cardPlayed,
                _componentProvider.DiscardCardGroup,
                new AnimationBehavior() { Duration = 0.3, Rotations = 3 }
            );

            _componentProvider.QueueAnimationsCompletedHandler(() => taskCompletionSource.SetResult(true));
            _componentProvider.BeginAnimations();
            return taskCompletionSource.Task;
        }

        public async Task<bool> DisplayPlayerBid(int bid, Player player)
        {
            _componentProvider.SetMessageBoxText($"{player.Name} bids {bid}");
            _componentProvider.SetPlayerStatus(_playerOrdinals[player.Name], $"0 / {bid}");
            await Task.Delay(1000);
            return true;
        }

        public Task<bool> DisplayShuffle(IReadonlyDeck deckToShuffle)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            // add cards alternating to left and right center stacks
            for (int i = 0; i < deckToShuffle.Cards.Count; i += 2)
            {
                var leftCard = deckToShuffle.Cards[i];
                var rightCard = deckToShuffle.Cards[i + 1];

                _componentProvider.LeftCenterCardGroup.Add(leftCard);
                _componentProvider.RightCenterCardGroup.Add(rightCard);                

                _componentProvider.LeftCenterCardGroup.Transfer
                (
                    leftCard,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.025 * i, Duration = 0.1 }
                );

                _componentProvider.RightCenterCardGroup.Transfer
                (
                    rightCard,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.025 * i + .0125, Duration = 0.1 }
                );
            }

            _componentProvider.QueueAnimationsCompletedHandler(() =>
            {
                // complete the task
                taskCompletionSource.SetResult(true);
            });
            _componentProvider.BeginAnimations();

            return taskCompletionSource.Task;
        }

        public async Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            for (int dealStage = 0; dealStage < gameContext.CurRound.RoundNum; dealStage++)
            {
                await DisplaySingleDealStage(gameContext, players, dealStage);
            }

            _componentProvider.Player1CardGroup.FlipAll();

            return true;
        }

        private Task<bool> DisplaySingleDealStage(GameContext gameContext, List<Player> players, int stage)
        {
            // complete task asynchronously so that it doesn't block
            var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            List<string> playerDealOrder = gameContext.CurRound.PlayerDealOrder;
            // iterate through all players: cards for AI players are dealt face down and cards for human players face
            for (int i = 0; i < playerDealOrder.Count(); i++)
            {
                string playerName = playerDealOrder[i];
                Card cardToTransfer = players.Find(player => player.Name == playerName).Hand[stage];
                CardGroup destinationGroup = _playerCardGroups[playerName];
                _componentProvider.CenterCardGroup.Transfer(cardToTransfer, destinationGroup, new AnimationBehavior
                {
                    Duration = 0.3,
                    Delay = .125 * i,
                    Rotations = 3
                });
            }

            _componentProvider.QueueAnimationsCompletedHandler(() => taskCompletionSource.SetResult(true)); 
            _componentProvider.BeginAnimations();

            return taskCompletionSource.Task;
        }

        public async Task<bool> DisplayTrickWinner(RoundContext curRound)
        {
            var curTrick = curRound.CurTrick;
            var winner = curTrick.Winner;
            _componentProvider.SetMessageBoxText($"{winner.Name} won with a {curTrick.WinningCard.DisplayName}");
            
            var bid = curRound.Bids[winner];
            // note: technically the round is not over, so the results in the current round are from the prev trick => 1 is added
            var tricksTaken = curRound.Results[winner] + 1;
            _componentProvider.SetPlayerStatus(_playerOrdinals[winner.Name], $"{tricksTaken}/{bid}");

            await Task.Delay(1000);
            return true;
        }

        public Task<bool> DisplayRoundScores(GameContext gameContext)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> DisplayBidOutcome(int roundNum, int totalBids)
        {
            string bidResult = null;
            if (totalBids > roundNum)
                bidResult = "overbid";
            else if (totalBids == roundNum)
                bidResult = "matched bid";
            else
                bidResult = "underbid";
            _componentProvider.SetMessageBoxText($"{totalBids} bids on {roundNum} tricks - {bidResult}");

            await Task.Delay(2000);
            return true;
        }

        public Task<Card> PromptPlayerCardSelection(HumanPlayer player, IReadOnlyList<Card> playableCards)
        {
            var tcs = new TaskCompletionSource<Card>();
            var playerCardGroup = (InteractiveAdjacentCardGroup)_playerCardGroups[player.Name];
            _componentProvider.SetMessageBoxText($"{player.Name}, choose your card");
            playerCardGroup.StartInteractiveSession(playableCards, displayCard => tcs.SetResult(displayCard.CoreCard));
            return tcs.Task;
        }

        public Task<int> PromptPlayerBid(HumanPlayer player)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            _componentProvider.SetMessageBoxText($"{player.Name}: make your bid");
            _componentProvider.SetHumanPlayerBidInputVisibility(true);
            _componentProvider.OnPlayerBidInputEntered((int bid) =>
            {
                _componentProvider.SetPlayerStatus(_playerOrdinals[player.Name], $"0/{bid}");
                taskCompletionSource.SetResult(bid);
            });
            return taskCompletionSource.Task;
        }

        public Task<List<string>> PromptPlayerCreation()
        {
            _componentProvider.SetMessageBoxText("Player Creation");
            _componentProvider.SetPlayerCreationInputVisibility(true);            

            TaskCompletionSource<List<string>> taskCompletionSource = new TaskCompletionSource<List<string>>();
            _componentProvider.OnPlayerCreationInputEntered((string input) =>
            {
                // input is the nanme of the user
                // default bot players will be added too
                List<string> playerNames = new List<string> { input, "wizbot1", "wizbot2", "wizbot3" };

                // set names to frontend name elements
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER1, playerNames[0]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER2, playerNames[1]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER3, playerNames[2]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER4, playerNames[3]);


                // cache new names in name: playerOrdinal dictionary
                _playerOrdinals[playerNames[0]] = PlayerOrdinal.PLAYER1;
                _playerOrdinals[playerNames[1]] = PlayerOrdinal.PLAYER2;
                _playerOrdinals[playerNames[2]] = PlayerOrdinal.PLAYER3;
                _playerOrdinals[playerNames[3]] = PlayerOrdinal.PLAYER4;

                // cache new names in name: cardGroup dictionary
                _playerCardGroups[playerNames[0]] = _componentProvider.Player1CardGroup;
                _playerCardGroups[playerNames[1]] = _componentProvider.Player2CardGroup;
                _playerCardGroups[playerNames[2]] = _componentProvider.Player3CardGroup;
                _playerCardGroups[playerNames[3]] = _componentProvider.Player4CardGroup;

                taskCompletionSource.SetResult(playerNames);
                _componentProvider.SetPlayerCreationInputVisibility(false);
            });

            return taskCompletionSource.Task;
        }
    }
}
