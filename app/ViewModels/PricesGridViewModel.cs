using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.ViewModels
{
	public class PricesGridViewModel
	{
		public virtual uint PriceId { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime PriceDate { get; set; }
	}
}