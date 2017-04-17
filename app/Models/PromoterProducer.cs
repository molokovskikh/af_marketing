using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Dapper;
using NHibernate;

namespace Marketing.Models
{
	/// <summary>
	/// Производитель, участвующий в акции
	/// </summary>
	public class PromoterProducer
	{
		public virtual uint Id { get; set; }

		public virtual MarketingEvent MarketingEvent { get; set; }

		public virtual Producer Producer { get; set; }

		[Display(Name="Контакты")]
		public virtual string Contacts { get; set; }

	}
}