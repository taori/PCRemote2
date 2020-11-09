using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Amusoft.PCR.Blazor.Services
{
	public interface IValidationService
	{
		bool TryValidate(object model, out ICollection<ValidationResult> results);
	}

	public class ValidationService : IValidationService
	{
		public bool TryValidate(object model, out ICollection<ValidationResult> results)
		{
			results = new List<ValidationResult>();
			return Validator.TryValidateObject(model, new ValidationContext(model), results);
		}
	}
}