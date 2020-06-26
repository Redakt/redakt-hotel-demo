using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Editors;
using RedaktHotel.Models.Pages;
using System.Collections.Generic;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.LayoutModule)]
    [NameFormat("[Facilities] {Heading}")]
    public class FacilitiesGrid : ModuleWithIntroBase
    {
        [AllowContentType(typeof(FacilityPage))]
        [CultureInvariant]
        [MinCount(3)]
        public IReadOnlyList<Link> Facilities { get; set; }
    }
}
