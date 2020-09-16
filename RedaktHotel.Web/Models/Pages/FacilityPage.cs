using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FileStar)]
    [AllowView("FacilityPage")]
    public class FacilityPage: ContentPage
    {
        [Section("Facility")]
        public IReadOnlyList<Image> Images { get; set; }

        [Section("Facility")]
        [Multiline]
        [Required]
        [CultureDependent]
        [Tooltip("This description will be used when facilities are displayed as a list")]
        public string ListDescription { get; set; }
    }
}
