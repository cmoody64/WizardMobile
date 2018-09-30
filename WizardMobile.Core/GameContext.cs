using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Concurrent;

namespace WizardMobile.Core
{
    // state that persists across the scope of a single game
    public class GameContext
    {
        // standard constructor called when there is no previously existing game state
        public GameContext(List<Player> players)
        {
            _playerCount = players.Count;
            _rounds = new ConcurrentDictionary<int, RoundContext>();
            _playerScores = new ConcurrentDictionary<Player, int>();

            // initialize player scores based off of the current player list passed in by the engine
            players.ForEach(player => PlayerScores[player] = 0);
        }

        // constructor called when basing a game state off of previously existing game state
        public GameContext(IDictionary<Player, int> existingScores)
        {
            _playerCount = existingScores.Count;
            _rounds = new ConcurrentDictionary<int, RoundContext>();
            _playerScores = new ConcurrentDictionary<Player, int>(existingScores);            
        }

        private int _playerCount;
        private ConcurrentDictionary<int, RoundContext> _rounds; // maps round number to round context
        private ConcurrentDictionary<Player, int> _playerScores;

        public int PlayerCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _playerCount;
        }

        public ConcurrentDictionary<int, RoundContext> Rounds
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _rounds;
        }

        public ConcurrentDictionary<Player, int> PlayerScores
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _playerScores;
        }

        public RoundContext CurRound
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => Rounds.Count > 0 ? Rounds [Rounds.Count] : null;
        }

        public RoundContext PrevRound
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => Rounds.Count > 1 ? Rounds[Rounds.Count - 1] : null;
        }

        public int MaxRoundCount
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => PlayerCount / Deck.STARTING_CARD_COUNT;
        }
    }

    // state that persists across a single round
    public class RoundContext
    {
        public RoundContext(int roundNum, Card trumpCard)
        {
            _roundNum = roundNum;
            _trumpCard = trumpCard;
            _tricks = new ConcurrentDictionary<int, TrickContext>();
            _bids = new ConcurrentDictionary<Player, int>();
            _results = new ConcurrentDictionary<Player, int>();
            _playerDealOrder = new ConcurrentDictionary<int, string>();
        }

        private int _roundNum;
        private ConcurrentDictionary<int, TrickContext> _tricks; // maps trick number to trick context
        private ConcurrentDictionary<Player, int> _bids;
        private ConcurrentDictionary<Player, int> _results;
        private Player _dealer;
        private Card _trumpCard;
        private ConcurrentDictionary<int, string> _playerDealOrder; // maps player order to player name

        public int RoundNum
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _roundNum;
        }

        public ConcurrentDictionary<int, TrickContext> Tricks
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _tricks;
        }

        public ConcurrentDictionary<Player, int> Bids
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _bids;
        }

        public ConcurrentDictionary<Player, int> Results
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _results;
        }

        public CardSuite TrumpSuite
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => TrumpCard?.Suite ?? CardSuite.SPECIAL;
        }

        public Player Dealer
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _dealer;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _dealer = value;
        }
        
        public Card TrumpCard
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _trumpCard;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _trumpCard = value;
        }

        public TrickContext CurTrick
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => Tricks.Count > 0 ? Tricks[Tricks.Count] : null;
        }

        public TrickContext PrevTrick
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => Tricks.Count > 1 ? Tricks [Tricks.Count - 1] : null;
        }

        // player names arranged in the order they will be dealt cards (i.e. dealer last)  
        public ConcurrentDictionary<int, string> PlayerDealOrder
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _playerDealOrder;
        }
    }

    // state that persists across a single trick
    public class TrickContext
    {
        public TrickContext()
        {
            _cardsPlayed = new ConcurrentDictionary<int, Card>();
        }
        public TrickContext(int trickNum)
        {
            TrickNum = trickNum;
            _cardsPlayed = new ConcurrentDictionary<int, Card>();
        }

        private int _trickNum;
        private ConcurrentDictionary<int, Card> _cardsPlayed; // maps play order to cards
        private Player _winner;
        private Card _winningCard;


        public int TrickNum
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _trickNum;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _trickNum = value;
        }

        public ConcurrentDictionary<int, Card> CardsPlayed
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _cardsPlayed;
        }

        public CardSuite? LeadingSuite
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => CardsPlayed.Count > 0 ? CardsPlayed[0].Suite : (CardSuite?)null;
        }

        public Player Winner
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _winner;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _winner = value;
        }

        public Card WinningCard
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get => _winningCard;

            [MethodImpl(MethodImplOptions.Synchronized)]
            set => _winningCard = value;
        }
    }
}
