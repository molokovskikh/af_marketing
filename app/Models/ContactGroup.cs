using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace Marketing.Models
{
	/// <summary>
	/// Заглушка для групп контактов
	/// </summary>
	public class ContactGroup
	{
		public virtual uint Id { get; set; }

		public virtual string Name { get; set; }

		public virtual ContactGroupType ContactGroupTypeId { get; set; }

		public virtual IList<Contact> Contacts { get; set; }

		public virtual uint ContactGroupOwnerId { get; set; }
	}

	public enum ContactGroupType : short
	{
		[Description("Общая информация")]
		General = 0,
		[Description("Администратор клиентов в автоматизированной системе")]
		ClientManagers = 1,
		[Description("Менеджер прайс листов(заказов)")]
		OrderManagers = 2,
		[Description("Бухгалтер по расчетам с АналитФармация")]
		AccountantManagers = 3,
		[Description("Контактная информация для биллинга")]
		Billing = 4,
		[Description("Дополнительные контакты")]
		Custom = 5,
		[Description("Рассылка отчетов")]
		Reports = 6,
		[Description("Рассылка заказов")]
		OrdersDelivery = 7,
		[Description("Рассылка счетов")]
		Invoice = 8,
		[Description("Известные телефоны")]
		KnownPhones = 9,
		[Description("Список E-mail, с которых разрешена отправка писем клиентам АналитФармация")]
		MiniMails = 10,
		//Список контактов для подписки на отчеты
		[Description("Самостоятельная подписка на отчеты")]
		ReportSubscribers = 11,
		[Description("Список адресов, на которые не будет отправлять письма спаморезка")]
		MiniMailNoSend = 12,
		[Description("Маркетинг")]
		Marketing = 13
	}
}