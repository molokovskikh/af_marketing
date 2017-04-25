using Marketing.Helpers;
using Marketing.Models;
using Marketing.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using NHibernate.Linq;
using NHibernate;

namespace Marketing.Controllers
{
	[Authorize]
	public class AccountController : BaseController
	{
		[AllowAnonymous]
		public ActionResult Login()
		{
			return View(new LoginViewModel());
		}

		[AllowAnonymous]
		[HttpPost]
		public ActionResult Login(LoginViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var login = Promoter.ACC_LOGIN_PREFIX + model.Login;
			var skipValidation = false;
#if DEBUG
			skipValidation = !String.IsNullOrEmpty(ConfigurationManager.AppSettings["SkipLogonAD"]);
#endif
			if (!skipValidation) {
				if (!Membership.ValidateUser(login, model.Password)) {
					login = model.Login;
					if (!Membership.ValidateUser(login, model.Password)) {
						ModelState.AddModelError("", "Неверный логин/пароль.");
						return View(model);
					}
				}
			}

			var user = DbSession.Query<Promoter>().Where(r => r.Login == model.Login).FirstOrDefault();
			if (user == null) {
				var admin = DbSession.Query<RegionalAdmin>().Any(r => r.Name == model.Login);
				if (admin) {
					FormsAuthentication.SetAuthCookie(model.Login, model.RememberMe);
					return RedirectToAction("Register");
				} else {
					ModelState.AddModelError("",
						$"Пользователь \"{model.Login}\" не включен в список организаторов акций. Обратитесь в АналитФармацию.");
					return View(model);
				}
			}

			FormsAuthentication.SetAuthCookie(login, model.RememberMe);
			System.Web.HttpContext.Current.Session["promoter"] = user;

			Association lastAssociation = null;
			if (user.LastAssociationId.HasValue) {
				lastAssociation = DbSession.Query<Association>().FirstOrDefault(r => r.Id == user.LastAssociationId);
			}
			if (lastAssociation == null) {
				lastAssociation = DbSession.Query<PromoterAssociation>().First(r => r.Promoter == user).Association;
				user.LastAssociationId = lastAssociation.Id;
			}
			NHibernateUtil.Initialize(lastAssociation);
			DbSession.Evict(lastAssociation);
			CurrentAssociation = lastAssociation;

			return Redirect("~");
		}

		[HttpPost]
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			DbSession.Clear();
			CurrentAssociation = null;
			return RedirectToAction("Login");
		}

		public ActionResult Register()
		{
			var model = new RegisterViewModel();
			model.AvailableAssociations = GetAllAssociationList();
			return View(model);
		}

		[HttpPost]
		public ActionResult AllAssociationList()
		{
			var model = new RegisterViewModel();
			model.AvailableAssociations = GetAllAssociationList();
			return View("_AllAssociationList", model);
		}

		private IList<AssociationItemViewModel> GetAllAssociationList()
		{
			return DbSession.Query<Association>()
				.OrderBy(r => r.Name)
				.ToList()
				.Select(r => new AssociationItemViewModel
				{
					AssociationId = r.Id,
					Name = r.Name,
					Regions = string.Join(", ", r.Regions.Select(x => x.Region.Name).OrderBy(x => x).ToArray())
				})
				.ToList();
		}

		[HttpPost]
		public ActionResult Register(RegisterViewModel model)
		{
			if (!ModelState.IsValid)
				return View(model);

			try {
				var promoter = new Promoter {
					Name = model.Name,
					Login = ""
				};
				DbSession.Save(promoter);
				promoter.Login = promoter.Id.ToString();
				DbSession.Flush();
				model.Login = promoter.Login;

				model.Password = GeneratePassword();
				CreateUserInAD(Promoter.ACC_LOGIN_PREFIX + promoter.Login, model.Password);

				Association association;
				if (model.CreateAssociation) {
					association = new Association {
						Name = model.Name
					};
					DbSession.Save(association);
					DbSession.Flush();
				} else {
					association = DbSession.Query<Association>().First(r => r.Id == model.AssociationId);
				}
				var promoterAssociation = new PromoterAssociation {
					Association = association,
					Promoter = promoter
				};
				DbSession.Save(promoterAssociation);
			}
			catch (Exception ex) {
				DbSession.Transaction.Rollback();
#if !DEBUG
				Membership.DeleteUser(Promoter.ACC_LOGIN_PREFIX + model.Login);
#endif
				ModelState.AddModelError("", ex.Message);
				return View(model);
			}

			return View("Confirm", model);
		}

