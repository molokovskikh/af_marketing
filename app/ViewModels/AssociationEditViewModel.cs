using Marketing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class AssociationEditViewModel
	{
		public AssociationEditViewModel()
		{
			AvailableRegions = new List<Region>();
			Contacts = new List<AssociationContact>();
		}

		public uint? AssociationId { get; set; }

		[Display(Name = "Наименование")]
		[Required]
		[StringLength(255, ErrorMessage = "Длина поля {0} не может превышать {1}")]
		public string Name { get; set; }

		[Display(Name = "Примечания")]
		[StringLength(2000, ErrorMessage = "Длина поле {0} не может превышать {1}")]
		public string Comments { get; set; }

		public string SelectedRegionIds { get; set; }
		public IList<Region> AvailableRegions { get; set; }

		public IList<AssociationContact> Contacts { get; set; }
	}
}