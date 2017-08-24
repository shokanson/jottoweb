using System.Collections.Generic;

namespace Hokanson.JottoRepository
{
    public interface IWordList
    {
        IEnumerable<string> GetAllWords();
        string GetRandomWord();
        bool IsWordInList(string word);
    }
}
