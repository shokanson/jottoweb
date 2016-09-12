using System.Collections.Generic;

namespace JottoOwin.Models
{
    public class GameHelperModel
    {
        public GameHelperModel()
        {
            Clues = new List<string>();
        }
        public string KnownIn { get; set; }
        public string KnownOut { get; set; }
        public string Unknown { get; set; }
        public List<string> Clues { get; set; }
    }
}