using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;
using NUnit.Framework;

namespace test.Data
{
	[TestFixture]
	public class BaseDataAdding : BaseFixture
	{
		[Test, Ignore("На дванный момент для запуска руками")] //
		public void GenerateFixtureData()
		{
			for (int i = 0; i < 10; i++) {
				var newPromoter = AddPromoter(DbSession);
				var newProducer = DbSession.Query<Producer>().FirstOrDefault(); //AddProducer(DbSession);
				if (i%2 == 0) {
					var newPromoterProducer = AddPromoterProducer(DbSession, newPromoter, newProducer);
				}
			}
		}

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

		/// <summary>
		/// Добавление поставщика для пользователя
		/// </summary>
		/// <param name="dbSession"></param>
		/// <param name="promoter"></param>
		/// <param name="producer"></param>
		/// <param name="contacts">По умолчанию "+7 (000) 00 00"</param>
		/// <returns></returns>
		public static PromoterProducer AddPromoterProducer(ISession dbSession, Promoter promoter, Producer producer,
			string contacts = null)
		{
			var newItem = new PromoterProducer();
			// todo исправить после окончательного изменения структуры БД
			//newItem.Promoter = promoter;
			newItem.Producer = producer;
			if (contacts == null)
				newItem.Contacts = "+7 (000) 00 00";
			dbSession.Save(newItem);
			return newItem;
		}
	}
}