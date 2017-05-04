using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Сумма вознаграждения по акции в зависимости от количества упаковок
	/// </summary>
	public class PromotionSum
	{
		public virtual uint Id { get; set; }
		public virtual ProducerPromotion Promotion { get; set; }
		public virtual uint Quantity { get; set; }
		public virtual decimal DealerSum { get; set; }
		public virtual decimal MemberSum { get; set; }
	}
}