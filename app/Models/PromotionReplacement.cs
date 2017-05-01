using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	public class PromotionReplacement
	{
		public virtual uint Id { get; set; }
		public virtual PromotionProduct PromotionProduct { get; set; }
		public virtual Catalog Catalog { get; set; }
	}
}