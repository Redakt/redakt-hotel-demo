using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Video")]
    [Icon(ContentIcons.Media.Video)]
    public class Video: IContentType
    {
        public Media File { get; set; }

        [CultureDependent]
        public string Title { get; set; }
    }
}
