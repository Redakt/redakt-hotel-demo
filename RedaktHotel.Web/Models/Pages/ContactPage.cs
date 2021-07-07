using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.BackOfficeExtensions.Components;
using RedaktHotel.BackOfficeExtensions.Models;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.Phone)]
    [AllowView("ContactPage")]
    public class ContactPage : SimplePage
    {
        [Editor(typeof(GeoLocationEditor))]
        [HelpText("This is an example of a custom property editor.")]
        public GeoLocation Location { get; set; }
    }
}
