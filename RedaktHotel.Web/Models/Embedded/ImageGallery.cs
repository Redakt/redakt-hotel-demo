﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.Media.Picture)]
    [TitleFormat("[Images] {{Heading}}")]
    public class ImageGallery: ModuleWithIntroBase
    {
        public IReadOnlyList<Image> Images { get; set; }
    }
}
