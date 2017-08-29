namespace Hokanson.JottoRepository.Models
{
    public class PlayerGuess
    {
        public string Id { get; set; }
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public string Word { get; set; }
        public int Score { get; set; }  // derived--computed from player1's opponent's word for this game
    }
}
