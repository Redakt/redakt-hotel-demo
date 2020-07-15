using Redakt.Serialization;

namespace RedaktHotel.BackOfficeExtensions.Models
{
    [TypeDiscriminator("GeoLocation")]
    public class GeoLocation
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }
}
