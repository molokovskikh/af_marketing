using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Dapper;
using DevExpress.XtraPrinting.Native;
using Marketing.Models;
using NHibernate;
using NHibernate.Linq;

namespace Marketing.ViewModels
{
	public enum PromotionTableRequestType
	{
		ProductsListToGet,
		ProductsListToSet,
		SuppliersListToGet,
		SuppliersListToSet
	}

	public class PromotionTableSelectorViewModel<T> where T : IViewModelListItem
	{
		private const string CacheOfProductsOfProducts = "CacheOfProductsOfProducts";
		private const string CacheOfProductsOfSuppliers = "CacheOfProductsOfSuppliers";

		public static List<T> DbSuppliers
		{
			get
			{
				if (HttpContext.Current.Items[CacheOfProductsOfSuppliers] == null)
					HttpContext.Current.Items[CacheOfProductsOfSuppliers] = new List<T>();
				return (List<T>) HttpContext.Current.Items[CacheOfProductsOfSuppliers];
			}
			set { HttpContext.Current.Items[CacheOfProductsOfSuppliers] = value; }
		}

		public static List<T> DbProducts
		{
			get
			{
				if (HttpContext.Current.Items[CacheOfProductsOfProducts] == null)
					HttpContext.Current.Items[CacheOfProductsOfProducts] = new List<T>();
				return (List<T>) HttpContext.Current.Items[CacheOfProductsOfProducts];
			}
			set { HttpContext.Current.Items[CacheOfProductsOfProducts] = value; }
		}

		public PromotionTableSelectorViewModel()
		{
			ItemsList = new List<T>();
			Height = 300;
		}

		public int Height { get; set; }

		public string Name { get; set; }
		public string Caption { get; set; }

		public List<T> ItemsList { get; set; }

		public void SetData(ISession dbSession, uint promotionId, PromotionTableRequestType type, string selectedList,
			string regionList)
		{
			Name = type.ToString();
			Caption = type == PromotionTableRequestType.ProductsListToGet
				|| type == PromotionTableRequestType.SuppliersListToGet
				? "Доступные"
				: "Участвующие в акции";

			if (type == PromotionTableRequestType.ProductsListToGet) {
				Height = 600;
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				var producerIds = string.Join(",", promotion.MarketingEvent.Producers.Select(r => r.Producer.Id.ToString()).ToArray());
				if (DbProducts.Count == 0) {
					UpdateDbProducts(dbSession, producerIds);
				}
				var itemList = GetUlongListForString(selectedList);
				ItemsList = DbProducts.Where(s => itemList.All(n => n != s.Value)).ToList();
				return;
			}

			if (type == PromotionTableRequestType.ProductsListToSet) {
				Height = 600;
				var itemList = GetUlongListForString(selectedList);
				var promotion = dbSession.Query<ProducerPromotion>().First(s => s.Id == promotionId);
				var producerIds = string.Join(",", promotion.MarketingEvent.Producers.Select(r => r.Producer.Id.ToString()).ToArray());
				if (DbProducts.Count == 0) {
					UpdateDbProducts(dbSession, producerIds);
				}
				ItemsList = DbProducts.Where(s => itemList.Any(n => n == s.Value)).ToList();
				return;
			}
			if (type == PromotionTableRequestType.SuppliersListToGet) {
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
				if (DbSuppliers.Count == 0)
					UpdateDbSuppliers(dbSession);
				var itemList = GetUlongListForString(selectedList);

				if (mask == 0) {
					ItemsList = DbSuppliers.Where(s => itemList.All(n => n != s.Value)).ToList();
				} else {
					ItemsList = DbSuppliers.Where(s =>
						(((s as ViewModelRegionListItem)?.RegionId & mask) > 0) && itemList.All(n => n != s.Value)).ToList();
				}
				return;
			}
			if (type == PromotionTableRequestType.SuppliersListToSet) {
				if (DbSuppliers.Count == 0)
					UpdateDbSuppliers(dbSession);
				var itemList = GetUlongListForString(selectedList);
				ItemsList = DbSuppliers.Where(s => itemList.Any(n => n == s.Value)).ToList();
			}
		}

		private void UpdateDbProducts(ISession dbSession, string producerIds)
		{
			if (string.IsNullOrEmpty(producerIds))
				producerIds = "0";
			DbProducts = dbSession.Connection.Query<T>(string.Format(@"
SELECT pr.Id as 'Value',CONCAT(ct.Name,' ',IFNULL(pr.Properties,'')) as 'Text' FROM
catalogs.assortment as pp
INNER JOIN catalogs.catalog AS ct ON ct.Id = pp.CatalogId
INNER JOIN catalogs.Products as pr ON pr.CatalogId = ct.Id
WHERE pp.ProducerId IN ({0})
ORDER BY Text
", producerIds)).ToList();
		}

		private void UpdateDbSuppliers(ISession dbSession)
		{
			DbSuppliers = dbSession.Connection.Query<T>(@"
SELECT sp.Id AS 'Value', sp.Name AS 'Text', rg.Region AS 'Region', sp.RegionMask AS 'RegionId'  FROM customers.suppliers AS sp
INNER JOIN farm.Regions as rg ON rg.RegionCode & sp.HomeRegion  > 0
WHERE sp.Disabled = 0 AND sp.Name IS NOT NULL AND sp.HomeRegion <> 524288
 AND rg.DrugsSearchRegion = 0
ORDER BY Text
").ToList(); //524288 - регион inforoom (список регионов отдельно*)
		}

		private List<ulong> GetUlongListForString(string selectedList)
		{
			return selectedList?.Split(',').Select(s => {
				ulong val = 0;
				ulong.TryParse(s, out val);
				return val;
			}).Where(s => s != 0).ToList() ?? new List<ulong>();
		}
	}
}