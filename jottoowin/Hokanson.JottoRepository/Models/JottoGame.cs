using System;

namespace Hokanson.JottoRepository.Models
{
	public class JottoGame : ICloneable
	{
		public string Id { get; set; }
		public string Player1Id { get; set; }
		public string Player2Id { get; set; }
		public string Word1 { get; set; }
		public string Word2 { get; set; }
		public DateTime CreationDate { get; set; }
		public object Clone()
		{
			return new JottoGame { Id = this.Id, Player1Id = this.Player1Id, Player2Id = this.Player2Id, Word1 = this.Word1, Word2 = this.Word2 };
		}
	}
}
