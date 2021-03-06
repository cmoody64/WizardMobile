﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using WizardMobile.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core.Tests
{
    [TestClass()]
    public class AIPlayerTests
    {
        [TestMethod()]
        public void MakeTurnTest()
        {
            IWizardFrontend testFrontend = null;

            var roundNum = 4;
            var deck = new Deck();
            deck.Shuffle();

            var aiPlayer = new AIPlayer(testFrontend, "wizardAI");
            var humanPlayer1 = new HumanPlayer(testFrontend, "Connor");
            var humanPlayer2 = new HumanPlayer(testFrontend, "Diana");
            for(int i = 0; i < roundNum; i++)
            {
                aiPlayer.TakeCard(deck.PopTop());
                humanPlayer1.TakeCard(deck.PopTop());
                humanPlayer2.TakeCard(deck.PopTop());
            }            

            var testPlayers = new List<Player>
            {
                aiPlayer,
                humanPlayer1,
                humanPlayer2
            };

            var testGameContext = new GameContext(testPlayers);

            var trumpCard = new Card(CardValue.EIGHT, CardSuite.DIAMONDS);
            var curRound = new RoundContext(roundNum);
            curRound.TrumpCard = trumpCard;
            testPlayers.ForEach(player => curRound.Bids[player] = 0);
            testPlayers.ForEach(player => curRound.Results[player] = 0);
            var trick1 = new TrickContext(1);
            trick1.CardsPlayed[0] = (new Card(CardValue.JACK, CardSuite.CLUBS));

            curRound.Tricks[0] = (trick1);
            testGameContext.Rounds[0] = (curRound);

            aiPlayer.MakeTurn(testGameContext);
        }

        [TestMethod()]
        public void MakeBidTest()
        {
            IWizardFrontend testFrontend = null;

            var roundNum = 3;
            var deck = new Deck();
            deck.Shuffle();

            var aiPlayer = new AIPlayer(testFrontend, "wizardAI");
            var humanPlayer1 = new HumanPlayer(testFrontend, "Connor");
            var humanPlayer2 = new HumanPlayer(testFrontend, "Diana");
            for (int i = 0; i < roundNum; i++)
            {
                aiPlayer.TakeCard(deck.PopTop());
                humanPlayer1.TakeCard(deck.PopTop());
                humanPlayer2.TakeCard(deck.PopTop());
            }

            var testPlayers = new List<Player>
            {
                aiPlayer,
                humanPlayer1,
                humanPlayer2
            };
            var testGameContext = new GameContext(testPlayers);

            var trumpCard = new Card(CardValue.EIGHT, CardSuite.DIAMONDS);
            var curRound = new RoundContext(roundNum);
            curRound.TrumpCard = trumpCard;
            testGameContext.Rounds[0] = (curRound);

            aiPlayer.MakeBid(testGameContext);
        }
    }
}