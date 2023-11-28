using System.ComponentModel.DataAnnotations;

namespace ASP_Server.Models
{
	public class PasswordConfirmationModel
	{
		public string TeacherID { get; set; } = null!;

		[Required(ErrorMessage = "Текущий пароль не указан")]
		[StringLength(20, MinimumLength = 4, ErrorMessage = "Пароль должен быть от 4 символов")]
		[Display(Name = "Текущий пароль пользователя")]
		public string Password { get; set; } = null!;

		[Required(ErrorMessage = "Новый пароль не указан")]
		[StringLength(20, MinimumLength = 4, ErrorMessage = "Пароль должен быть от 4 символов")]
		[Display(Name = "Новый пароль пользователя")]
		public string NewPassword { get; set; } = null!;

		[Required(ErrorMessage = "Пароль подтверждения не указан")]
		[StringLength(20, MinimumLength = 4, ErrorMessage = "Пароль должен быть от 4 символов")]
		[Display(Name = "Подтвердите пароль")]
		[Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
		public string ConfirmPassword { get; set; } = null!;
	}
}
