using System.Collections.Generic;
using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.LayoutDashboard1)]
    [NameFormat("[Offers] {Heading}")]
    public class OffersMosaic: ModuleWithIntroBase
    {
        [CultureInvariant]
        public IReadOnlyList<OfferItem> Offers { get; set; }
    }
}
