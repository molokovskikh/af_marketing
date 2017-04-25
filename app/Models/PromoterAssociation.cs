using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Ассоциации под управлением организатора маркетинговых мероприятий
	/// </summary>
	public class PromoterAssociation
	{
		public virtual uint Id { get; set; }
		public virtual Promoter Promoter { get; set; }
		public virtual Association Association { get; set; }
	}
}