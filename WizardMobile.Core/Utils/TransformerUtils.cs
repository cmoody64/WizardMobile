using System;
using System.Collections.Generic;
using System.Text;

namespace WizardMobile.Core.Utils
{
    public static class GameContextTransformer
    {
        public static GameContext FromDto(GameContextDto gameContextDto)
        {
            var gameContext = new GameContext(gameContextDto.PlayerScores.ToDictionary());
            foreach(var roundPair in gameContextDto.Rounds)
                gameContext.Rounds[roundPair.Key] = RoundContextTransformer.FromDto(roundPair.Value);

            return gameContext;
        }
    }

    static class RoundContextTransformer
    {
        public static RoundContext FromDto(RoundContextDto roundContextDto)
        {
            var roundContext = new RoundContext(roundContextDto.RoundNum);
            roundContext.Dealer = roundContextDto.Dealer;
            roundContext.TrumpCard = roundContextDto.TrumpCard;

            foreach (var kvp in roundContextDto.Bids)
                roundContext.Bids[kvp.Key] = kvp.Value;

            foreach (var kvp in roundContextDto.PlayerDealOrder)
                roundContext.PlayerDealOrder[kvp.Key] = kvp.Value;

            foreach (var kvp in roundContextDto.Results)
                roundContext.Results[kvp.Key] = kvp.Value;

            foreach (var kvp in roundContextDto.Tricks)
                roundContext.Tricks[kvp.Key] = TrickContextTransformer.FromDto(kvp.Value);

            return roundContext;
        }
    }

    static class TrickContextTransformer
    {
        public static TrickContext FromDto(TrickContextDto trickContextDto)
        {
            var trickContext = new TrickContext();

            foreach (var kvp in trickContextDto.CardsPlayed)
                trickContext.CardsPlayed[kvp.Key] = kvp.Value;

            trickContext.TrickNum = trickContextDto.TrickNum;
            trickContext.Winner = trickContextDto.Winner;
            trickContext.WinningCard = trickContextDto.WinningCard;

            return trickContext;
        }
    }
}
