﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Models.Assets;
using System.Collections.Generic;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.PictureStackLandscape)]
    [NameFormat("[Images] {Heading}")]
    public class ImageGallery: ModuleWithIntroBase
    {
        [CultureInvariant]
        public IReadOnlyList<Image> Images { get; set; }
    }
}
