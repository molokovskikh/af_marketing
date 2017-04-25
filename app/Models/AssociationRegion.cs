using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Регионы работы ассоциации производителей
	/// </summary>
	public class AssociationRegion
	{
		public virtual uint Id { get; set; }
		public virtual Association Association { get; set; }
		public virtual Region Region { get; set; }
	}
}