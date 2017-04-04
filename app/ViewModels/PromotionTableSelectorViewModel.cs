using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dapper;
using NHibernate;

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
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
customers.promotion_producersproducts as pp
INNER JOIN catalogs.Products as pr ON pr.Id = pp.ProductId
INNER JOIN catalogs.catalog AS ct ON ct.Id = pr.CatalogId
WHERE pp.ProductId NOT IN ({0})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList)).ToList();
				return;
			}

			if (type == RequestType.ProductsListToSet) {
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
customers.promotion_producersproducts as pp
INNER JOIN catalogs.Products as pr ON pr.Id = pp.ProductId
INNER JOIN catalogs.catalog AS ct ON ct.Id = pr.CatalogId
WHERE pp.ProductId IN ({0})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList)).ToList();
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