using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marketing.Helpers;
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate;
using NHibernate.Linq;
using DevExpress.Web.Mvc;

namespace Marketing.Controllers
{
	public class PromoterProducersItem
	{
		public virtual uint Id { get; set; }
		public virtual Producer Producer { get; set; }
		public virtual string Contacts { get; set; }
	}

	public class MarketingController : BaseController
	{
		public ActionResult Index()
		{
			var PromoterProducers = DbSession.Query<PromoterProducer>()
				.Where(s => s.Promoter.Id == CurrentPromoter.Id).OrderBy(s => s.Producer.Name).ToList();
			return View(PromoterProducers);
		}

		public ActionResult IndexGridView()
		{
			var PromoterProducers = DbSession.Query<PromoterProducer>()
				.Where(s => s.Promoter.Id == CurrentPromoter.Id).OrderBy(s => s.Producer.Name).ToList();
			return PartialView("IndexGridView", PromoterProducers);
		}

		[HttpGet]
		public ActionResult ProducerAdd()
		{
			var model = new PromoterProducersViewModel();
			model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
				.Where(s => CurrentPromoter.Producers.All(f => f.Producer.Id != s.Id))
				.Select(s => new SelectListItem {Value = s.Id.ToString(), Text = s.Name}).ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult ProducerAdd(PromoterProducersViewModel model)
		{
			if (!this.ModelState.IsValid) {
				model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
				.Where(s => CurrentPromoter.Producers.All(f => f.Producer.Id != s.Id))
					.Select(s => new SelectListItem {Value = s.Id.ToString(), Text = s.Name})
					.ToList();
				return View(model);
			}
			var currentProducer = DbSession.Query<Producer>().First(s => s.Id == model.SelectedProducerId);
			if (CurrentPromoter.Producers.Any(s => s.Producer.Id == currentProducer.Id)) {
				ErrorMessage($"Поставщик \"{currentProducer.Name}\" не может быть добавлен повторно.");
			} else {
				var newItem = new PromoterProducer {
					Promoter = CurrentPromoter,
					Producer = currentProducer,
					Contacts = model.Contacts
				};
				DbSession.Save(newItem);

				SuccessMessage($"Поставщик \"{newItem.Producer.Name}\" успешно добавлен.");
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult GetFilterProducerAdd()
		{
			var result = DbSession.Query<Producer>().OrderBy(s => s.Name).ToList()
				.Where(s => CurrentPromoter.Producers.All(f => f.Producer.Id != s.Id))
				.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
				.ToList();
			return PartialView("../_default/ProducerAddFilter", result);

		}

		[HttpGet]
		public ActionResult ProducerEdit(uint id)
		{
			var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			return View(model);
		}

		[HttpPost]
		public ActionResult ProducerEdit(uint id, string contacts)
		{
			var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			model.Contacts = contacts;
			SuccessMessage($"Контакты поставщика \"{model.Producer.Name}\" успешно изменен.");
			return RedirectToAction("Index");
		}
		public ActionResult ProducerDelete(uint id)
		{
			var model = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			DbSession.Delete(model);
			SuccessMessage($"Поставщик \"{model.Producer.Name}\" успешно удален.");
			return RedirectToAction("Index");
		}
		
		public ActionResult PromotionList(uint id)
		{
			var currentProducer = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			var promoterProducers = DbSession.Query<ProducerPromotion>()
				.Where(s => s.Producer== currentProducer).OrderBy(s => s.Name).ToList();

			ViewBag.Producer = currentProducer;
			return View(promoterProducers);
		}

		public ActionResult PromotionListGridView(uint id)
		{
			var currentProducer = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			return PartialView("PromotionListGridView", DbSession.Query<ProducerPromotion>()
				.Where(s => s.Producer == currentProducer).OrderBy(s => s.Name).ToList());
		}

		[HttpGet]
		public ActionResult PromotionAdd(uint id)
		{
			var currentProducer = DbSession.Query<PromoterProducer>().First(s => s.Id == id);
			var model = new ProducerPromotion();
			model.Producer = currentProducer;
			ViewBag.Producer = currentProducer;
			return View(model);
		}

		[HttpPost]
		public ActionResult PromotionAdd(ProducerPromotion model)
		{
			var currentProducer = DbSession.Query<PromoterProducer>().First(s => s.Id == model.Producer.Id);
			if (!this.ModelState.IsValid)
			{
				ViewBag.Producer = currentProducer;
				return View(model);
			}
			var newItem = new ProducerPromotion() {
				Producer = currentProducer,
				Name = model.Name
			};
			DbSession.Save(newItem);

			SuccessMessage($"Акция \"{newItem.Name}\" успешно добавлена.");

			return RedirectToAction("PromotionList", new {id = currentProducer.Id});
		}


		public ActionResult PromotionEditListManager(uint id, string list,
			PromotionTableSelectorViewModel.RequestType type)
		{
			var model = new PromotionTableSelectorViewModel();
			model.SetData(DbSession, id, type, list);
			return PartialView("PromotionEditListGridView", model);
		}
		public ActionResult GetFilterRegion(string term, string currentValues = "")
		{
			var model = new List<SelectListItem>();
			DbSession.Query<Region>();
			return PartialView("RegionFilterLogic", model);
		}

		[HttpGet]
		public ActionResult PromotionEdit(uint id)
		{
			var model = new PromotionViewModel();
			model.SetData(DbSession, id);
			return View(model);
		}

		[HttpGet]
		public ActionResult PromotionChange(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == id);
			ViewBag.Producer = promotion.Producer;
			return View(promotion);
		}

		[HttpPost]
		public ActionResult PromotionChange(ProducerPromotion model)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == model.Id);
			if (!ModelState.IsValid) {
				ViewBag.Producer = promotion.Producer;
				return View(model);
			}
			promotion.Name = model.Name;
			promotion.DateStarted = model.DateStarted;
			promotion.DateFinished = model.DateFinished;
			DbSession.Save(promotion);
			return RedirectToAction("PromotionList", new {id = promotion.Producer.Id});
		}


