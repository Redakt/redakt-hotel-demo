using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Editors;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Pages;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.HotelDoubleBed)]
    [NameFormat("[Rooms]")]
    public class RoomCarousel: ModuleWithIntroBase
    {
        [AllowContentType(typeof(RoomDetail))]
        [CultureInvariant]
        [MinCount(3)]
        public IReadOnlyList<Link> Rooms { get; set; }
    }
}
