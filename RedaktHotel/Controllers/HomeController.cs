using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Redakt.Extensions;

namespace RedaktHotel.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// This controller action demonstrates that Redakt and MVC can work side by side without any issues.
        /// In this example, the MVC controller only serves as a "splash" page, redirecting the browser to one of the supported website language homepages based on the browser detected language.
        /// </summary>
        public IActionResult Index()
        {
            // Detect current browser language and redirect to the appropriate supported website languages.
            var requestContext = Request.HttpContext.Features.Get<IRequestCultureFeature>();
            switch (requestContext.RequestCulture.Culture.NeutralCulture()?.Name)
            {
                case "nl": return Redirect("/nl");
                default: return Redirect("/en");
            }
        }
    }
}