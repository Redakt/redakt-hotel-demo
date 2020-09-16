using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Models;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("StaffMember")]
    [Icon(ContentIcons.UserIdCard2)]
    public class StaffMember: IContentType
    {
        public Media Picture { get; set; }

        public string Name { get; set; }

        [CultureDependent]
        public string Role { get; set; }
    }
}
