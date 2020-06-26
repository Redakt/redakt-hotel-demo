using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Models.Embedded;
using System.Collections.Generic;

namespace RedaktHotel.Models.Pages
{
    [Page]
    [Icon(ContentIcons.FileText)]
    [AllowChildren(typeof(ContentPage))]
    [AllowView("ContentPage", "RoomList", "FacilitiesList")]
    public class ContentPage : SimplePage
    {
        [Section("Modules")]
        [HideLabel]
        [HelpText("This list is language-dependent, therefore every language will have its separate set of modules.")]
        public IReadOnlyList<ModuleBase> Modules { get; set; }
    }
}
