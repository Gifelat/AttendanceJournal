using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class ClassAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public ClassAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Class classe = (Class)validationContext.ObjectInstance;
			var check = _context.Classes.FirstOrDefault(u => u.Theme == (string)value && u.ClassID != classe.ClassID);

			if (check != null)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}
	}
}
