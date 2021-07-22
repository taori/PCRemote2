using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Amusoft.PCR.Model.Entities
{
	public class Permission
	{
		[ForeignKey(nameof(UserId))]
		public ApplicationUser User { get; set; }

		[MaxLength(450)]
		public string UserId { get; set; }
		
		public PermissionKind PermissionType { get; set; }

		[MaxLength(40)]
		public string SubjectId { get; set; }
	}
}