using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using ImplementAuth0.Models;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ImplementAuth0.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        private IAuthenticationManager AuthenticationManager
        {
           set;  get; 
        }



        [HttpGet]

        public ActionResult Login(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Login(LoginViewModel vm, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string auth0Domain = ConfigurationManager.AppSettings["auth0:Domain"];
                    string auth0ClientId = ConfigurationManager.AppSettings["auth0:ClientId"];
                    string auth0ClientSecret = ConfigurationManager.AppSettings["auth0:ClientSecret"];

                    AuthenticationApiClient client =
                        new AuthenticationApiClient(
                            new Uri($"https://{auth0Domain}/"));

                    var result = await client.GetTokenAsync(new ResourceOwnerTokenRequest
                    {
                        ClientId = auth0ClientId,
                        ClientSecret = auth0ClientSecret,
                        Scope = "openid profile",
                        Realm = "CustomerApplication", // Specify the correct name of your DB connection
                        Username = vm.EmailAddress,
                        Password = vm.Password
                    });

                    // Get user info from token
                    var user = await client.GetUserInfoAsync(result.AccessToken);

                    // Create claims principal
                    var claimsIdentity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.UserId, "http://www.w3.org/2001/XMLSchema#string", $"https://{auth0Domain}/"),
                        new Claim(ClaimTypes.Name, user.FullName ?? user.Email, "http://www.w3.org/2001/XMLSchema#string", $"https://{auth0Domain}/"),
                    }, CookieAuthenticationDefaults.AuthenticationType);

                    // Sign user into cookie middleware
                    AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = false }, claimsIdentity);

                    return RedirectToLocal(returnUrl);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }
            }

            return View(vm);
        }

        [Authorize]
        public ActionResult Logout()
        {
            AuthenticationManager.SignOut();

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Claims()
        {
            return View();
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}