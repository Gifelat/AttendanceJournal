using System.ComponentModel.DataAnnotations;

namespace AttendanceJournalLibrary.Validation
{
	internal class UserLoginAttribute : ValidationAttribute
	{
		private readonly AttendanceJournalContext _context;
		public UserLoginAttribute() 
		{
			_context = new AttendanceJournalContext();
		}

		protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
		{
			Teacher user = (Teacher)validationContext.ObjectInstance;
			var check = _context.Teachers.FirstOrDefault(u => u.Login == (string)value && u.TeacherID != user.TeacherID);

			if (check != null)
				return new ValidationResult(ErrorMessage);

			return ValidationResult.Success;
		}
	}
}
