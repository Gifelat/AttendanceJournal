using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class SubjectAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public SubjectAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Subject subjectName = (Subject)validationContext.ObjectInstance;
			var check = _context.Subjects.FirstOrDefault(u => u.SubjectName == (string)value && u.SubjectID != subjectName.SubjectID);

			if (check != null)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}
	}
}