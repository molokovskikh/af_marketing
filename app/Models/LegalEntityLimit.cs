using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	public class LegalEntityLimit
	{
		public virtual uint Id { get; set; }
		public virtual decimal? MinSum { get; set; }
		public virtual PromotionMember Member { get; set; }
		public virtual LegalEntity LegalEntity { get; set; }
	}
}