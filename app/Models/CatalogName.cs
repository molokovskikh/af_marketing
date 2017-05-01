using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности каталожного наименования
	/// </summary>
	public class CatalogName
	{
		public CatalogName()
		{
			Catalogs = new List<Catalog>();
		}

		public virtual uint Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Mnn Mnn { get; set; }
		public virtual IList<Catalog> Catalogs { get; set; }
	}
}