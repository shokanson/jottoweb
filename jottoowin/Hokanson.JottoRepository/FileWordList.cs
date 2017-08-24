﻿using System;
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
                    if (!string.IsNullOrEmpty(line)) _words.Add(line);
                }
            }
        }

        private readonly HashSet<string> _words = new HashSet<string>();

        public IEnumerable<string> GetAllWords()
        {
            return _words;
        }

        public string GetRandomWord()
        {
            return _words.Skip(new Random().Next(0, _words.Count))
                         .Take(1)
                         .First();
        }

        public bool IsWordInList(string word)
        {
            return _words.Contains(word);
        }
    }
}