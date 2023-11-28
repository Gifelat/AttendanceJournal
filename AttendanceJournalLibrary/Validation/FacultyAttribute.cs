using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class FacultyAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public FacultyAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Faculty faculty = (Faculty)validationContext.ObjectInstance;
			var check = _context.Faculties.FirstOrDefault(u => u.FacultyName == (string)value && u.FacultyID != faculty.FacultyID);

			if (check != null)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}
	}
}
