using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.Files;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Document")]
    [Icon(ContentIcons.FileText)]
    [MediaUpload]
    public class Document: IContentType
    {
        public FileDescriptor File { get; set; }

        public string Title { get; set; }
    }
}
