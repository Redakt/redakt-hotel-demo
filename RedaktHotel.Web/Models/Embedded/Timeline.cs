using Redakt.ContentManagement.BackOffice.Content.Editors.NestedContent;
using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;

namespace RedaktHotel.Web.Models.Embedded
{
    public class Timeline: ModuleWithIntroBase
    {
        [DisplayOrder(3)]
        [TableEditor]
        [MinCount(2)]
        public IReadOnlyList<TimelineItem> Items { get; set; }
    }
}
