using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class DateRangeViewModel
	{
		public DateTime DateBegin { get; set; }
		public DateTime DateEnd { get; set; }
		public List<DateTime> DateBeginDisabledDates { get; set; }
		public DateTime? DateBeginMax { get; set; }
		public DateTime? DateBeginMin { get; set; }
		public List<DateTime> DateEndDisabledDates { get; set; }
		public DateTime? DateEndMax { get; set; }
		public DateTime? DateEndMin { get; set; }
	}
}