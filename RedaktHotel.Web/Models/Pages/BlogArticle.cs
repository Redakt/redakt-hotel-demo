using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.BookOpenBookmark)]
    [AllowView("BlogArticle")]
    public class BlogArticle : SimplePage
    {
        [Required]
        [CultureInvariant]
        [Section("Article")]
        public Image Image { get; set; }

        [Required]
        [CultureInvariant]
        [Section("Article")]
        public DateTime PublicationDate { get; set; }

        [Required]
        [CultureInvariant]
        [Section("Article")]
        public string Author { get; set; }

        [SelectList("news", "hotel", "dining", "facilities", "offers", "events", "excursions", "spa")]
        [CultureInvariant]
        [Section("Article")]
        public IReadOnlyList<string> Categories { get; set; }

        [Required]
        [Multiline]
        [Section("Article")]
        public string ListDescription { get; set; }

        [Section("Article")]
        [RichTextEditor]
        [Required]
        public string Body { get; set; }
    }
}
