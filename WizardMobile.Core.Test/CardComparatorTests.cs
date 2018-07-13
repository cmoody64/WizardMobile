using Microsoft.VisualStudio.TestTools.UnitTesting;
using WizardMobile.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardMobile.Core.Tests
{
    [TestClass()]
    public class CardComparatorTests
    {
        [TestMethod()]
        public void CalcWinningCardTest1()
        {
            var trumpSuite = CardSuite.SPADES;
            var cardsPlayed = new[]
            {
                new Card(CardValue.FIVE, CardSuite.SPADES),
                new Card(CardValue.WIZARD, CardSuite.SPECIAL),
                new Card(CardValue.SIX, CardSuite.DIAMONDS),
                new Card(CardValue.NINE, CardSuite.CLUBS)
            }.ToList();
            var leadingSuite = cardsPlayed[0].Suite;
            Card expectedWinner = cardsPlayed[1];
            Card winner = CardUtils.CalcWinningCard(cardsPlayed, trumpSuite, leadingSuite);
            Assert.AreEqual(winner, expectedWinner);
        }

        [TestMethod()]
        public void CalcWinningCardTest2()
        {
            PrivateObject engineTester = new PrivateObject(new WizardEngine());
            var trumpSuite = CardSuite.DIAMONDS;
            var cardsPlayed = new[]
            {
                new Card(CardValue.FIVE, CardSuite.SPADES),
                new Card(CardValue.QUEEN, CardSuite.SPADES),
                new Card(CardValue.KING, CardSuite.SPADES),
                new Card(CardValue.ACE, CardSuite.HEARTS)
            }.ToList();
            var leadingSuite = cardsPlayed[0].Suite;
            Card expectedWinner = cardsPlayed[2];
            Card winner = CardUtils.CalcWinningCard(cardsPlayed, trumpSuite, leadingSuite);
            Assert.AreEqual(winner, expectedWinner);
        }

        [TestMethod()]
        public void CalcWinningCardTest3()
        {
            PrivateObject engineTester = new PrivateObject(new WizardEngine());
            var trumpSuite = CardSuite.CLUBS;
            var cardsPlayed = new[]
            {
                new Card(CardValue.FIVE, CardSuite.HEARTS),
                new Card(CardValue.SEVEN, CardSuite.CLUBS),
                new Card(CardValue.KING, CardSuite.CLUBS),
                new Card(CardValue.QUEEN, CardSuite.CLUBS)
            }.ToList();
            var leadingSuite = cardsPlayed[0].Suite;
            Card expectedWinner = cardsPlayed[2];
            Card winner = CardUtils.CalcWinningCard(cardsPlayed, trumpSuite, leadingSuite);
            Assert.AreEqual(winner, expectedWinner);
        }

        [TestMethod()]
        public void CalcWinningCardTest4()
        {
            PrivateObject engineTester = new PrivateObject(new WizardEngine());
            var trumpSuite = CardSuite.SPADES;
            var cardsPlayed = new[]
            {
                new Card(CardValue.THREE, CardSuite.SPADES),
                new Card(CardValue.SEVEN, CardSuite.SPADES),
                new Card(CardValue.ACE, CardSuite.DIAMONDS),
                new Card(CardValue.TEN, CardSuite.HEARTS)
            }.ToList();
            var leadingSuite = cardsPlayed[0].Suite;
            Card expectedWinner = cardsPlayed[1];
            Card winner = CardUtils.CalcWinningCard(cardsPlayed, trumpSuite, leadingSuite);
            Assert.AreEqual(winner, expectedWinner);
        }

        [TestMethod()]
        public void CalcWinningCardTest5()
        {
            PrivateObject engineTester = new PrivateObject(new WizardEngine());
            var trumpSuite = CardSuite.HEARTS;
            var cardsPlayed = new[]
            {
                new Card(CardValue.FIVE, CardSuite.CLUBS),
                new Card(CardValue.WIZARD, CardSuite.SPECIAL),
                new Card(CardValue.NINE, CardSuite.HEARTS),
                new Card(CardValue.WIZARD, CardSuite.SPECIAL)
            }.ToList();
            var leadingSuite = cardsPlayed[0].Suite;
            Card expectedWinner = cardsPlayed[1];
            Card winner = CardUtils.CalcWinningCard(cardsPlayed, trumpSuite, leadingSuite);
            Assert.AreEqual(winner, expectedWinner);
        }
    }
}