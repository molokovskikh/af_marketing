using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Вознаграждения по акции в процентах в зависмости от общей суммы
	/// </summary>
	public class PromotionPercent
	{
		public virtual uint Id { get; set; }
		public virtual ProducerPromotion Promotion { get; set; }
		public virtual decimal Total { get; set; }
		public virtual decimal DealerPercent { get; set; }
		public virtual decimal MemberPercent { get; set; }
	}
}