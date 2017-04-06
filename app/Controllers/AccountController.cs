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
using log4net;

namespace Marketing.Controllers
{

	[Authorize]
	public class AccountController : BaseController
	{

		private static ILog _log = LogManager.GetLogger(typeof(AccountController));

		[AllowAnonymous]
		public ActionResult Login()
		{
			ViewBag.ReturnUrl = Request.QueryString["returnUrl"] ?? "/";
			return View(new LoginViewModel());
		}

		[AllowAnonymous]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Login(LoginViewModel model, string returnUrl)
		{
			if (!ModelState.IsValid)
				return View(model);

			var login = Promoter.ACC_LOGIN_PREFIX + model.Login;
			var skipValidation = false;
#if DEBUG
			skipValidation = !string.IsNullOrEmpty(ConfigurationManager.AppSettings["SkipLogonAD"]);
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

			var user = DbSession.QueryOver<Promoter>().Where(r => r.Login == model.Login).List().FirstOrDefault();
			if (user == null) {
				var admin = DbSession.QueryOver<RegionalAdmin>().Where(r => r.Name == model.Login).List().Count > 0;
				if (admin) {
					FormsAuthentication.SetAuthCookie(model.Login, model.RememberMe);
					return RedirectToAction("Register");
				}
				ModelState.AddModelError("",
					$"Пользователь \"{model.Login}\" не включен в список организаторов акций. Обратитесь в АналитФармацию.");
				return View(model);
			}
			FormsAuthentication.SetAuthCookie(login, model.RememberMe);
			if ((returnUrl ?? "/") == "/")
				returnUrl = "/marketing/";
			return Redirect(returnUrl);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Logout()
		{
			FormsAuthentication.SignOut();
			DbSession.Clear();
			return RedirectToAction("Login");
		}

		public ActionResult Register()
		{
			if (CurrentPromoter?.Login != null) //если задан пользователь с логином это не админ (оперативное решение)
				return RedirectToAction("Index", "Home");

			return View(new RegisterViewModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Register(RegisterViewModel model)
		{
			if (CurrentPromoter?.Login != null) //если задан пользователь с логином это не админ (оперативное решение)
				return RedirectToAction("Index", "Home");

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
			} catch (Exception ex) {
				DbSession.Transaction.Rollback();
				_log.Error(ex);
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
		[ValidateAntiForgeryToken]
		public ActionResult Confirm(bool sendEmail)
		{
			if (CurrentPromoter?.Login != null) //если задан пользователь с логином это не админ (оперативное решение)
				return RedirectToAction("Index", "Home");

			return RedirectToAction("Register");
		}

		private string GeneratePassword()
		{
			var availableChars = "23456789qwertyupasdfghjkzxcvbnmQWERTYUPASDFGHJKLZXCVBNM";
			var password = string.Empty;
			var random = new Random();
			while (password.Length < 8)
				password += availableChars[random.Next(0, availableChars.Length - 1)];
			return password;
		}
	}
}