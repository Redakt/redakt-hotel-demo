using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.BackOfficeExtensions.Components;
using RedaktHotel.BackOfficeExtensions.Models;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.Interface.MapPointer)]
    public class MapsLocation: ModuleBase
    {
        [Editor(typeof(GeoLocationEditor))]
        public GeoLocation Location { get; set; }
    }
}
