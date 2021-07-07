using System.Collections.Generic;
using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;

namespace RedaktHotel.Web.Models.Embedded
{
    [TitleFormat("[Offers] {{Heading}}")]
    public class OffersMosaic: ModuleWithIntroBase
    {
        public IReadOnlyList<OfferItem> Offers { get; set; }
    }
}
