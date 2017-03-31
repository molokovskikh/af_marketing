using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Dapper;
using DevExpress.Web;
using DevExpress.Web.Mvc;
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate.Linq;

namespace Marketing.Controllers
{
	public class DevExampleController : Controller
	{
		public List<dynamic> DevExampleData { get; set; }

		public DevExampleController()
		{
			DevExampleData = new List<dynamic>() {
				new {
					CatalogId = 1,
					CatalogName = "CatalogName 1",
					ProducerId = 1,
					ProducerName = "ProducerName 1",
					SupplierId = 1,
					SupplierName = "SupplierName 1",
					Cost = 100,
					Quantity = 10,
					Date = DateTime.Now.Date.AddDays(-1)
				},
				new {
					CatalogId = 2,
					CatalogName = "CatalogName 2",
					ProducerId = 2,
					ProducerName = "ProducerName 2",
					SupplierId = 2,
					SupplierName = "SupplierName 2",
					Cost = 200,
					Quantity = 20,
					Date = DateTime.Now.Date.AddDays(-2)
				},
				new {
					CatalogId = 3,
					CatalogName = "CatalogName 3",
					ProducerId = 3,
					ProducerName = "ProducerName 3",
					SupplierId = 3,
					SupplierName = "SupplierName 3",
					Cost = 300,
					Quantity = 30,
					Date = DateTime.Now.Date.AddDays(-3)
				},
				new {
					CatalogId = 4,
					CatalogName = "CatalogName 4",
					ProducerId = 4,
					ProducerName = "ProducerName 4",
					SupplierId = 4,
					SupplierName = "SupplierName 4",
					Cost = 400,
					Quantity = 40,
					Date = DateTime.Now.Date.AddDays(-4)
				},
				new {
					CatalogId = 5,
					CatalogName = "CatalogName 5",
					ProducerId = 5,
					ProducerName = "ProducerName 5",
					SupplierId = 5,
					SupplierName = "SupplierName 5",
					Cost = 500,
					Quantity = 50,
					Date = DateTime.Now.Date.AddDays(-5)
				},
				new {
					CatalogId = 6,
					CatalogName = "CatalogName 6",
					ProducerId = 6,
					ProducerName = "ProducerName 6",
					SupplierId = 6,
					SupplierName = "SupplierName 6",
					Cost = 600,
					Quantity = 60,
					Date = DateTime.Now.Date.AddDays(-6)
				},
				new {
					CatalogId = 7,
					CatalogName = "CatalogName 7",
					ProducerId = 7,
					ProducerName = "ProducerName 7",
					SupplierId = 7,
					SupplierName = "SupplierName 7",
					Cost = 700,
					Quantity = 70,
					Date = DateTime.Now.Date.AddDays(-7)
				},
				new {
					CatalogId = 8,
					CatalogName = "CatalogName 8",
					ProducerId = 8,
					ProducerName = "ProducerName 8",
					SupplierId = 8,
					SupplierName = "SupplierName 8",
					Cost = 800,
					Quantity = 80,
					Date = DateTime.Now.Date.AddDays(-8)
				},
				new {
					CatalogId = 9,
					CatalogName = "CatalogName 9",
					ProducerId = 9,
					ProducerName = "ProducerName 9",
					SupplierId = 9,
					SupplierName = "SupplierName 9",
					Cost = 900,
					Quantity = 90,
					Date = DateTime.Now.Date.AddDays(-9)
				},
				new {
					CatalogId = 10,
					CatalogName = "CatalogName 10",
					ProducerId = 10,
					ProducerName = "ProducerName 10",
					SupplierId = 10,
					SupplierName = "SupplierName 10",
					Cost = 1000,
					Quantity = 100,
					Date = DateTime.Now.Date.AddDays(-10)
				},
				new {
					CatalogId = 11,
					CatalogName = "CatalogName 11",
					ProducerId = 11,
					ProducerName = "ProducerName 11",
					SupplierId = 11,
					SupplierName = "SupplierName 11",
					Cost = 1100,
					Quantity = 110,
					Date = DateTime.Now.Date.AddDays(-11)
				},
				new {
					CatalogId = 12,
					CatalogName = "CatalogName 12",
					ProducerId = 12,
					ProducerName = "ProducerName 12",
					SupplierId = 12,
					SupplierName = "SupplierName 12",
					Cost = 1200,
					Quantity = 120,
					Date = DateTime.Now.Date.AddDays(-12)
				},
				new {
					CatalogId = 13,
					CatalogName = "CatalogName 13",
					ProducerId = 13,
					ProducerName = "ProducerName 13",
					SupplierId = 13,
					SupplierName = "SupplierName 13",
					Cost = 1300,
					Quantity = 130,
					Date = DateTime.Now.Date.AddDays(-13)
				}
			};
		}

