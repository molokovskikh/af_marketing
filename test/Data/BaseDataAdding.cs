using System;
using System.Linq;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;

namespace test.Data
{
	[TestFixture]
	public class BaseDataAdding : BaseFixture
	{
		public static Promoter AddPromoter(ISession dbSession)
		{
			var newItem = new Promoter();
			newItem.Name = $"Тестовый пользователь";
			newItem.Login = $"login_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 5)}";
			dbSession.Save(newItem);
			return newItem;
		}

		public static Producer AddProducer(ISession dbSession)
		{
			var newItem = new Producer();
			newItem.Name = "Тестовый поставщик";
			dbSession.Save(newItem);
			return newItem;
		}

		public static Association AddAssociation(ISession dbSession, Promoter promoter)
		{
			var newItem = new Association {
				Name = "new Association"
			};
			dbSession.Save(newItem);
			var link = new PromoterAssociation {
				Association = newItem,
				Promoter = promoter
			};
			dbSession.Save(link);
			newItem.Promoters.Add(link);
			return newItem;
		}

		public static MarketingEvent AddMarketingEvent(ISession dbSession, Association association)
		{
			var newItem = new MarketingEvent();
			newItem.Name = " new MarketingEvent";
			newItem.Association = association;
			dbSession.Save(newItem);
			return newItem;
		}

		/// <summary>
		///   Добавление поставщика для пользователя
		/// </summary>
		/// <param name="dbSession"></param>
		/// <param name="promoter"></param>
		/// <param name="producer"></param>
		/// <param name="contacts">По умолчанию "+7 (000) 00 00"</param>
		/// <returns></returns>
		public static PromoterProducer AddPromoterProducer(ISession dbSession, Promoter promoter, Producer producer,
			MarketingEvent marketingEvent,
			string contacts = null)
		{
			var newItem = new PromoterProducer();
			// todo исправить после окончательного изменения структуры БД
			newItem.MarketingEvent = marketingEvent;
			newItem.Producer = producer;
			if (contacts == null)
				newItem.Contacts = "+7 (000) 00 00";
			dbSession.Save(newItem);
			return newItem;
		}

		[Test, Ignore("На дванный момент для запуска руками")] //
		public void GenerateFixtureData()
		{
			for (var i = 0; i < 10; i++) {
				var newPromoter = AddPromoter(DbSession);
				var newAssociation = AddAssociation(DbSession, newPromoter);
				var newProducer = DbSession.Query<Producer>().FirstOrDefault(); //AddProducer(DbSession);
				if (i%2 == 0) {
					var mev = AddMarketingEvent(DbSession, newAssociation);
					var newPromoterProducer = AddPromoterProducer(DbSession, newPromoter, newProducer, mev);
				}
			}
		}
	}
}