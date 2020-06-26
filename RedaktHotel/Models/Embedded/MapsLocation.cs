using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Components;
using RedaktHotel.Data;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.MapsPin)]
    public class MapsLocation: ModuleBase
    {
        [Editor(typeof(GeoLocationEditor))]
        public GeoLocation Location { get; set; }
    }
}
