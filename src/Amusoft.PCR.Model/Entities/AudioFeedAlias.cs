using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amusoft.PCR.Model.Entities
{
	public class AudioFeedAlias
	{
		[MaxLength(45)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string Id { get; set; }

		public string FeedId { get; set; }

		[ForeignKey(nameof(FeedId))]
		public AudioFeed Feed { get; set; }

		public string Alias { get; set; }
	}
}