using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности Торговая точка клиента
	/// </summary>
	public class Address
	{
		public virtual uint Id { get; set; }

		public virtual string AddressName { get; set; }

		public virtual bool Enabled { get; set; }

		public virtual Client Client { get; set; }
	}
}