using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class ClassTimeAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public ClassTimeAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Class classe = (Class)validationContext.ObjectInstance;

			if (classe.ClassDate.Date <= DateTime.Now.Date && classe.ClassDate.Date.Year > 1980)
			{
				if (classe.ClassDate.Date == DateTime.Now.Date && classe.StartTime > DateTime.Now.TimeOfDay)
					return new ValidationResult(ErrorMessage);

				return ValidationResult.Success;
			}

			if (classe.ClassDate.Date <= DateTime.Now.Date && classe.ClassDate.Date.Year > 1980)
			{
				if (classe.ClassDate.Date == DateTime.Now.Date && classe.EndTime > DateTime.Now.TimeOfDay)
					return new ValidationResult(ErrorMessage);

				return ValidationResult.Success;
			}

			return new ValidationResult(ErrorMessage);
		}
	}
}
