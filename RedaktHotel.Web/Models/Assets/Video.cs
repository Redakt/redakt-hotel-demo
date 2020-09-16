using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Models;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Video")]
    [Icon(ContentIcons.FileVideoPlay)]
    public class Video: IContentType
    {
        public Media File { get; set; }

        [CultureDependent]
        public string Title { get; set; }
    }
}
