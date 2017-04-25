using Marketing.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NHibernate.Linq;
using Marketing.Models;
using NHibernate;

namespace Marketing.Controllers
{
	[Authorize]
	public class AssociationController : BaseController
	{
		public ActionResult Index()
		{
			var model = GetAssociationList();
			return View(model);
		}

		[HttpPost]
		public ActionResult Associations()
		{
			var model = GetAssociationList();
			return View("_IndexGridView", model);
		}

		private IList<AssociationGridModel> GetAssociationList()
		{
			return DbSession.Query<Association>()
				.OrderBy(r => r.Name)
				.ToList()
				.Select(r => new AssociationGridModel {
					AssociationId = r.Id,
					Name = r.Name,
					Comments = r.Comments,
					Contacts =
						string.Join("; ",
							r.Contacts.OrderBy(x => x.ContactType).ThenBy(x => x.Fio).Select(x => $"{x.Fio}, {x.Phone}, {x.Email}").ToArray()),
					Regions = string.Join(", ", r.Regions.OrderBy(x => x.Region.Name).Select(x => x.Region.Name).ToArray())
				})
				.ToList();
		}

		public ActionResult Add()
		{
			var model = new AssociationEditViewModel();
			return View(model);
		}

		[HttpPost]
		public ActionResult Add(AssociationEditViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var association = new Association {
				Name = model.Name,
				Comments = model.Comments
			};
			DbSession.Save(association);

			return RedirectToAction("Index");
		}

		public ActionResult Edit(uint id)
		{
			var association = DbSession.Query<Association>().First(r => r.Id == id);
			var model = new AssociationEditViewModel {
				AssociationId = association.Id,
				Name = association.Name,
				Comments = association.Comments
			};
			return View(model);
		}

		[HttpPost]
		public ActionResult Edit(AssociationEditViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var association = DbSession.Query<Association>().First(r => r.Id == model.AssociationId);
			association.Name = model.Name;
			association.Comments = model.Comments;

			return RedirectToAction("Index");
		}

		public ActionResult Delete(uint id)
		{
			var association = DbSession.Query<Association>().First(r => r.Id == id);
			DbSession.Delete(association);

			return RedirectToAction("Index");
		}

		public ActionResult Regions(uint id)
		{
			var association = DbSession.Query<Association>().First(r => r.Id == id);
			var model = new AssociationEditViewModel {
				AssociationId = association.Id,
				Name = association.Name
			};
			var regionIds = association.Regions.Select(r => r.Region.Id).ToArray();
			model.SelectedRegionIds = string.Join(",", regionIds.Select(r => r.ToString()).ToArray());
			model.AvailableRegions = DbSession.Query<Region>()
				.Where(r => r.Id != 0 && r.Id != Region.INFOROOM_CODE)
				.OrderBy(r => r.Name)
				.ToList()
				.Select(r => new Region {
					Id = r.Id,
					Name = (regionIds.Contains(r.Id) ? "*" : "") + r.Name
				})
				.ToList();
			return View(model);
		}

		[HttpPost]
		public ActionResult Regions(AssociationEditViewModel model)
		{
			var association = DbSession.Query<Association>().First(r => r.Id == model.AssociationId);
			var ids = model.SelectedRegionIds.Split(',').Select(r => ulong.Parse(r)).ToArray();
			var regions = association.Regions.ToList();
			DbSession.Evict(association);
			DbSession.Evict(regions);

			var del = regions.Where(r => !ids.Contains(r.Region.Id)).ToList();
			del.ForEach(r => DbSession.Delete(r));

			ids.Where(r => !regions.Any(x => x.Region.Id == r)).ToList().ForEach(r => {
				var region = new AssociationRegion {
					Association = association,
					Region = DbSession.Query<Region>().First(x => x.Id == r)
				};
				DbSession.Save(region);
			});
			DbSession.Flush();

			return RedirectToAction("Index");
		}

		public ActionResult Contacts(uint id)
		{
			return View();
		}
	}
}