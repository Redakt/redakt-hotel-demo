using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FormNew)]
    [AllowView("BookingForm")]
    public class BookingPage : SimplePage
    {
    }
}
