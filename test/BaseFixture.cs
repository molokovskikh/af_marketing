using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Marketing;
using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;

namespace test
{
	public class BaseFixture
	{
		private static ISessionFactory SessionFactory { get; set; }
		protected ISession DbSession;

		public static void SettingsFactory(ISessionFactory factory)
		{
			SessionFactory = factory;
		}


		public BaseFixture()
		{
			DbSession = SessionFactory.OpenSession();
		}

		[SetUp]
		public void FixtureStarted()
		{
			DbSession.BeginTransaction();
		}

		[TearDown]
		public void FixtureFinished()
		{
			if (DbSession == null)
				return;

			try {
				DbSession.Flush();
				DbSession.Transaction.Commit();
			} catch {
				DbSession.Transaction.Rollback();
			} finally {
				DbSession.Close();
			}
		}
	}
}