using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Hokanson.JottoRepository;

namespace MSTest.Hokanson.JottoRepository
{
    [TestClass]
    public class TestJottoRepository
    {
        [DataTestMethod]
        [DataRow("abcde")]
        [DataRow("ABCDE")]
        [DataRow("Abcde")]
        [DataRow("AbCdE")]
        [DataRow("aBcDe")]
        [Ignore]
        public void IsWordAsync_handles_all_casing(string word)
        {
            /*
            // arrange
            string actual = null;
            var mockWordList = new Mock<IWordList>();
            mockWordList.Setup(wl => wl.IsWordInList(It.IsAny<string>()))
                        .Callback<string>(s => actual = s);

            var repo = new global::Hokanson.JottoRepository.JottoRepository(mockWordList.Object);

            // act
            repo.IsWordAsync(word).Wait();

            // assert
            Assert.AreEqual("abcde", actual);
            */
        }
    }
}