		public ActionResult FilterRowPartial(DateTime dateBegin, DateTime dateEnd, string supplierIdList,
			string producerIdList, string catalogIdList)
		{
			return PartialView("GridView",
				GetData(dateBegin, dateEnd, supplierIdList, producerIdList, catalogIdList));
		}

		public ActionResult Index()
		{
			ViewBag.DateFrom = DateTime.Now.Date.AddDays(-6);
			ViewBag.DateTo = DateTime.Now;
			return View(GetData());
		}

		[HttpPost]
		public ActionResult Index(DateTime dateBegin, DateTime dateEnd, string supplierIdList, string producerIdList,
			string catalogIdList, bool? exportToExcel = null)
		{
			IEnumerable result = GetData(dateBegin, dateEnd, supplierIdList, producerIdList, catalogIdList);
			if (exportToExcel.HasValue) {
				return GridViewExtension.ExportToXlsx(GridViewHelper.ExportGridViewSettings, result);
			}
			ViewBag.DateFrom = dateBegin;
			ViewBag.DateTo = dateEnd;
			ViewBag.CurrentProducers = SearchProducers("", producerIdList);
			ViewBag.CurrentCatalogs = SearchCatalogs("", catalogIdList);
			ViewBag.CurrentSuppliers = SearchSuppliers(supplierIdList);

			return View(result);
		}

		private IEnumerable GetData(DateTime? dateBegin = null, DateTime? dateEnd = null, string supplierList = "",
			string producerList = "", string catalogList = "")
		{
			uint currentClient = 777;
			var beginDate = dateBegin ?? DateTime.Now.Date.AddDays(-6);
			var endDate = dateEnd ?? DateTime.Now;

			//если SQL запрос то лучше использовать форму " supplierId IN (supplierList) "
			//для примера
			var reportDataQuery = DevExampleData.Where(s => s.Date >= beginDate && s.Date < endDate.Date.AddDays(1));
			if (!string.IsNullOrEmpty(supplierList)) {
				var supplierIdList = supplierList.Split(',').Select(s => int.Parse(s)).ToList();
				reportDataQuery = reportDataQuery.Where(s => supplierIdList.Any(f => f == s.SupplierId));
			}
			if (!string.IsNullOrEmpty(producerList)) {
				var producerIdList = producerList.Split(',').Select(s => int.Parse(s)).ToList();
				reportDataQuery = reportDataQuery.Where(s => producerIdList.Any(f => f == s.ProducerId));
			}
			if (!string.IsNullOrEmpty(catalogList)) {
				var catalogIdList = catalogList.Split(',').Select(s => int.Parse(s)).ToList();
				reportDataQuery = reportDataQuery.Where(s => catalogIdList.Any(f => f == s.CatalogId));
			}
			return reportDataQuery.ToList();
		}


		[HttpPost]
		public ActionResult GetFilterCatalogs(string term, string currentValues = "")
		{
			var model = SearchCatalogs(term, currentValues);
			return PartialView("../_default/CatalogsFilterLogic", model);
		}

		private List<ViewModelList> SearchCatalogs(string term, string currentValues = "")
		{
			term = string.IsNullOrEmpty((term ?? "").Trim()) ? "%" : term;
			currentValues = string.IsNullOrEmpty(currentValues) ? "0" : currentValues;
			//даппер привязывает параметры к типу, чтобы не конвертировать currentValues добавляется в запрос
			//var sql = @"";
			//var result = DbSession.Connection.Query<ViewModelList>(sql, new {@stext = $"%{(term ?? "").Trim()}%"}).ToList();
			//но для примера:
			var itemsIdList = currentValues.Split(',').Select(s => uint.Parse(s)).ToList();
			var result =
				DevExampleData.Select(
					s =>
						new ViewModelList {
							Value = (uint) s.CatalogId,
							Text = s.CatalogName,
							Selected = itemsIdList.Any(f => f == (uint) s.CatalogId)
						}).ToList();
			return result;
		}


		[HttpPost]
		public ActionResult GetFilterSuppliers(string currentValues = "")
		{
			var model = SearchSuppliers(currentValues);
			return PartialView("../_default/SuppliersFilterLogic", model);
		}

