using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для контактов
	/// </summary>
	public class Contact
	{
		public virtual uint Id { get; set; }

		public virtual ContactType ContactType { get; set; }

		public virtual string Comment { get; set; }

		public virtual string ContactText { get; set; }

		public virtual uint ContactOwnerId { get; set; }
	}

	public enum ContactType
	{
		[Description("E-mail")]
		Email = 0,
		[Description("Телефон")]
		Phone = 1,
		[Description("Почтовый адрес")]
		MailingAddress = 2,
		[Description("Факс")]
		Fax = 3,
	}
}