using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	public class CatalogForm
	{
		public virtual uint Id { get; set; }
		public virtual string Form { get; set; }
		public virtual Catalog Catalog { get; set; }
	}
}