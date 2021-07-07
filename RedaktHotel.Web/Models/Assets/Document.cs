using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("Document")]
    [Icon(ContentIcons.Document)]
    [MediaUpload]
    public class Document: IContentType
    {
        public Media File { get; set; }

        public string Title { get; set; }
    }
}
