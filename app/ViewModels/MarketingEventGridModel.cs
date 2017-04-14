using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class MarketingEventGridModel
	{
		public uint MarketingEventId { get; set; }
		public string Name { get; set; }
		public string Producers { get; set; }
		public int PromotionCount { get; set; }
		public int EnabledPromotionCount { get; set; }
		public int ActivePromotionCount { get; set; }
	}
}