using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности Регион
	/// </summary>
	public class Region
	{
		public virtual ulong Id { get; set; }

		public virtual string Name { get; set; }
	}
}