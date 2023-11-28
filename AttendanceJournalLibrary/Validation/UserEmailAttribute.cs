using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class UserEmailAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public UserEmailAttribute()
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Teacher user = (Teacher)validationContext.ObjectInstance;
			var check = _context.Teachers.FirstOrDefault(u => u.Email == (string)value && u.TeacherID != user.TeacherID);

			if (check != null)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}
	}
}
