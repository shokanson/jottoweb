using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hokanson.JottoHelper
{
	public class GuessInfo
	{
		public GuessInfo(string word, int score)
		{
			_word = word;
			_score = score;
		}

		private readonly string _word;
		private readonly int _score;

		public Tuple<string, int> GetRemaining(string knownIn, string knownOut)
		{
			char[] remainingChars = _word.ToArray();
			int remainingScore = _score;
			foreach (var pos in HandleKnown(knownIn, true))
			{
				remainingChars[pos] = '-';
				remainingScore--;
			}
			foreach (var pos in HandleKnown(knownOut))
			{
				remainingChars[pos] = '-';
			}
			return new Tuple<string, int>(new string(remainingChars.Where(c => c != '-').ToArray()), remainingScore);
		}

		private IEnumerable<int> HandleKnown(string known, bool forIn = false)
		{
			char[] knownChars = known.ToArray();
			var knownPositions = new HashSet<int>();
			char[] wordChars = _word.ToArray();
			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < knownChars.Length; j++)
				{
					if (wordChars[i] == knownChars[j] && !knownPositions.Contains(i))
					{
						knownPositions.Add(i);
						if (forIn) knownChars[j] = '-';    // to prevent double-processing
					}
				}
			}
			return knownPositions;
		}
	}

	public class JottoGameHelper
	{
		private readonly Dictionary<string, int> _guesses = new Dictionary<string, int>();
		private readonly Dictionary<char, int> _knownIn = new Dictionary<char, int>();
		private readonly List<char> _knownOut = new List<char>();
		private Dictionary<string, int> _clues;

        public IReadOnlyDictionary<char, int> KnownIn => _knownIn;
        public IReadOnlyCollection<char> KnownOut => _knownOut;
        public IReadOnlyDictionary<string, int> Clues => _clues;

        public void HandleGuess(string word, int score)
		{
			if (string.IsNullOrEmpty(word) || word.Length != 5) throw new ArgumentException("word must be five letters long");
			// normalize before proceeding
			word = word.ToLower();
			if (word.ToArray().Any(c => !char.IsLetter(c))) throw new ArgumentException("word must contain only letters");
			if (score < 0 || score > 5) throw new ArgumentException("score must be 0-5");
			if (_guesses.ContainsKey(word)) throw new ArgumentException("that word has already been guessed");

			_guesses[word] = score;

			SetClues();
		}

		public void AddKnownIn(char c)
		{
			if (_knownOut.Contains(c)) throw new ArgumentException("already exists in KnownOut--cannot add to KnownIn");

			if (_knownIn.ContainsKey(c)) _knownIn[c]++;
			else _knownIn[c] = 1;

			SetClues();
		}

		public void AddKnownOut(char c)
		{
			if (_knownIn.ContainsKey(c)) throw new ArgumentException("already exists in KnownIn--cannot add to KnownOut");

			if (!_knownOut.Contains(c)) _knownOut.Add(c);

			SetClues();
		}

		public void RemoveKnownIn(char c)
		{
			if (_knownIn.ContainsKey(c))
			{
				if (_knownIn[c] > 1) _knownIn[c]--;
				else _knownIn.Remove(c);
			}

			SetClues();
		}

		public void RemoveKnownOut(char c)
		{
			if (_knownOut.Contains(c)) _knownOut.Remove(c);

			SetClues();
		}

		public void Reset()
		{
			_knownIn.Clear();
			_knownOut.Clear();
			SetClues();
			SetClues();	// ensure clues take into account KnownIn/KnownOut--hack until I can figure out how to do this within one call to SetClues
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			IEnumerable<char> unknownCharList = "abcdefghijklmnopqrstuvwxyz".Except(KnownInStr.ToArray()).Except(_knownOut);
			sb.Append($"Known in:  {new string(KnownInStr.ToArray().OrderBy(c => c).ToArray())}\r\n");
			sb.Append($"Known out: {new string(_knownOut.OrderBy(c => c).ToArray())}\r\n");
			sb.Append($"Unknown:   {new string(unknownCharList.ToArray())}\r\n");
			sb.Append("Clues:\r\n");
			foreach (var clue in Clues.Keys)
			{
				sb.Append($"\t{clue}: {Clues[clue]}\r\n");
			}

			return sb.ToString();
		}

		private void SetClues()
		{
			bool done;
			Dictionary<string, int> clues;
			do
			{
				done = true;
				clues = new Dictionary<string, int>();

				foreach (var pair in _guesses)
				{
					var info = new GuessInfo(pair.Key, pair.Value);
					Tuple<string, int> remaining = info.GetRemaining(KnownInStr, KnownOutStr);

					if (remaining.Item2 == 0 && remaining.Item1.Length > 0)
					{
						if (remaining.Item1.Length == 1 && _knownIn.ContainsKey(remaining.Item1[0]))
						{
							// Hmmm....this seems like a special case (see TestGame9), but I'm not sure what
							// the general rule is.

							// don't need to do anything with clues or known in/out
						}
						else
						{
							done = !AddRemainingToKnownOut(remaining.Item1);
						}
					}
					else if (remaining.Item2 > 0 && remaining.Item1.Length == remaining.Item2)
					{
						AddRemainingToKnownIn(remaining.Item1);
						done = false;
					}
                    else if (remaining.Item2 > 0 && AllTheSameLetter(remaining.Item1))
                    {
                        AddRemainingToKnownIn(new string(remaining.Item1[0], remaining.Item2));
                        done = false;
                    }
					else if (remaining.Item1.Length > 0)
					{
						var clue = new string(remaining.Item1.ToArray().OrderBy(c => c).ToArray());
						if (clues.ContainsKey(clue))
						{
							if (clues[clue] != remaining.Item2)
							{
								//throw new Exception("programming error: somehow wound up with same clue but different score");
							}
						}
						else
						{
							clues[clue] = remaining.Item2;
						}
					}
				}

			} while (!done);
			_clues = clues;
		}

        private bool AllTheSameLetter(string s)
        {
            return s.GroupBy(c => c).Count() == 1;
        }

        private void AddRemainingToKnownIn(string s)
		{
			foreach (var c in s)
			{
				if (_knownIn.ContainsKey(c)) _knownIn[c]++;
				else _knownIn[c] = 1;
			}
		}

		private bool AddRemainingToKnownOut(string s)
		{
			bool changed = false;
			// note: it may be that there were duplicate letters in the guess that aren't in the jotto word,
			// so we have to make sure we don't remove it from knownIn if it's already there
			foreach (var c in s)
			{
				if (!_knownIn.ContainsKey(c) && !_knownOut.Contains(c))
				{
					_knownOut.Add(c);
					changed = true;
				}
			}
			return changed;
		}

		private string KnownInStr
		{
			get
			{
				var builder = new StringBuilder();

				foreach (var pair in _knownIn)
				{
					for (int i = 0; i < pair.Value; i++) builder.Append(pair.Key);
				}

				return builder.ToString();
			}
		}

        private string KnownOutStr => new string(_knownOut.ToArray());
    }
}