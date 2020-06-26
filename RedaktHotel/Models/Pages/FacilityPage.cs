﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Models.Assets;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FileStar)]
    [AllowView("FacilityPage")]
    public class FacilityPage: ContentPage
    {
        [Section("Facility")]
        [CultureInvariant]
        public IReadOnlyList<Image> Images { get; set; }

        [Section("Facility")]
        [Multiline]
        [Required]
        [Tooltip("This description will be used when facilities are displayed as a list")]
        public string ListDescription { get; set; }
    }
}
