using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class RegisterViewModel
	{
		public RegisterViewModel()
		{
			SendEmail = true;
		}

		[Required]
		[Display(Name = "Наименование")]
		[StringLength(255, ErrorMessage = "Поле \"{0}\" не может содержать больше {1} символов.")]
		public string Name { get; set; }

		[Display(Name = "Логин")]
		[StringLength(100, ErrorMessage = "Поле \"{0}\" не может содержать больше {1} символов.")]
		public string Login { get; set; }

		[Display(Name = "Пароль")]
		public string Password { get; set; }

		[Display(Name = "Отправить email")]
		public bool SendEmail { get; set; }
	}
}