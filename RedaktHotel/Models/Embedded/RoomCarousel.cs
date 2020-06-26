using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Editors;
using RedaktHotel.Models.Pages;
using System.Collections.Generic;

namespace RedaktHotel.Models.Embedded
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
