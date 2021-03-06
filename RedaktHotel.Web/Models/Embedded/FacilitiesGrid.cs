﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Pages;

namespace RedaktHotel.Web.Models.Embedded
{
    [TitleFormat("[Facilities] {{Heading}}")]
    public class FacilitiesGrid : ModuleWithIntroBase
    {
        [AllowContentType(typeof(FacilityPage))]
        [MinCount(3)]
        public IReadOnlyList<Link> Facilities { get; set; }
    }
}
