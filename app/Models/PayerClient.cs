using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Привязка плательщика к клиенту
	/// </summary>
	public class PayerClient
	{
		public virtual Client Client { get; set; }
		public virtual Payer Payer { get; set; }
	}
}