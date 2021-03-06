﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using RedaktHotel.BackOfficeExtensions.Annotations;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.Book)]
    [AllowView("BlogArticle")]
    public class BlogArticle : SimplePage
    {
        [Required]
        [Section("Article")]
        public Image Image { get; set; }

        [Required]
        [MinCurrentDate]  // Custom validation attribute that does not allow dates in the past.
        [Section("Article")]
        public DateTime PublicationDate { get; set; }

        [Required]
        [Section("Article")]
        public string Author { get; set; }

        [SelectList("news", "hotel", "dining", "facilities", "offers", "events", "excursions", "spa")]
        [Section("Article")]
        public IReadOnlyList<string> Categories { get; set; }

        [Required]
        [Multiline]
        [CultureDependent]
        [Section("Article")]
        public string ListDescription { get; set; }

        [Section("Article")]
        [RichTextEditor]
        [Required]
        [CultureDependent]
        public string Body { get; set; }
    }
}
