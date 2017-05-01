using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для сущности МНН
	/// </summary>
	public class Mnn
	{
		public virtual uint Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string RussianMnn { get; set; }
	}
}