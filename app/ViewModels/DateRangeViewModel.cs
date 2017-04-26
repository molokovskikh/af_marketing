using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class DateRangeViewModel
	{
		[Display(Name = "Дата начала")]
		public DateTime? DateBegin { get; set; }

		[Display(Name = "Дата окончания")]
		public DateTime? DateEnd { get; set; }
		public List<DateTime> DateBeginDisabledDates { get; set; }
		public DateTime? DateBeginMax { get; set; }
		public DateTime? DateBeginMin { get; set; }
		public List<DateTime> DateEndDisabledDates { get; set; }
		public DateTime? DateEndMax { get; set; }
		public DateTime? DateEndMin { get; set; }
	}
}