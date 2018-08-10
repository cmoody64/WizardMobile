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
            return Task.FromResult(true);
        }

        public Task<bool> DisplayPlayerBid(int bid, Player player)
        {
            return Task.FromResult(true);
        }

        public Task<bool> DisplayShuffle()
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            const int shuffleCount = 5;
            _componentProvider.LeftCenterCardGroup.AddRange(Enumerable.Repeat(BACK_OF_CARD_KEY, shuffleCount));
            _componentProvider.RightCenterCardGroup.AddRange(Enumerable.Repeat(BACK_OF_CARD_KEY, shuffleCount));

            for (int i = 0; i < shuffleCount; i++)
            {
                _componentProvider.LeftCenterCardGroup.Transfer
                (
                    BACK_OF_CARD_KEY,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.2 * i, Duration = 0.2 }
                );
                _componentProvider.RightCenterCardGroup.Transfer
                (
                    BACK_OF_CARD_KEY,
                    _componentProvider.CenterCardGroup,
                    new AnimationBehavior { Delay = 0.2 * i, Duration = 0.2 }
                );
            }

            _componentProvider.QueueAnimationsCompletedHandler(() =>
            {
                // remove all but 1 card backs since they are stacked vertically
                int cardsToRemove = shuffleCount * 2 - 2;
                for (int i = 0; i < cardsToRemove; i++)
                {
                    _componentProvider.CenterCardGroup.Remove(BACK_OF_CARD_KEY);
                }
                // complete the task
                taskCompletionSource.SetResult(true);
            });
            _componentProvider.BeginAnimations();

            return taskCompletionSource.Task;
        }

        public Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            var faceUpHand = players.Find(player => player is HumanPlayer).Hand; // TODO this seems pretty hacky, better way to find human player at runtime?
            for (int i = 0; i < gameContext.CurRound.RoundNum; i++)
            {
                // iterate through all AI players and deal cards face  down
                for (int j = 0; j < players.Count - 1; j++)
                {
                    _componentProvider.CenterCardGroup.Add(BACK_OF_CARD_KEY);
                    CardGroup destinationGroup = null;
                    switch(j)
                    {
                        case 0: destinationGroup = _componentProvider.Player2CardGroup; break;
                        case 1: destinationGroup = _componentProvider.Player3CardGroup; break;
                        case 2: destinationGroup = _componentProvider.Player4CardGroup; break;
                    }
                    _componentProvider.CenterCardGroup.Transfer(BACK_OF_CARD_KEY, destinationGroup, new AnimationBehavior
                    {
                        Duration = 0.3,
                        Delay = 0.5 * i + .125 * j,
                        Rotations = 3
                    });
                }

                // deal Human players hand face up
                var playerCardName = faceUpHand[i].ToString();
                _componentProvider.CenterCardGroup.Add(playerCardName);
                _componentProvider.CenterCardGroup.Transfer(playerCardName, _componentProvider.Player1CardGroup, new AnimationBehavior
                {
                    Duration = 0.3,
                    Delay = 0.5 * (i + 1),
                    Rotations = 3
                });

            }

            _componentProvider.QueueAnimationsCompletedHandler(() => taskCompletionSource.SetResult(true));
            _componentProvider.BeginAnimations();

            return taskCompletionSource.Task;
        }

        public Task<bool> DisplayTrickWinner(Player winner, Card winningCard)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisplayRoundScores(GameContext gameContext)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DisplayBidOutcome(int roundNum, int totalBids)
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
            _componentProvider.SetMessageBoxText("Player Creation");
            _componentProvider.SetPlayerCreationInputVisibility(true);            

            TaskCompletionSource<List<string>> taskCompletionSource = new TaskCompletionSource<List<string>>();
            _componentProvider.PlayerCreationInputEntered += (string input) =>
            {
                taskCompletionSource.SetResult(new List<string>() { input });
                _componentProvider.SetPlayerCreationInputVisibility(false);
            };

            return taskCompletionSource.Task;
        }


        private static readonly string BACK_OF_CARD_KEY = "back_of_card";
        private static readonly Point LEFT_STACK_STARTING_POINT = new Point(-300, 50);
        private static readonly Point RIGHT_STACK_STARTING_POINT = new Point(300, 50);
        private static readonly Point CENTER_STACK_STARTING_POINT = new Point(0, 50);
    }
}
