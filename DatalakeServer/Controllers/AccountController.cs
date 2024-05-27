using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace DatalakeServer.Controllers
{
	public class AccountController : Controller
	{
		public IActionResult Login(string returnUrl = "/")
		{
			return Challenge(new AuthenticationProperties()
			{
				RedirectUri = returnUrl
			});
		}

		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return Redirect("http://localhost:9090/realms/energo/protocol/openid-connect/logout?client_id=datalake&post_logout_redirect_uri=" + HttpUtility.UrlEncode("http://localhost:5018/"));
		}

		public IActionResult NoAccess()
		{
			return View();
		}
	}
}
