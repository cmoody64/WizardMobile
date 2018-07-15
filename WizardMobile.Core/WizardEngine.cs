using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace WizardMobile.Core
{
    public class WizardEngine
    {
        public WizardEngine(IWizardFrontend frontend)
        {
            _frontend = frontend;
        }

        // blocking method that executes the entirity of the game flow
        public void Run()
        {
            Thread workerThread = new Thread(this.PlaySingleGame);
            workerThread.Start();
        }

        private async void PlaySingleGame()
        {
            _curDeck = new Deck();
            await _frontend.DisplayStartGame();
            List<string> playerNames = await _frontend.PromptPlayerCreation();
            _players = playerNames.Select<string, Player>((string name) =>
            {
                if (name.Contains("bot"))
                    return new AIPlayer(this._frontend, name);
                else
                    return new HumanPlayer(this._frontend, name);
            }).ToList();

            _gameContext = new GameContext(_players);

            int roundCount = _curDeck.Cards.Count / _players.Count;
            for (int round = 1; round <= roundCount; round++)
                await PlaySingleRound(round);
        }

        private async Task PlaySingleRound(int roundNum)
        {
            await _frontend.DisplayStartRound(roundNum);

            // shuffle, deal, and initialize round context
            _curDeck.Shuffle();
            await _frontend.DisplayDealInProgess(3/*message duration seconds*/);
            DealDeck(roundNum);
            Card trumpCard = _curDeck.Cards.Count > 0 ? _curDeck.PopTop() : null;

            _gameContext.Rounds.Add(new RoundContext(roundNum, trumpCard));
            var curRound = _gameContext.CurRound;
            curRound.Dealer = roundNum == 1
                ? _players[0]
                : _players[(_players.IndexOf(_gameContext.PrevRound.Dealer) + 1) % _players.Count];
            _players.ForEach(player => curRound.Results[player] = 0);

            await _frontend.DisplayDealDone(curRound.Dealer, trumpCard);

            // bid on current round
            _players.ForEach(async (player) => curRound.Bids[player] = await player.MakeBid(_gameContext));
            int totalBids = curRound.Bids.Aggregate(0, (accumulator, bidPair) => accumulator + bidPair.Value);
            await _frontend.DisplayBidOutcome(roundNum, totalBids);

            // execute tricks and record results
            for (int trickNum = 1; trickNum <= roundNum; trickNum++)
            {
                await PlaySingleTrick(trickNum);
                Player winner = curRound.CurTrick.Winner;
                if (curRound.Results.ContainsKey(winner))
                    curRound.Results[winner]++;
                else
                    curRound.Results[winner] = 1;
            }

            // resolve round scores
            _players.ForEach(player =>
            {
                int diff = Math.Abs(curRound.Bids[player] - curRound.Results[player]);
                if (diff == 0)
                    _gameContext.PlayerScores[player] += (BASELINE_SCORE + curRound.Bids[player] * HIT_SCORE);
                else
                    _gameContext.PlayerScores[player] += (diff * MISS_SCORE);
            });

            await _frontend.DisplayRoundScores(_gameContext);
        }

        // executes a single trick and stores state in a new TrickContext instance, as well
        private async Task PlaySingleTrick(int trickNum)
        {
            await _frontend.DisplayStartTrick(trickNum);
            _gameContext.CurRound.Tricks.Add(new TrickContext(trickNum));

            var curRound = _gameContext.CurRound;
            var curTrick = curRound.CurTrick;

            Player leader = trickNum == 1
                ? leader = _players[(_players.IndexOf(curRound.Dealer)+1) % _players.Count]
                : leader = curRound.PrevTrick.Winner;
            int leaderIndex = _players.IndexOf(leader);

            // create a player list that starts at the trick leader and wraps around
            List<Player> trickPlayerOrder = _players
                .GetRange(leaderIndex, _players.Count - leaderIndex)
                .Concat(_players.GetRange(0, leaderIndex)).ToList();

            trickPlayerOrder.ForEach(async (player) =>
            {
                var cardPlayed = await player.MakeTurn(_gameContext);
                curTrick.CardsPlayed.Add(cardPlayed);
                await _frontend.DisplayTurnTaken(cardPlayed, player);
            });

            // find winner and save it to trick context
            var winningCard = CardUtils.CalcWinningCard(curTrick.CardsPlayed, curRound.TrumpSuite, curTrick.LeadingSuite);
            var winningPlayer = trickPlayerOrder[curTrick.CardsPlayed.IndexOf(winningCard)];
            curTrick.Winner = winningPlayer;
            await _frontend.DisplayTrickWinner(winningPlayer, winningCard);            
        }

        private void DealDeck(int roundNum)
        {
            for(int i = 0; i < roundNum; i++)
                foreach (var player in _players)
                    player.TakeCard(_curDeck.PopTop());
        }


        private List<Player> _players;
        private Deck _curDeck;        
        private IWizardFrontend _frontend { get; }
        private GameContext _gameContext;

        private readonly int BASELINE_SCORE = 20;
        private readonly int HIT_SCORE = 10;
        private readonly int MISS_SCORE = -10;

    }
}
