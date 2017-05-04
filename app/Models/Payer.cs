using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	public class Payer
	{
		/// <summary>
		/// Заглушка для сущности Плательщик
		/// </summary>
		public Payer()
		{
			LegalEntities = new List<LegalEntity>();
		}

		public virtual uint Id { get; set; }
		public virtual string ShortName { get; set; }
		public virtual string JuridicalName { get; set; }
		public virtual string JuridicalAddress { get; set; }

		public virtual IList<LegalEntity> LegalEntities { get; set; }
	}
}