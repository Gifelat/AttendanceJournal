using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class ClassDateAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public ClassDateAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Class classe = (Class)validationContext.ObjectInstance;

			if (classe.ClassDate.Date <= DateTime.Now.Date && classe.ClassDate.Date.Year > 1980)
				return ValidationResult.Success;

			return new ValidationResult(ErrorMessage);
		}
	}
}
