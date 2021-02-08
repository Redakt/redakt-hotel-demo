using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.Interface.Calendar)]
    [TitleFormat("[Blog] {{Heading}}")]
    public class LatestBlogArticles: ModuleWithIntroBase
    {
        [Required]
        public Link Parent { get; set; }
    }
}
