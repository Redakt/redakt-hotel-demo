using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FileText)]
    [AllowChildren(typeof(ContentPage), typeof(FacilityPage), typeof(RoomDetail), typeof(BlogArticle))]
    [AllowView("RoomList", "FacilitiesList", "BlogOverview")]
    public class SimplePage : PageBase
    {
        [Section("Heading")]
        [DisplayName("Caption")]
        [CultureDependent]
        public string HeadingCaption { get; set; }

        [Section("Heading")]
        [DisplayName("Introduction")]
        [Multiline]
        [CultureDependent]
        public string HeadingIntro { get; set; }

        [Section("Heading")]
        [DisplayName("Background Image")]
        public Image HeadingBackgroundImage { get; set; }
    }
}
