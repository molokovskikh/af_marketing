using Marketing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class ProfileViewModel
	{
		[Required(ErrorMessage = "Необходимо выбрать ассоциацию")]
		[Display(Name = "Текущая ассоциация")]
		public uint AssociationId { get; set; }

		public IList<AssociationViewModel> AvailableAssociations { get; set; }
	}
}