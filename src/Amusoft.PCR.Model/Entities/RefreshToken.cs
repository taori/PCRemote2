using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amusoft.PCR.Model.Entities
{
	public class RefreshToken
	{
		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }
		public string UserId { get; set; }

		[MaxLength(50)]
		public string RefreshTokenId { get; set; }

		public bool IsUsed { get; set; }

		public DateTime ValidUntil { get; set; }

		public DateTime IssuedAt { get; set; }
	}
}