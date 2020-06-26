using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.Files;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Video")]
    [Icon(ContentIcons.FileVideoPlay)]
    public class Video: IContentType
    {
        [CultureInvariant]
        public FileDescriptor File { get; set; }

        public string Title { get; set; }
    }
}
