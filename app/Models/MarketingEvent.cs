using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Маркетинговое мероприятие
	/// </summary>
	public class MarketingEvent
	{
		public MarketingEvent()
		{
			Producers = new List<PromoterProducer>();
			Promotions = new List<ProducerPromotion>();
		}

		public virtual uint Id { get; set; }

		public virtual string Name { get; set; }

		public virtual Promoter Promoter { get; set; }

		public virtual IList<PromoterProducer> Producers { get; set; }

		public virtual IList<ProducerPromotion> Promotions { get; set; }
	}
}