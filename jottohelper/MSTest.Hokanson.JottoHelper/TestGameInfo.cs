using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Hokanson.JottoHelper;

namespace MSTest.Hokanson.JottoHelper
{
    [TestClass]
    public class TestGameInfo
    {
        // test: GetRemaining handles empty strings
        [TestMethod]
        public void GetRemaining_Empty()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, string.Empty);

            // assert
            Assert.AreEqual("wordy", remaining.Item1);
            Assert.AreEqual(3, remaining.Item2);
        }

        // test: GetRemaining, no "in" overlap, leaves remaining as is
        [TestMethod]
        public void GetRemaining_NoInOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("s", string.Empty);
            
            // assert
            Assert.AreEqual("wordy", remaining.Item1);
            Assert.AreEqual(3, remaining.Item2);
        }

        // test: GetRemaining, called once, with "in" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_WithInOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("w", string.Empty);

            // assert
            Assert.AreEqual("ordy", remaining.Item1);
            Assert.AreEqual(2, remaining.Item2);
        }

        // test: GetRemaining, with full "in" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_FullInOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 5);

            // act
            Tuple<string, int> remaining = info.GetRemaining("dorwy", string.Empty);

            // assert
            Assert.AreEqual(string.Empty, remaining.Item1);
            Assert.AreEqual(0, remaining.Item2);
        }

        // test: GetRemaining, no "out" overlap, leaves remaining as is
        [TestMethod]
        public void GetRemaining_NoOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, "s");

            // assert
            Assert.AreEqual("wordy", remaining.Item1);
            Assert.AreEqual(3, remaining.Item2);
        }

        // test: GetRemaining, with "out" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_WithOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, "w");

            // assert
            Assert.AreEqual("ordy", remaining.Item1);
            Assert.AreEqual(3, remaining.Item2);
        }

        // test: GetRemaining, with some "out" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_SomeOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 1);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, "doy");

            // assert
            Assert.AreEqual("wr", remaining.Item1);
            Assert.AreEqual(1, remaining.Item2);
        }

        // test: GetRemaining, with full "out" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_FullOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 0);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, "dorwy");

            // assert
            Assert.AreEqual(string.Empty, remaining.Item1);
            Assert.AreEqual(0, remaining.Item2);
        }

        // test: GetRemaining, with superset "out" overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_SupersetOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 0);

            // act
            Tuple<string, int> remaining = info.GetRemaining(string.Empty, "abdorwxyz");

            // assert
            Assert.AreEqual(string.Empty, remaining.Item1);
            Assert.AreEqual(0, remaining.Item2);
        }

        // test: GetRemaining, with some "in", some "out", reports remaining accordingly
        [TestMethod]
        public void GetRemaining_SomeInSomeOutOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("ow", "abdrxz");

            // assert
            Assert.AreEqual("y", remaining.Item1);
            Assert.AreEqual(1, remaining.Item2);
        }

        // test: GetRemaining, with duplicates in "in", with overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_DuplicatesInIn_Overlap()
        {
            // arrange
            var info = new GuessInfo("woody", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("oow", string.Empty);

            // assert
            Assert.AreEqual("dy", remaining.Item1);
            Assert.AreEqual(0, remaining.Item2);
        }

        // test: GetRemaining, with duplicates in "in", with some overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_DuplicatesInIn_SomeOverlap()
        {
            // arrange
            var info = new GuessInfo("wordy", 2);

            // act
            Tuple<string, int> remaining = info.GetRemaining("oow", string.Empty);

            // assert
            Assert.AreEqual("rdy", remaining.Item1);
            Assert.AreEqual(0, remaining.Item2);
        }

        // test: GetRemaining, no duplicates in "in", with some overlap, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_NoDuplicatesInIn_SomeOverlap()
        {
            // arrange
            var info = new GuessInfo("woody", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("ow", string.Empty);

            // assert
            Assert.AreEqual("ody", remaining.Item1);
            Assert.AreEqual(1, remaining.Item2);
        }

        // test: GetRemaining,  duplicates in guess, reports remaining accordingly
        [TestMethod]
        public void GetRemaining_DuplicatesInGuess()
        {
            // arrange
            var info = new GuessInfo("troll", 3);

            // act
            Tuple<string, int> remaining = info.GetRemaining("at", "l");

            // assert
            Assert.AreEqual("ro", remaining.Item1);
            Assert.AreEqual(2, remaining.Item2);
        }
    }
}
