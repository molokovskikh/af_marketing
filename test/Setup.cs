using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using NUnit.Framework;

namespace test
{
	[SetUpFixture]
	class Setup
	{

		[OneTimeSetUp]
		public void FixtureStarted()
		{
			var hibernate = new Marketing.NHibernate();
			hibernate.Init();
			BaseFixture.SettingsFactory(hibernate.Factory);
		}

	}
}
