using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Assets
{
    [Folder]
    [AllowAtRoot]
    [AllowChildren(typeof(StaffMember))]
    [Icon(ContentIcons.Folder)]
    public class StaffFolder: IContentType
    {
    }
}
