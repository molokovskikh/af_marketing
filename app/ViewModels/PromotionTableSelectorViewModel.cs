using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;

namespace Marketing.ViewModels
{
	public class PromotionTableSelectorViewModel
	{
		public enum RequestType
		{
			ProductsListToGet,
			ProductsListToSet,
			SuppliersListToGet,
			SuppliersListToSet
		}

		public PromotionTableSelectorViewModel()
		{
			ItemsList = new List<SelectListItem>();
		}

		public string Name { get; set; }
		public string Caption { get; set; }

		public List<SelectListItem> ItemsList { get; set; }

		public void SetData(ISession dbSession, uint promotionId, RequestType type, string selectedList)
		{
			Name = type.ToString();
			Caption = type == RequestType.ProductsListToGet
				|| type == RequestType.SuppliersListToGet
				? "Доступные"
				: "Участвующие в акции";


			if (type == RequestType.ProductsListToGet) {
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
catalogs.assortment as pp
INNER JOIN catalogs.catalog AS ct ON ct.Id = pp.CatalogId
INNER JOIN catalogs.Products as pr ON pr.CatalogId = ct.Id
WHERE pr.Id NOT IN ({0})
AND pp.ProducerId IN ({1})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList, promotion.Producer.Producer.Id)).ToList();
				return;
			}

			if (type == RequestType.ProductsListToSet) {
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
catalogs.assortment as pp
INNER JOIN catalogs.catalog AS ct ON ct.Id = pp.CatalogId
INNER JOIN catalogs.Products as pr ON pr.CatalogId = ct.Id
WHERE pr.Id IN ({0})
AND pp.ProducerId IN ({1})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList, promotion.Producer.Producer.Id)).ToList();
				return;
			}
			if (type == RequestType.SuppliersListToGet) {
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT sp.Id AS 'Value',sp.Name AS 'Text' FROM customers.suppliers AS sp
WHERE sp.Id NOT IN ({0})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList)).ToList();
				return;
			}
			if (type == RequestType.SuppliersListToSet) {
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT sp.Id AS 'Value',sp.Name AS 'Text' FROM customers.suppliers AS sp
WHERE sp.Id IN ({0})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList)).ToList();
			}
		}
	}
}