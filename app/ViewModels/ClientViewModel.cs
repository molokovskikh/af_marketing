using Marketing.Models;
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

		public IList<Address> Addresses { get; set; }
	}
}