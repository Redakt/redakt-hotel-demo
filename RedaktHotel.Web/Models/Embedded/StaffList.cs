﻿using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    public class StaffList: ModuleWithIntroBase
    {
        public IReadOnlyList<StaffMember> Members { get; set; }
    }
}