		private List<ViewModelList> SearchSuppliers(string currentValues = "")
		{
			uint currentClient = 777;
			currentValues = string.IsNullOrEmpty(currentValues) ? "0" : currentValues;
			//даппер привязывает параметры к типу, чтобы не конвертировать currentValues добавляется в запрос
			//var result = DbSession.Connection.Query<ViewModelList>(@"", new {@clientId = currentClient}).ToList().OrderBy(s => s.Text).ToList();
			//но для примера:
			var itemsIdList = currentValues.Split(',').Select(s => uint.Parse(s)).ToList();
			var result =
				DevExampleData.Select(
					s =>
						new ViewModelList {
							Value = (uint) s.SupplierId,
							Text = s.SupplierName,
							Selected = itemsIdList.Any(f => f == (uint) s.SupplierId)
						}).ToList();
			return result;
		}


		[HttpPost]
		public ActionResult GetFilterProducers(string term, string currentValues = "")
		{
			var model = SearchProducers(term, currentValues);
			return PartialView("../_default/ProducersFilterLogic", model);
		}

		private List<ViewModelList> SearchProducers(string term, string currentValues = "")
		{
			term = string.IsNullOrEmpty((term ?? "").Trim()) ? "%" : term;
			currentValues = string.IsNullOrEmpty(currentValues) ? "0" : currentValues;
			//даппер привязывает параметры к типу, чтобы не конвертировать currentValues добавляется в запрос
			//var sql = @"";
			//var result = DbSession.Connection.Query<ViewModelList>(sql, new {@stext = $"%{(term ?? "").Trim()}%"}).ToList();
			//но для примера:
			var itemsIdList = currentValues.Split(',').Select(s => uint.Parse(s)).ToList();
			var result =
				DevExampleData.Select(
					s =>
						new ViewModelList {
							Value = (uint) s.ProducerId,
							Text = s.ProducerName,
							Selected = itemsIdList.Any(f => f == (uint) s.ProducerId)
						}).ToList();
			return result;
		}

		public static class GridViewHelper
		{
			private static GridViewSettings exportGridViewSettings;

			public static GridViewSettings ExportGridViewSettings
			{
				get
				{
					if (exportGridViewSettings == null)
						exportGridViewSettings = CreateExportGridViewSettings();
					return exportGridViewSettings;
				}
			}

			private static GridViewSettings CreateExportGridViewSettings()
			{
				GridViewSettings settings = new GridViewSettings();
				settings.Name = "Пример выгрузки";
				settings.CallbackRouteValues = new {Controller = "DevExample", Action = "FilterRowPartial"};
				settings.Columns.Add(column => {
					column.FieldName = "Date";
					column.Caption = "Дата";
					column.Width = Unit.Pixel(200);
					column.PropertiesEdit.DisplayFormatString = "d";
				});

				settings.Columns.Add("SupplierName").Caption = "Поставщик";
				settings.Columns.Add(column => {
					column.FieldName = "CatalogName";
					column.Caption = "Товар";
				});

				settings.Columns.Add("ProducerName").Caption = "Производитель";

				settings.Columns.Add(column => {
					column.FieldName = "Cost";
					column.Caption = "Цена";
					column.PropertiesEdit.DisplayFormatString = "c";
				});

				settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "Cost").DisplayFormat = "c";

				var sp = new ASPxSummaryItem("Cost", DevExpress.Data.SummaryItemType.Sum);
				sp.ShowInGroupFooterColumn = "Цена";
				sp.DisplayFormat = "c";
				settings.GroupSummary.Add(sp);

				settings.Columns.Add(column => {
					column.FieldName = "Quantity";
					column.Caption = "Количество";
				});

				settings.TotalSummary.Add(DevExpress.Data.SummaryItemType.Sum, "Quantity").ShowInGroupFooterColumn = "Ед.";


				var sm = new ASPxSummaryItem("Quantity", DevExpress.Data.SummaryItemType.Sum);
				sm.ShowInGroupFooterColumn = "Приход";
				settings.GroupSummary.Add(sm);


				settings.Settings.ShowGroupFooter = GridViewGroupFooterMode.VisibleAlways;
				settings.Settings.ShowGroupPanel = true;
				settings.Settings.ShowFooter = true;
				settings.Settings.VerticalScrollableHeight = 500;
				settings.SettingsPager.PageSize = 100;


				settings.SettingsSearchPanel.Visible = true;
				settings.SettingsPager.Position = PagerPosition.TopAndBottom;
				settings.SettingsPager.FirstPageButton.Visible = true;
				settings.SettingsPager.LastPageButton.Visible = true;
				settings.SettingsPager.PageSizeItemSettings.Visible = true;
				settings.SettingsPager.PageSizeItemSettings.Items = new string[] {"10", "20", "50", "100", "150", "200"};

				return settings;
			}
		}
	}
}