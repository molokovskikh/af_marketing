using Marketing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class MarketingEventViewModel
	{
		public uint MarketingEventId { get; set; }
		[Display(Name = "Наименование")]
		public string Name { get; set; }
		public IList<Producer> AvailableProducers { get; set; }
		public string SelectedProducerIds { get; set; }
	}
}