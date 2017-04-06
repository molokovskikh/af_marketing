using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marketing.Models;

namespace Marketing.ViewModels
{
	public class PromoterProducersViewModel
	{
		public PromoterProducersViewModel()
		{
			ProducersList = new List<SelectListItem>();
		}

		public List<SelectListItem> ProducersList { get; set; }


		[Range(1, int.MaxValue, ErrorMessage = "Необходимо выбрать поставщика")]
		[Display(Name = "Поставщик")]
		public uint SelectedProducerId { get; set; }

		public uint AttachedProducerId { get; set; }

		[Display(Name = "Контакты")]
		public string Contacts { get; set; }

	}
}