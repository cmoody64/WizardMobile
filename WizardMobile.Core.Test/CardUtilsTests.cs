using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WizardMobile.Core.Utils;

namespace WizardMobile.Core.Test
{
    [TestClass]
    public class CardUtilsTests
    {
        [TestMethod]
        public void TestGetPlayableCards_StandardLeadingSuite()            
        {
            List<Card> hand = new List<Card>
            {
                new Card(CardValue.ACE, CardSuite.CLUBS),
                new Card(CardValue.FOUR, CardSuite.CLUBS),
                new Card(CardValue.JESTER, CardSuite.SPECIAL),
                new Card(CardValue.THREE, CardSuite.HEARTS)
            };

            var playableCards = CardUtils.GetPlayableCards(hand, CardSuite.CLUBS);

            Assert.AreEqual(3, playableCards.Count);
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.ACE, CardSuite.CLUBS)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.FOUR, CardSuite.CLUBS)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.JESTER, CardSuite.SPECIAL)));
        }

        [TestMethod]
        public void TestGetPlayableCards_StandardLeadingSuite_MultipleSpecialCards()
        {
            List<Card> hand = new List<Card>
            {
                new Card(CardValue.ACE, CardSuite.CLUBS),
                new Card(CardValue.TWO, CardSuite.HEARTS),
                new Card(CardValue.JESTER, CardSuite.SPECIAL),
                new Card(CardValue.WIZARD, CardSuite.SPECIAL)
            };

            var playableCards = CardUtils.GetPlayableCards(hand, CardSuite.CLUBS);

            Assert.AreEqual(3, playableCards.Count);
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.ACE, CardSuite.CLUBS)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.WIZARD, CardSuite.SPECIAL)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.JESTER, CardSuite.SPECIAL)));
        }

        [TestMethod]
        public void TestGetPlayableCards_SpecialLeadingSuite()
        {
            List<Card> hand = new List<Card>
            {
                new Card(CardValue.ACE, CardSuite.CLUBS),
                new Card(CardValue.TWO, CardSuite.HEARTS),
                new Card(CardValue.JESTER, CardSuite.SPECIAL),
                new Card(CardValue.WIZARD, CardSuite.SPECIAL)
            };

            var playableCards = CardUtils.GetPlayableCards(hand, CardSuite.SPECIAL);

            Assert.AreEqual(4, playableCards.Count);

            Assert.IsTrue(playableCards.Contains(new Card(CardValue.ACE, CardSuite.CLUBS)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.TWO, CardSuite.HEARTS)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.JESTER, CardSuite.SPECIAL)));
            Assert.IsTrue(playableCards.Contains(new Card(CardValue.WIZARD, CardSuite.SPECIAL)));
        }
    }
}
