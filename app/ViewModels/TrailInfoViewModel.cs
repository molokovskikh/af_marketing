using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class TrailInfoViewModel
	{
		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Email")]
		public string Email { get; set; }

		[Display(Name = "Телефон")]
		public string Phone { get; set; }
	}
}