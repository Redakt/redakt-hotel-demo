using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Editors;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.ListBullets1)]
    [NameFormat("[Blog] {Heading}")]
    public class LatestBlogArticles: ModuleWithIntroBase
    {
        [Required]
        [CultureInvariant]
        public Link Parent { get; set; }
    }
}
