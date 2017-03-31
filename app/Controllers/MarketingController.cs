using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Marketing.Models;
using Marketing.ViewModels;
using NHibernate;
using NHibernate.Linq;

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
			model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name)
				.Select(s => new SelectListItem() {Value = s.Id.ToString(), Text = s.Name}).ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult ProducerAdd(PromoterProducersViewModel model)
		{
			if (!this.ModelState.IsValid) {
				model.ProducersList = DbSession.Query<Producer>().OrderBy(s => s.Name)
					.Select(
						s => new SelectListItem() {Value = s.Id.ToString(), Text = s.Name, Selected = s.Id == model.SelectedProducerId})
					.ToList();
				return View(model);
			}
			var currentProducer = DbSession.Query<Producer>().First(s => s.Id == model.SelectedProducerId);
			var newItem = new PromoterProducer() {
				Promoter = CurrentPromoter,
				Producer = currentProducer,
				Contacts = model.Contacts
			};
			DbSession.Save(newItem);

			SuccessMessage($"Поставщик \"{newItem.Producer.Name}\" успешно добавлен.");
			return RedirectToAction("Index");
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

		[HttpGet]
		public ActionResult PromotionEdit(uint id)
		{
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult PromotionrEdit(uint id)
		{
			return RedirectToAction("Index");
		}

		[HttpGet]
		public ActionResult PromotionСonditionsEdit(uint id)
		{
			return RedirectToAction("Index");
		}

		[HttpPost]
		public ActionResult PromotionСonditionsEdit(uint id, string contacts)
		{
			return RedirectToAction("Index");
		}
	}
}