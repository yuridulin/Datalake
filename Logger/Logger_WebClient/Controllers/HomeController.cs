using System.Web.Mvc;

namespace Logger.WebClient.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index() => View();

		public ActionResult Tree() => View();
	}
}