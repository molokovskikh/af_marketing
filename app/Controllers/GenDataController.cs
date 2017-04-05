using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Dapper;
using Marketing.Models;
using NHibernate.Linq;

namespace Marketing.Controllers
{
	/// <summary>
	/// TODO: удалить после проработки корректного алгоритма
	/// </summary>
	public class GenDataController : BaseController
	{
		// GET: GenData
		public ActionResult Index(uint id)
		{
				PromoterProducer.GeneRateCashForProducts(DbSession, id);
			return null;
		}

		// GET: GenData
		public ActionResult ForAllProducers()
		{
			var producers = DbSession.Query<Producer>().Select(s => s.Id).ToList();
			foreach (var producer in producers) {
				var products = DbSession.Connection.Query<List<uint>>(
					"Select ProductId from customers.promotion_producersproducts WHERE ProducerId = @producerId",
					new {@producerId = producer});
				if (products.Count() == 0) {
					PromoterProducer.GeneRateCashForProducts(DbSession, producer);
				}
			}
			return null;
		}
	}
}