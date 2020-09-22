using Microsoft.AspNetCore.Mvc;
using Redakt.ContentManagement.Web;

namespace RedaktHotel.Web.ViewComponents
{
    public class NavigationViewComponent : ViewComponent
    {
        private readonly IRedaktContext _context;

        public NavigationViewComponent(IRedaktContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            return View(_context);
        }
    }
}