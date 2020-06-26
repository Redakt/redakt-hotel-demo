using Redakt.Serialization;

namespace RedaktHotel.BackOfficeExtensions.Models
{
    [KnownType("GeoLocation")]
    public class GeoLocation
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
