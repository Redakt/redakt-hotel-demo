using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;

namespace RedaktHotel.Web.Models.Assets
{
    [Folder]
    [AllowAtRoot]
    [AllowChildren(typeof(MediaFolder), typeof(Image), typeof(Video), typeof(Document))]
    [Icon(ContentIcons.FolderImage)]
    public class MediaFolder: IContentType
    {
    }
}
