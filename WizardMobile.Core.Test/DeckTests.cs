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
    public class DeckTests
    {
        private readonly int WIZARD_DECK_COUNT = 60;

        [TestMethod()]
        public void DeckTest()
        {
            var deck = new Deck();
            Assert.AreEqual(deck.Cards.Count, WIZARD_DECK_COUNT);

            // use a set to test that each card is unique in the standard deck
            var standardDeckSet = new HashSet<Card>();
            // count the number of wizards and jesters to make sure there are 4 each
            int wizardCount = 0;
            int jesterCount = 0;
            foreach(var card in deck.Cards)
            {
                if(card.Suite != CardSuite.SPECIAL)
                {
                    if (standardDeckSet.Contains(card))
                        Assert.Fail($"Duplicate card: {card.ToString()}");
                    else
                        standardDeckSet.Add(card);
                }
                else if(card.Value == CardValue.JESTER)
                {
                    jesterCount++;
                }
                else if(card.Value == CardValue.WIZARD)
                {
                    wizardCount++;
                }
            }

            Assert.AreEqual(jesterCount, 4);
            Assert.AreEqual(wizardCount, 4);
        }
    }
}