using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Участник акции, клиент
	/// </summary>
	public class PromotionMember
	{
		public PromotionMember()
		{
			Subscribes = new List<PromotionSubscribe>();
		}

		public virtual uint Id { get; set; }

		public virtual Promoter Promoter { get; set; }

		public virtual Client Client { get; set; }

		public virtual IList<PromotionSubscribe> Subscribes { get; set; }
	}
}