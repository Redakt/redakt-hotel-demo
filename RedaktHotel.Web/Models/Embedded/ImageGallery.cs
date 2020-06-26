using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.PictureStackLandscape)]
    [NameFormat("[Images] {Heading}")]
    public class ImageGallery: ModuleWithIntroBase
    {
        [CultureInvariant]
        public IReadOnlyList<Image> Images { get; set; }
    }
}
