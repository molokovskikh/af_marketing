using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Юридическое лицо.
	/// Связано с клиентом через плательщика.
	/// </summary>
	public class LegalEntity
	{
		public virtual uint Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string ShortName { get; set; }
		public virtual Payer Payer { get; set; }
	}
}