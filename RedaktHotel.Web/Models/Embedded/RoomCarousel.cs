using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Pages;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.Business.Suitcase)]
    [TitleFormat("[Rooms]")]
    public class RoomCarousel: ModuleWithIntroBase
    {
        [AllowContentType(typeof(RoomDetail))]
        [MinCount(3)]
        public IReadOnlyList<Link> Rooms { get; set; }
    }
}
