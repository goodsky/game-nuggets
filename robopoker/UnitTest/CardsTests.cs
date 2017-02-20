using RoboPoker;
using TableRunner;

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Test the Cards and Decks.
    /// </summary>
    [TestClass]
    public class CardsTests
    {
        /// <summary>
        /// Make sure we can create a basic deck.
        /// Iterate through all the cards to make sure we have them all.
        /// </summary>
        [TestMethod]
        public void TestDeckInitialization()
        {
            Deck testDeck = new Deck();

            HashSet<Card> verifyUniqueness = new HashSet<Card>();
            while (testDeck.CardsLeft > 0)
            {
                Card topCard = testDeck.Draw();
                Assert.IsTrue(verifyUniqueness.Add(topCard), "The deck contained two of the same card!");
                Console.WriteLine("{0} of {1}s", topCard.Value.ToString(), topCard.Suit.ToString());
            }

            Assert.AreEqual(52, verifyUniqueness.Count, "The deck was missing cards!");
        }

        /// <summary>
        /// Make sure the Deck is working and can be shuffled correctly.
        /// Iterate through all the cards to make sure we have them all.
        /// </summary>
        [TestMethod]
        public void TestDeckShuffling()
        {
            Deck testDeck = new Deck();
            testDeck.Shuffle();

            HashSet<Card> verifyUniqueness = new HashSet<Card>();
            while (testDeck.CardsLeft > 0)
            {
                Card topCard = testDeck.Draw();
                Assert.IsTrue(verifyUniqueness.Add(topCard), "The deck contained two of the same card!");
                Console.WriteLine("{0} of {1}s", topCard.Value.ToString(), topCard.Suit.ToString());
            }

            Assert.AreEqual(52, verifyUniqueness.Count, "The deck was missing cards!");
        }

        /// <summary>
        /// Test sorting all the cards after a random shuffle
        /// </summary>
        [TestMethod]
        public void TestCardSortingByValue()
        {
            // Sort by suit first and make sure it is sorted back to the default initialized deck
            Deck inOrderDeck = new Deck();
            Deck shuffledDeck = new Deck();
            shuffledDeck.Shuffle();

            Card[] allTheCards = new Card[52];
            for (int i = 0; i < 52; ++i)
                allTheCards[i] = shuffledDeck.Draw();

            Array.Sort(allTheCards, PokerUtils.CompareBySuits);

            for (int i = 0; i < 52; ++i)
                Assert.AreEqual(allTheCards[i], inOrderDeck.Draw());
        }

        /// <summary>
        /// Test sorting all the cards after a random shuffle
        /// </summary>
        [TestMethod]
        public void TestCardSortingBySuit()
        {
            // Sort by suit first and make sure it is sorted back to the default initialized deck
            Deck inOrderDeck = new Deck();
            Deck shuffledDeck = new Deck();
            shuffledDeck.Shuffle();

            Card[] allTheCards = new Card[52];
            for (int i = 0; i < 52; ++i)
                allTheCards[i] = shuffledDeck.Draw();

            Array.Sort(allTheCards, PokerUtils.CompareBySuits);

            for (int i = 0; i < 52; ++i)
                Assert.AreEqual(allTheCards[i], inOrderDeck.Draw());
        }
    }
}
