using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Поставщик, участвующий в акции
	/// </summary>
	public class PromotionSupplier
	{
		public virtual uint Id { get; set; }

		public virtual ProducerPromotion Promotion { get; set; }

		public virtual Supplier Supplier { get; set; }
	}
}