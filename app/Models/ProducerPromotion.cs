using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Акция производителя
	/// </summary>
	public class ProducerPromotion
	{
		public virtual uint Id { get; set; }

		public virtual string Name { get; set; }

		public virtual PromoterProducer Producer { get; set; }

		public virtual IList<PromotionProduct> Products { get; set; }

		public virtual IList<PromotionSupplier> Suppliers { get; set; }

		public virtual IList<PromotionSubscribe> Subscribes { get; set; }
	}
}