using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.BackOfficeExtensions.Components;
using RedaktHotel.BackOfficeExtensions.Models;

namespace RedaktHotel.Models.Pages
{
    [Page]
    [Icon(ContentIcons.SeoSearchPage)]
    [AllowView("ContactPage")]
    public class ContactPage : SimplePage
    {
        [Editor(typeof(GeoLocationEditor))]
        [HelpText("This is an example of a custom property editor.")]
        public GeoLocation Location { get; set; }
    }
}
