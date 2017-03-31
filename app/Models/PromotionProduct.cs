using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Товар, участвующий в акции
	/// </summary>
	public class PromotionProduct
	{
		public virtual uint Id { get; set; }

		public virtual ProducerPromotion Promotion { get; set; }

		public virtual Product Product { get; set; }

		public virtual decimal Price { get; set; }

		public virtual decimal DealerPercent { get; set; }

		public virtual decimal MemberPercent { get; set; }
	}
}