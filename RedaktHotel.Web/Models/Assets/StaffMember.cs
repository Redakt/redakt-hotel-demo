using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.Files;

namespace RedaktHotel.Web.Models.Assets
{
    [Asset]
    [Key("StaffMember")]
    [Icon(ContentIcons.UserIdCard2)]
    public class StaffMember: IContentType
    {
        [CultureInvariant]
        public ImageFile Picture { get; set; }

        [CultureInvariant]
        public string Name { get; set; }

        public string Role { get; set; }
    }
}
