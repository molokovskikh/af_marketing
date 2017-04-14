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
			Height = 300;
		}

		public int Height { get; set; }

		public string Name { get; set; }
		public string Caption { get; set; }

		public List<SelectListItem> ItemsList { get; set; }

		public void SetData(ISession dbSession, uint promotionId, RequestType type, string selectedList, string regionList)
		{
			Name = type.ToString();
			Caption = type == RequestType.ProductsListToGet
				|| type == RequestType.SuppliersListToGet
				? "Доступные"
				: "Участвующие в акции";


			if (type == RequestType.ProductsListToGet) {
				Height = 600;
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
catalogs.assortment as pp
INNER JOIN catalogs.catalog AS ct ON ct.Id = pp.CatalogId
INNER JOIN catalogs.Products as pr ON pr.CatalogId = ct.Id
WHERE pr.Id NOT IN ({0})
AND pp.ProducerId IN ({1})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList, promotion./*Producers.Producer.*/Id)).ToList();
				return;
			}

			if (type == RequestType.ProductsListToSet) {
				Height = 600;
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
catalogs.assortment as pp
INNER JOIN catalogs.catalog AS ct ON ct.Id = pp.CatalogId
INNER JOIN catalogs.Products as pr ON pr.CatalogId = ct.Id
WHERE pr.Id IN ({0})
AND pp.ProducerId IN ({1})
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList, promotion./*Producers.Producer.*/Id)).ToList();
				return;
			}
			if (type == RequestType.SuppliersListToGet) {
				ulong mask = 0;
				if (!string.IsNullOrEmpty(regionList)) {
					var arraySplited = regionList.Split(',');
					if (arraySplited.All(s => s != "0")) {
						var regions = arraySplited.Select(s => {
							ulong value = 0;
							ulong.TryParse(s, out value);
							return value;
						}).Where(s => s != 0).ToList();
						mask = regions.Aggregate(mask, (current, region) => current | region);
					}
				}
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT sp.Id AS 'Value', CONCAT(sp.Name,' - ',rg.Region) AS 'Text' FROM customers.suppliers AS sp
INNER JOIN farm.Regions as rg ON rg.RegionCode & sp.RegionMask  > 0
WHERE sp.Id NOT IN ({0}) AND sp.Disabled = 0 {1}
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList, mask == 0 ? "" : "AND sp.RegionMask & @code > 0"),
					new {@code = mask}).ToList();
				return;
			}
			if (type == RequestType.SuppliersListToSet) {
				ItemsList = dbSession.Connection.Query<SelectListItem>(string.Format(@"
SELECT sp.Id AS 'Value', CONCAT(sp.Name,' - ',rg.Region) AS 'Text' FROM customers.suppliers AS sp
INNER JOIN farm.Regions as rg ON rg.RegionCode & sp.RegionMask  > 0
WHERE sp.Id IN ({0}) AND sp.Disabled = 0
ORDER BY Text
", string.IsNullOrEmpty(selectedList) ? "0" : selectedList)).ToList();
			}
		}
	}
}