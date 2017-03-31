using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Производитель, участвующий в акции
	/// </summary>
	public class PromoterProducer
	{
		public PromoterProducer()
		{
			Promotions = new List<ProducerPromotion>();
		}

		public virtual uint Id { get; set; }

		public virtual Promoter Promoter { get; set; }

		public virtual Producer Producer { get; set; }

		public virtual string Contacts { get; set; }

		public virtual IList<ProducerPromotion> Promotions { get; set; }
	}
}