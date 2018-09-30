using System;
using System.Collections.Generic;
using System.Text;

namespace WizardMobile.Core
{
    public class GameContextDto
    {
        public int PlayerCount { get; }
        public IReadOnlyDictionary<int, RoundContextDto> Rounds { get; }
        public IReadOnlyDictionary<Player, int> PlayerScores { get; }
        public RoundContextDto CurRound { get; }
        public RoundContextDto PrevRound { get; }
        public int MaxRoundCount { get; }
    }

    public class RoundContextDto
    {
        public int RoundNum { get; }
        public IReadOnlyDictionary<int, TrickContextDto> Tricks { get; }
        public IReadOnlyDictionary<Player, int> Bids { get; }
        public IReadOnlyDictionary<Player, int> Results { get; }
        public CardSuite? TrumpSuite { get; }
        public Card TrumpCard { get; }
        public Player Dealer { get; }
        public TrickContextDto CurTrick { get; }
        public TrickContextDto PrevTrick { get; }
        public IReadOnlyDictionary<int, string> PlayerDealOrder { get; }
    }

    public class TrickContextDto
    {
        public int TrickNum { get; }
        public IReadOnlyDictionary<int, Card> CardsPlayed { get; }
        public CardSuite? LeadingSuite { get; }
        public Player Winner { get; }
        public Card WinningCard { get; }
    }
}
