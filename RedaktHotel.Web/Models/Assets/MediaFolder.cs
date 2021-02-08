using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Assets
{
    [Folder]
    [AllowAtRoot]
    [AllowChildren(typeof(MediaFolder), typeof(Image), typeof(Video), typeof(Document))]
    [Icon(ContentIcons.Folders.Picture)]
    public class MediaFolder: IContentType
    {
    }
}