		private void CreateUserInAD(string login, string password)
		{
#if !DEBUG
			var root = new DirectoryEntry("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var userGroup = new DirectoryEntry("LDAP://CN=Базовая группа клиентов - получателей данных,OU=Группы,OU=Клиенты,DC=adc,DC=analit,DC=net");
			var user = root.Children.Add("CN=" + login, "user");
			user.Properties["samAccountName"].Value = login;
			user.Properties["description"].Value = "Пользователь Интерфейса маркетолога";
			user.CommitChanges();
			user.Invoke("SetPassword", password);
			user.Properties["userAccountControl"].Value = 66048;
			user.CommitChanges();
			userGroup.Invoke("Add", user.Path);
			userGroup.CommitChanges();
			root.CommitChanges();
#endif
		}

		[HttpPost]
		public ActionResult Confirm(RegisterViewModel model)
		{
			if (model.SendEmail) {
				var body = $"Зарегистрирован новый организатор маркетинговых мероприятий <b>\"{model.Name}\"</b><br/>" +
					$"Логин: <b>{model.Login}</b><br/>" +
					$"Пароль: <b>{model.Password}</b><br/>";
#if DEBUG
				var email = ConfigurationManager.AppSettings["DebugEmail"];
#else
				var user = Membership.GetUser(User.Identity.Name);
				var email = user.Email;
				if (string.IsNullOrEmpty(email))
					email = ConfigurationManager.AppSettings["DebugEmail"];
#endif
				MailHelper.SendMail("Интерфейс маркетолога: регистрация организатора", body, email);
			}
			return RedirectToAction("Register");
		}

		private string GeneratePassword()
		{
			var availableChars = "23456789qwertyupasdfghjkzxcvbnmQWERTYUPASDFGHJKLZXCVBNM";
			var password = String.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}

		public ActionResult PromoterProfile()
		{
			var model = new ProfileViewModel();
			model.AvailableAssociations = GetPromoterAssociationList();
			model.AssociationId = CurrentAssociation.Id;
			return View(model);
		}

		[HttpPost]
		public ActionResult PromoterProfile(ProfileViewModel model)
		{
			if (!ModelState.IsValid) {
				model.AvailableAssociations = GetPromoterAssociationList();
				return View(model);
			}

			var promoter = DbSession.Query<Promoter>().FirstOrDefault(r => r.Id == CurrentPromoter.Id);
			if (promoter == null)
				return HttpNotFound();
			var association = DbSession.Query<Association>().FirstOrDefault(r => r.Id == model.AssociationId);
			if (association == null)
				return HttpNotFound();
			CurrentAssociation = association;
			promoter.LastAssociationId = association.Id;
			DbSession.Flush();
			CurrentPromoter.LastAssociationId = promoter.LastAssociationId;
			DbSession.Evict(CurrentPromoter);

			return RedirectToAction("Index", "Home");
		}

		[HttpPost]
		public ActionResult AssociationList()
		{
			var model = new ProfileViewModel();
			model.AvailableAssociations = GetPromoterAssociationList();
			return PartialView("_AssociationList", model);
		}

		private IList<AssociationItemViewModel> GetPromoterAssociationList()
		{
			return DbSession.Query<PromoterAssociation>()
				.Where(r => r.Promoter == CurrentPromoter)
				.Select(r => r.Association)
				.OrderBy(r => r.Name)
				.ToList()
				.Select(r => new AssociationItemViewModel {
					AssociationId = r.Id,
					Name = r.Name,
					Regions = string.Join(", ", r.Regions.Select(x => x.Region.Name).OrderBy(x => x))
				})
				.ToList();
		}
	}
}