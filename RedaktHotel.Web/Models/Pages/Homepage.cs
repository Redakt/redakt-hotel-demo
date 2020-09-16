using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using RedaktHotel.Web.Models.Embedded;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.Hotel)]
    [AllowAtRoot]
    [AllowChildren(typeof(ContentPage), typeof(SimplePage), typeof(BookingPage))]
    [AllowView("Homepage")]
    public class Homepage: PageBase
    {
        [Section("Slider Items")]
        [HideLabel]
        public IReadOnlyList<SliderItem> SliderItems { get; set; }

        [Section("Modules")]
        [HideLabel]
        [HelpText("This list is culture invariant, therefore the same set of modules is shared between all cultures.")]
        public IReadOnlyList<ModuleBase> Modules { get; set; }
    }
}