		public ActionResult PromotionDelete(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == id);
			DbSession.Delete(promotion);
			SuccessMessage($"Акция \"{promotion.Name}\" успешно удалена.");
			return RedirectToAction("PromotionList", new { id = promotion.Producer.Id });
		}
		

		public ActionResult PromotionrEditGet(PromotionViewModel model)
		{
			var promotion = DbSession.Query<ProducerPromotion>().First(s => s.Id == model.Promotion.Id);

			promotion.UpdateProductsAndSuppliersByIds(DbSession, model.ProductsListToSetList, model.SuppliersListToSetList);

			return RedirectToAction("PromotionList",new {id = promotion.Producer.Id});
		}

		[HttpGet]
		public ActionResult PromotionСonditionsEdit(uint id)
		{
			var model = GetConditionsViewModel(id);
			return View(model);
		}

		public ActionResult GetConditionsGridData(uint id)
		{
			var model = GetConditionsViewModel(id);
			return PartialView("ConditionsGridView", model);
		}

		[ValidateInput(false)]
		public ActionResult EditConditionsBatch(uint id, MVCxGridViewBatchUpdateValues<ConditionsGridViewModel, int> updateValues)
		{
			foreach (var item in updateValues.Update) {
				if (updateValues.IsValid(item))
					try {
						var condition = DbSession.Query<PromotionProduct>().Single(r => r.Id == item.ConditionId);
						condition.Price = item.Price;
						condition.DealerPercent = item.DealerPercent;
						condition.MemberPercent = item.MemberPercent;
					} catch (Exception ex) {
						updateValues.SetErrorText(item, ex.Message);
					}
			}
			var model = GetConditionsViewModel(id);
			return PartialView("ConditionsGridView", model);
		}

		private PromotionConditionsViewModel GetConditionsViewModel(uint id)
		{
			var promotion = DbSession.Query<ProducerPromotion>().FirstOrDefault(r => r.Id == id);
			var producer = promotion.Producer;
			var query = DbSession.Query<PromotionProduct>()
				.Where(r => r.Promotion == promotion)
				.Fetch(r => r.Product)
				.ThenFetch(r => r.Catalog);
			var conditions = query
				.Select(r => new ConditionsGridViewModel
				{
					PromotionId = promotion.Id,
					ConditionId = r.Id,
					ProductName = r.Product.Catalog.Name,
					Price = r.Price,
					DealerPercent = r.DealerPercent,
					MemberPercent = r.MemberPercent
				})
				.ToList();
			var model = new PromotionConditionsViewModel
			{
				Conditions = conditions,
				Promotion = promotion,
				Producer = producer.Producer
			};
			return model;
		}

		[HttpPost]
		public ActionResult PromotionСonditionsEdit(uint id, string contacts)
		{
			return RedirectToAction("Index");
		}
	}
}