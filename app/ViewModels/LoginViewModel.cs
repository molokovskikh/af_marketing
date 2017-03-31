using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class LoginViewModel
	{
		[Display(Name = "Логин")]
		[Required]
		public string Login { get; set; }

		[Display(Name = "Пароль")]
		[Required]
		public string Password { get; set; }

		[Display(Name = "Запомнить меня")]
		public bool RememberMe { get; set; }
	}
}