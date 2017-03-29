using NUnit.Framework;

namespace test
{
	[TestFixture]
	public class SampleFixture
	{
		[Test]
		public void Fail()
		{
			Assert.Fail("Напиши меня");
		}
	}
}