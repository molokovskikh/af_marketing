using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности Товар
	/// </summary>
	public class Product
	{
		public virtual uint Id { get; set; }

		public virtual bool Hidden { get; set; }

		public virtual Catalog Catalog { get; set; }
	}
}