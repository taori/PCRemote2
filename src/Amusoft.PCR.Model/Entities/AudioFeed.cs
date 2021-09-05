using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amusoft.PCR.Model.Entities
{
	public class AudioFeed
	{
		[MaxLength(45)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public string Id { get; set; }

		public string Name { get; set; }

		public ICollection<AudioFeedAlias> Aliases { get; set; }
	}
}