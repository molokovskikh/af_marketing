using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Ограничение по минимальной сумме для аптеки
	/// </summary>
	public class AddressLimit
	{
		public virtual uint Id { get; set; }
		public virtual decimal? MinSum { get; set; }
		public virtual PromotionMember Member { get; set; }
		public virtual Address Address { get; set; }
	}
}