using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;
[assembly: OwinStartup(typeof(ImplementAuth0.Startup))]
namespace ImplementAuth0
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Enable Kentor Cookie Saver middleware
            app.UseKentorOwinCookieSaver();

            // Set Cookies as default authentication type
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
                LoginPath = new PathString("/Account/Login")
            });
        }
    }
}