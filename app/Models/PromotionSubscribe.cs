using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Подписка участника на акцию
	/// </summary>
	public class PromotionSubscribe
	{
		public virtual uint Id { get; set; }

		public virtual PromotionMember Member { get; set; }

		public virtual ProducerPromotion Promotion { get; set; }
	}
}