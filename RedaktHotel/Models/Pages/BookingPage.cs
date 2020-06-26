using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;

namespace RedaktHotel.Models.Pages
{
    [Page]
    [Icon(ContentIcons.SeoSearchPage)]
    [AllowView("BookingForm")]
    public class BookingPage : SimplePage
    {
    }
}
