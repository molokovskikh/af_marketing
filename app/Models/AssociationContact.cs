using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Контактная информация по ассоциации производителей
	/// </summary>
	public class AssociationContact
	{
		public virtual uint Id { get; set; }
		public virtual Association Association { get; set; }
		public virtual AssociationContactType ContactType { get; set; }
		public virtual string Fio { get; set; }
		public virtual string Phone { get; set; }
		public virtual string Email { get; set; }
	}

	public enum AssociationContactType : byte
	{
		[Description("Руководитель")] Chief = 0,
		[Description("Рабочий")] Active,
		[Description("IT-специалисты")] IT
	}
}