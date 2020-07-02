using System;
using Redakt.Data.DocumentStore;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    public class BookingEntity: DocumentEntity
    {
        public string Status { get; set; }

        public DateTime CheckInDate { get; set; }

        public DateTime CheckOutDate { get; set; }
        
        public string Room { get; set; }

        public int NumberOfAdults { get; set; }

        public int NumberOfChildren { get; set; }

        public string GuestFirstName { get; set; }

        public string GuestLastName { get; set; }

        public string GuestEmail { get; set; }

        public string GuestPhone { get; set; }

        public string Comments { get; set; }
    }
}
