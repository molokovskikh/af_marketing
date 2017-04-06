using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Организатор акции, пользователь сайта
	/// </summary>
	public class Promoter
	{
		public const string ACC_LOGIN_PREFIX = "Marketing_";

		public Promoter()
		{
			Producers = new List<PromoterProducer>();
			Members = new List<PromotionMember>();
		}

		public virtual uint Id { get; set; }

		[Display(Name = "Наименование")]
		public virtual string Name { get; set; }

		[Display(Name = "Логин")]
		public virtual string Login { get; set; }

		public virtual IList<PromoterProducer> Producers { get; set; }

		public virtual IList<PromotionMember> Members { get; set; }
	}
}