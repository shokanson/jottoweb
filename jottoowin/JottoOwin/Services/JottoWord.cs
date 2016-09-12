using System;
using System.Linq;

namespace JottoOwin.Services
{
    public class JottoWord
    {
        public JottoWord(string word)
        {
            if (word == null || word.Length != 5)
            {
                throw new ArgumentException("word length must be 5 letters exactly");
            }

            if (word.ToCharArray().Any(c => !char.IsLetter(c)))
            {
                throw new ArgumentException("word must contain only alphabetic characters");
            }
            // normalize to all caps
            Word = word.ToUpper();
        }

        public string Word { get; private set; }

        public bool IsJotto(string guess)
        {
            // normalize to all caps
            return Word == guess.ToUpper();
        }

        public int GetCount(string guess)
        {
            if (guess == null || guess.Length != 5)
            {
                throw new ArgumentException("word length must be 5 letters exactly");
            }

            // normalize to all caps
            guess = guess.ToUpper();

            return IsJotto(guess)
                ? 5
                : (from c in guess.ToCharArray()
                   group c by c into g1
                   let g2 = Word.ToCharArray().ToLookup(c => c)[g1.Key]
                   from c in (g1.Count() < g2.Count() ? g1 : g2)
                   select c).Count();
        }
    }
}