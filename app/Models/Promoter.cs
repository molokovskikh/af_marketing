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
			Associations = new List<PromoterAssociation>();
		}

		public virtual uint Id { get; set; }

		[Display(Name = "Наименование")]
		public virtual string Name { get; set; }

		[Display(Name = "Логин")]
		public virtual string Login { get; set; }

		public virtual uint? LastAssociationId { get; set; }

		public virtual IList<PromoterAssociation> Associations { get; set; }
	}
}