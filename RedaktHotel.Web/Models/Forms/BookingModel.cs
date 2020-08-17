using System;

namespace RedaktHotel.Web.Models.Forms
{
    public class BookingModel
    {
        public string RoomName { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }

        public int NumberOfAdults { get; set; }

        public int? NumberOfChildren { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string SpecialRequirements { get; set; }

        public string RedirectToUrl { get; set; }
    }
}
