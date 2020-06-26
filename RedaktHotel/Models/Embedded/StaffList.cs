using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Models.Assets;
using System.Collections.Generic;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.UserFullBodyCircle)]
    public class StaffList: ModuleWithIntroBase
    {
        public IReadOnlyList<StaffMember> Members { get; set; }
    }
}
