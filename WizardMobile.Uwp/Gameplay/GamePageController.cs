using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using WizardMobile.Core;
using WizardMobile.Uwp.Common;

namespace WizardMobile.Uwp.Gameplay
{
    class GamePageController: IWizardFrontend
    {
        public GamePageController(IWizardComponentProvider componentProvider, CoreDispatcher uiDispatcher)
        {
            _componentProvider = componentProvider;
            _playerCardGroups = new Dictionary<string, CardGroup>();

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

        /*************** IWizardFrontend implementation ********************/
        public async Task<bool> DisplayStartGame()
        {
            _componentProvider.SetMessageBoxText("Game Starting");
            await Task.Delay(2000);
            return true;

        }

        public async Task<bool> DisplayStartRound(int roundNum)
        {
            _componentProvider.SetMessageBoxText($"Round {roundNum} Starting");
            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayStartTrick(int trickNum)
        {
            _componentProvider.SetMessageBoxText($"Trick {trickNum} Starting");
            await Task.Delay(1000);
            return true;
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
            await Task.Delay(1000);
            return true;
        }

        public Task<bool> DisplayShuffle(IReadonlyDeck deckToShuffle)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            // add cards alternating to left and right center stacks
            for(int i = 0; i < deckToShuffle.Cards.Count; i += 2)
            {
                var leftCard = deckToShuffle.Cards[i];
                var rightCard = deckToShuffle.Cards[i + 1];

                _componentProvider.LeftCenterCardGroup.Add(leftCard);
                _componentProvider.RightCenterCardGroup.Add(rightCard);

                _componentProvider.LeftCenterCardGroup.Transfer
                (
                    leftCard,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.05 * i, Duration = 0.05 }
                );
                _componentProvider.RightCenterCardGroup.Transfer
                (
                    rightCard,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.05 * i, Duration = 0.05 }
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

        public Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            List<string> playerDealOrder = gameContext.CurRound.PlayerDealOrder;
            for (int i = 0; i < gameContext.CurRound.RoundNum; i++)
            {
                // iterate through all players: cards for AI players are dealt face down and cards for human players face
                for(int j = 0; j < playerDealOrder.Count(); j++)
                {
                    string playerName = playerDealOrder[j];
                    Card cardToTransfer = players.Find(player => player.Name == playerName).Hand[i];
                    CardGroup destinationGroup = _playerCardGroups[playerName];
                    _componentProvider.CenterCardGroup.Transfer(cardToTransfer, destinationGroup, new AnimationBehavior
                    {
                        Duration = 0.3,
                        Delay = 0.5 * i + .125 * j,
                        Rotations = 3
                    });
                }
            }

            _componentProvider.QueueAnimationsCompletedHandler(() =>
            {
                _componentProvider.Player1CardGroup.FlipAll();
                taskCompletionSource.SetResult(true);
            });
            _componentProvider.BeginAnimations();

            return taskCompletionSource.Task;
        }

        public async Task<bool> DisplayTrickWinner(Player winner, Card winningCard)
        {
            _componentProvider.SetMessageBoxText($"{winner.Name} won with a {winningCard.DisplayName}");
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

        public Task<Card> PromptPlayerCardSelection(Player player)
        {
            var taskCompletionSource = new TaskCompletionSource<Card>();
            var playerCardGroup = _playerCardGroups[player.Name];
            _componentProvider.SetMessageBoxText($"{player.Name}, choose your card");
            playerCardGroup.QueueClickHandlerForCards(displayCard =>
            {
                taskCompletionSource.SetResult(displayCard.CoreCard);
            });

            return taskCompletionSource.Task;
        }

        public Task<int> PromptPlayerBid(Player player)
        {
            TaskCompletionSource<int> taskCompletionSource = new TaskCompletionSource<int>();
            _componentProvider.SetMessageBoxText($"${player.Name}: make your bid");
            _componentProvider.SetHumanPlayerBidInputVisibility(true);
            _componentProvider.OnPlayerBidInputEntered((int bid) =>
            {
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
