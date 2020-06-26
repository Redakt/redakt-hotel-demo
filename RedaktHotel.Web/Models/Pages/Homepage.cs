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
        [CultureInvariant]
        public IReadOnlyList<SliderItem> SliderItems { get; set; }

        [Section("Modules")]
        [HideLabel]
        [CultureInvariant]
        [HelpText("This list is language invariant, therefore the same set of modules is shared between languages.")]
        public IReadOnlyList<ModuleBase> Modules { get; set; }
    }
}
