using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности Позиция каталога
	/// </summary>
	public class Catalog
	{
		public virtual uint Id { get; set; }

		public virtual string Name { get; set; }
	}
}