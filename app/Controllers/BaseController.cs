using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;

namespace Marketing.Controllers
{
	public class BaseController : Controller
	{
		protected ISession DbSession => HttpContext.Items[typeof(ISession)] as ISession;
	}
}