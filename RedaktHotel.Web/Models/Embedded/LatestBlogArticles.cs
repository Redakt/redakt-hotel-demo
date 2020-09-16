using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.ListBullets1)]
    [NameFormat("[Blog] {Heading}")]
    public class LatestBlogArticles: ModuleWithIntroBase
    {
        [Required]
        public Link Parent { get; set; }
    }
}
