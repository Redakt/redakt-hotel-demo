using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;

namespace RedaktHotel.Web.Models.Assets
{
    [Folder]
    [AllowAtRoot]
    [AllowChildren(typeof(StaffMember))]
    [Icon(ContentIcons.UserPhoneBook)]
    public class StaffFolder: IContentType
    {
    }
}
