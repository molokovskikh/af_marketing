﻿using Marketing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class ClientViewModel
	{
		public uint ClientId { get; set; }

		[Display(Name = "Наименование")]
		public string Name { get; set; }

		[Display(Name = "Регион")]
		public string RegionName { get; set; }

		[Display(Name = "Email")]
		[EmailAddress]
		public string Email { get; set; }

		[Display(Name = "Телефон")]
		[Phone]
		public string Phone { get; set; }

		public IList<Address> Addresses { get; set; }
	}
}