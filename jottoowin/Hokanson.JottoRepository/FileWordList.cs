using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Hokanson.JottoRepository
{
    public class FileWordList : IWordList
    {
        public FileWordList(string fileName)
        {
            System.Diagnostics.Trace.TraceInformation(fileName);
            using (var reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line) && line.Length == 5) _words.Add(line.ToLower());
                }
            }
        }

        private readonly HashSet<string> _words = new HashSet<string>();

        public IEnumerable<string> GetAllWords() => _words;

        public string GetRandomWord() => _words.Skip(new Random().Next(0, _words.Count)).Take(1).First();

        public bool IsWordInList(string word) => _words.Contains(word, StringComparer.InvariantCultureIgnoreCase);
    }
}
