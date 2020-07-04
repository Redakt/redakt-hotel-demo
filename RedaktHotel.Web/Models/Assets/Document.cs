using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Models;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Document")]
    [Icon(ContentIcons.FileText)]
    [MediaUpload]
    public class Document: IContentType
    {
        public Media File { get; set; }

        public string Title { get; set; }
    }
}
