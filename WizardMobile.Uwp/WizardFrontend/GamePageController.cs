using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
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
            _offScreenPlayerCardGroups = new Dictionary<string, CardGroup>();
            _playerOrdinals = new Dictionary<string, PlayerOrdinal>();
            _dealerButtonPositions = new Dictionary<string, DealerButtonPosition>();

            // since engine runs certain functionality on a separate worker thread, the calls that the engine make to the frontend
            // must be marshalled through the proxy frontend which implements multithreading protocol
            // this relationship does not extend two ways - this class can make calls directly to the engine
            // this is because the engine and this class both live on the same thread, the engine only does work on a different thread
            UwpWizardFrontendProxy _proxyFrontend = new UwpWizardFrontendProxy(this, uiDispatcher);
            _engine = new WizardEngine(_proxyFrontend);
            _engine.Run();

            // bind menu button handlers
            _componentProvider.OnPauseButtonClick(this.PauseButtonClickedHandler);
            _componentProvider.OnScoresButtonClick(this.ScoresButtonClickedHandler);
            _componentProvider.OnQuitButtonClick(this.QuitButtonClickedHandler);
        }

        private IWizardComponentProvider _componentProvider;
        private WizardEngine _engine;
        private Dictionary<string, CardGroup> _playerCardGroups; // maps player names to the corresponding hand cardGroup
        private Dictionary<string, CardGroup> _offScreenPlayerCardGroups;
        private Dictionary<string, DealerButtonPosition> _dealerButtonPositions;
        private Dictionary<string, PlayerOrdinal> _playerOrdinals; // maps player names to PlayerOrdinals used in the componentProvider
        public static readonly string GAME_STATE_FILENAME = "game_state.txt";

        /*************** IWizardFrontend implementation ********************/
        public async Task<bool> DisplayStartGame()
        {
            _componentProvider.SetMessageBoxText("Game Starting");
            _componentProvider.SetAllPersonasVisibility(false);
            await Task.Delay(2000);            
            return true;

        }

        public async Task<bool> DisplayStartRound(GameContext gameContext)
        {
            _componentProvider.SetMessageBoxText($"Round {gameContext.CurRound.RoundNum} Starting");

            if (gameContext.CurRound.RoundNum == 1)
            {
                _dealerButtonPositions[gameContext.CurRound.Dealer.Name].ShowButton();
            }
            else
            {
                _dealerButtonPositions[gameContext.PrevRound.Dealer.Name]
                    .TransferButton(_dealerButtonPositions[gameContext.CurRound.Dealer.Name]);
                await _componentProvider.RunQueuedAnimations();
            }

            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayEndRound(int roundNum)
        {
            _componentProvider.SetMessageBoxText("Round Over");
            _componentProvider.DeckCardGroup.RemoveAll();
            _componentProvider.TrumpCardGroup.RemoveAll();
            _componentProvider.OffScreenPlayer1CardGroup.RemoveAll();
            _componentProvider.OffScreenPlayer2CardGroup.RemoveAll();
            _componentProvider.OffScreenPlayer3CardGroup.RemoveAll();
            _componentProvider.OffScreenPlayer4CardGroup.RemoveAll();

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
            _componentProvider.DiscardCardGroup.RemoveAll();
            return Task.FromResult(true);
        }

        public async Task<bool> DisplayTrumpCardSelected(Card trumpCard)
        {
            _componentProvider.CenterShuffleCardGroup.Transfer
            (
                trumpCard,
                _componentProvider.RightShuffleCardGroup,
                new AnimationBehavior { Duration = 0.3 }
            );

            await _componentProvider.RunQueuedAnimations();
            _componentProvider.RightShuffleCardGroup.Flip(trumpCard);
            return true;
        }

        public async Task<bool> DisplayTurnInProgress(Player player)
        {
            _componentProvider.SetMessageBoxText($"{player.Name}'s turn");
            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayTurnTaken(Card cardPlayed, Player player)
        {
            var sourceCardGroup = _playerCardGroups[player.Name];

            if (player is AIPlayer)
                sourceCardGroup.Flip(cardPlayed);

            sourceCardGroup.Transfer
            (
                cardPlayed,
                _componentProvider.DiscardCardGroup,
                new AnimationBehavior() { Duration = 0.3, Rotations = 3 }
            );

            await _componentProvider.RunQueuedAnimations();
            return true;
        }

        public async Task<bool> DisplayPlayerBid(int bid, Player player)
        {
            _componentProvider.SetMessageBoxText($"{player.Name} bids {bid}");
            _componentProvider.SetPlayerStatus(_playerOrdinals[player.Name], $"0 / {bid}");
            await Task.Delay(1000);
            return true;
        }

        public async Task<bool> DisplayShuffle(IReadonlyDeck deckToShuffle)
        {
            // add cards alternating to left and right center stacks
            for (int i = 0; i < deckToShuffle.Cards.Count; i += 2)
            {
                var leftCard = deckToShuffle.Cards[i];
                var rightCard = deckToShuffle.Cards[i + 1];

                _componentProvider.LeftShuffleCardGroup.Add(leftCard);
                _componentProvider.RightShuffleCardGroup.Add(rightCard);

                _componentProvider.LeftShuffleCardGroup.Transfer
                (
                    leftCard,
                    _componentProvider.CenterShuffleCardGroup,
                    new AnimationBehavior { Delay = 0.025 * i, Duration = 0.1 }
                );

                _componentProvider.RightShuffleCardGroup.Transfer
                (
                    rightCard,
                    _componentProvider.CenterShuffleCardGroup,
                    new AnimationBehavior { Delay = 0.025 * i + .0125, Duration = 0.1 }
                );
            }

            await _componentProvider.RunQueuedAnimations();

            return true;
        }

        public async Task<bool> DisplayDeal(GameContext gameContext, List<Player> players)
        {
            for (int dealStage = 0; dealStage < gameContext.CurRound.RoundNum; dealStage++)
            {
                await DisplaySingleDealStage(gameContext, players, dealStage);
            }

            _componentProvider.Player1CardGroup.FlipAll();
            await DisplayShiftDeckPostShuffle();

            return true;
        }

        private async Task<bool> DisplayShiftDeckPostShuffle()
        {
            _componentProvider.CenterShuffleCardGroup.TransferAll(_componentProvider.DeckCardGroup, new AnimationBehavior { Duration = 0.3 });
            _componentProvider.RightShuffleCardGroup.TransferAll(_componentProvider.TrumpCardGroup, new AnimationBehavior { Duration = 0.3 });
            _componentProvider.DeckCardGroup.BringCardGroupToFront(_componentProvider.TrumpCardGroup);
            await _componentProvider.RunQueuedAnimations();
            return true;
        }

        private async Task<bool> DisplaySingleDealStage(GameContext gameContext, List<Player> players, int stage)
        {
            List<string> playerDealOrder = gameContext.CurRound.PlayerDealOrder
                .OrderBy(keyValPair => keyValPair.Key)
                .Select(keyValPair => keyValPair.Value)
                .ToList();

            // iterate through all players: cards for AI players are dealt face down and cards for human players face
            for (int i = 0; i < playerDealOrder.Count(); i++)
            {
                string playerName = playerDealOrder[i];
                Card cardToTransfer = players.Find(player => player.Name == playerName).Hand[stage];
                CardGroup destinationGroup = _playerCardGroups[playerName];
                _componentProvider.CenterShuffleCardGroup.Transfer(cardToTransfer, destinationGroup, new AnimationBehavior
                {
                    Duration = 0.3,
                    Delay = .125 * i,
                    Rotations = 3
                });
            }

            await _componentProvider.RunQueuedAnimations();

            return true;
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

            _componentProvider.DiscardCardGroup.BringCardToFront(curTrick.WinningCard);
            _componentProvider.DiscardCardGroup.TransferAll
            (
                _componentProvider.CollapsedDiscardCardGroup,
                new AnimationBehavior() { Duration = 0.2 }
            );
            await _componentProvider.RunQueuedAnimations();

            var winningCardGroup = _offScreenPlayerCardGroups[curTrick.Winner.Name];
            _componentProvider.CollapsedDiscardCardGroup.TransferAll
            (
                winningCardGroup,
                new AnimationBehavior() { Duration = 0.75, Rotations = 2 }
            );
            await _componentProvider.RunQueuedAnimations();

            return true;
        }

        public async Task<bool> DisplayRoundScores(GameContext gameContext)
        {
            // update scoreboard text
            foreach(var playerScorePair in gameContext.PlayerScores)
            {
                var player = playerScorePair.Key;
                var score = playerScorePair.Value;
                _componentProvider.SetPlayerScore(_playerOrdinals[player.Name], score);
            }

            // translate scoreboard
            _componentProvider.QueueAnimationRequest(new NamedAnimationRequest
            {
                TargetElementName = "scoreboard_container",
                Destination = new NormalizedPosition(50, 47),
                Duration = 0.2  
            });

            _componentProvider.QueueAnimationRequest(new NamedAnimationRequest
            {
                TargetElementName = "scoreboard_container",
                AdditionalBehaviors = new Dictionary<string, double>() { { AnimationProperties.OPACITY, _componentProvider.OPACITY_HIGH } },
                Duration = 0.2
            });

            // increase font size of scoreboard
            Func<string, double, NamedAnimationRequest> makeScoreboardTextAnimation = (string targetName, double fontSize) => new NamedAnimationRequest
            {
                TargetElementName = targetName,
                Duration = 0.5,
                AdditionalBehaviors = new Dictionary<string, double>() { { AnimationProperties.FONT_SIZE, fontSize } }
            };
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_title", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player1_name", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player1_score", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player2_name", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player2_score", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player3_name", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player3_score", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player4_name", 20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player4_score", 20));

            await _componentProvider.RunQueuedAnimations();
            await Task.Delay(1500);

            // translate scoreboard back to original position
            _componentProvider.QueueAnimationRequest(new NamedAnimationRequest
            {
                TargetElementName = "scoreboard_container",
                Destination = new NormalizedPosition(10, 16),
                Duration = 0.2
            });

            _componentProvider.QueueAnimationRequest(new NamedAnimationRequest
            {
                TargetElementName = "scoreboard_container",
                AdditionalBehaviors = new Dictionary<string, double>() { { AnimationProperties.OPACITY, -_componentProvider.OPACITY_HIGH } },
                Duration = 0.2
            });

            // increase font size of scoreboard
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_title", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player1_name", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player1_score", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player2_name", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player2_score", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player3_name", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player3_score", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player4_name", -20));
            _componentProvider.QueueAnimationRequest(makeScoreboardTextAnimation("scoreboard_player4_score", -20));

            await _componentProvider.RunQueuedAnimations();

            return true;
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

        public Task<GameConfiguration> PromptGameConfiguration()
        {
            _componentProvider.SetMessageBoxText("Player Creation");
            _componentProvider.SetGameConfigurationInputVisibility(true);

            TaskCompletionSource<GameConfiguration> taskCompletionSource = new TaskCompletionSource<GameConfiguration>();
            _componentProvider.OnGameConfigurationFinished((int roundCount, string input) =>
            {
                List<string> botNames = new List<string> { "wizbot1", "wizbot2", "wizbot3" };

                // input is the nanme of the user
                // check to make sure that user input is unique, if not it is modified slightly
                if (botNames.Contains(input))
                    input = $"{input}+";

                // default bot players will be added too
                List<string> playerNames = new List<string> { input }.Concat(botNames).ToList();

                // set names to frontend name elements
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER1, playerNames[0]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER2, playerNames[1]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER3, playerNames[2]);
                _componentProvider.SetPlayerName(PlayerOrdinal.PLAYER4, playerNames[3]);

                // initialize scores to 0
                _componentProvider.SetPlayerScore(PlayerOrdinal.PLAYER1, 0);
                _componentProvider.SetPlayerScore(PlayerOrdinal.PLAYER2, 0);
                _componentProvider.SetPlayerScore(PlayerOrdinal.PLAYER3, 0);
                _componentProvider.SetPlayerScore(PlayerOrdinal.PLAYER4, 0);


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

                // cache new names in name: offScreenCardGroup dictionary
                _offScreenPlayerCardGroups[playerNames[0]] = _componentProvider.OffScreenPlayer1CardGroup;
                _offScreenPlayerCardGroups[playerNames[1]] = _componentProvider.OffScreenPlayer2CardGroup;
                _offScreenPlayerCardGroups[playerNames[2]] = _componentProvider.OffScreenPlayer3CardGroup;
                _offScreenPlayerCardGroups[playerNames[3]] = _componentProvider.OffScreenPlayer4CardGroup;

                // cache new names in name: dealerButtonPosition dictionary
                _dealerButtonPositions[playerNames[0]] = _componentProvider.Player1ButtonPosition;
                _dealerButtonPositions[playerNames[1]] = _componentProvider.Player2ButtonPosition;
                _dealerButtonPositions[playerNames[2]] = _componentProvider.Player3ButtonPosition;
                _dealerButtonPositions[playerNames[3]] = _componentProvider.Player4ButtonPosition;

                taskCompletionSource.SetResult(new GameConfiguration { RoundCount = roundCount, PlayerNames = playerNames });
                _componentProvider.SetGameConfigurationInputVisibility(false);
                _componentProvider.SetAllPersonasVisibility(true);
                _componentProvider.SetScoreboardVisibility(false);
                _componentProvider.SetScoresButtonEnabled(true);
            });

            return taskCompletionSource.Task;
        }

        private void PauseButtonClickedHandler()
        {

        }

        private void ScoresButtonClickedHandler()
        {
            var visibility = !_componentProvider.ScoreboardVisibility;
            _componentProvider.SetScoreboardVisibility(visibility);
        }

        private async void QuitButtonClickedHandler()
        {
            await SaveGameState();
            _componentProvider.App.NavigateToPage(WizardUwpApp.Page.MAIN_MENU);
        }

        private async Task<bool> SaveGameState()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile gameStateFile = await localFolder.CreateFileAsync(GAME_STATE_FILENAME, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(gameStateFile, _engine.SerializeEngineState());
            return true;
        }
    }
}
