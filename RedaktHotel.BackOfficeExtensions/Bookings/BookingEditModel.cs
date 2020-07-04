using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Redakt.BackOffice.Forms;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    public class BookingEditModel
    {
        public static readonly IReadOnlyList<SelectOption> StatusOptions = new List<SelectOption>
        {
            new SelectOption("open", "Open"),
            new SelectOption("confirmed", "Confirmed"),
            new SelectOption("cancelled", "Cancelled")
        };

        public bool IsNew { get; private set; } = true;
        
        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime? CheckInDate { get; set; }

        [Required]
        public DateTime? CheckOutDate { get; set; }
        
        [Required]
        public string Room { get; set; }

        //[Required]
        public int? NumberOfAdults { get; set; }

        public int? NumberOfChildren { get; set; }

        public string GuestFirstName { get; set; }

        [Required]
        public string GuestLastName { get; set; }

        [Required]
        public string GuestEmail { get; set; }

        [Required]
        public string GuestPhone { get; set; }

        public string Comments { get; set; }

        #region [ Entity ]
        public void PopulateEntity(BookingEntity entity)
        {
            entity.Status = this.Status;
            entity.CheckInDate = this.CheckInDate.GetValueOrDefault();
            entity.CheckOutDate = this.CheckOutDate.GetValueOrDefault();
            entity.Room = this.Room;
            entity.NumberOfAdults = this.NumberOfAdults.GetValueOrDefault();
            entity.NumberOfChildren = this.NumberOfChildren.GetValueOrDefault();
            entity.GuestFirstName = this.GuestFirstName;
            entity.GuestLastName = this.GuestLastName;
            entity.GuestEmail = this.GuestEmail;
            entity.GuestPhone = this.GuestPhone;
        }

        public static BookingEditModel FromEntity(BookingEntity entity)
        {
            return new BookingEditModel
            {
                IsNew = false,
                Status = entity.Status,
                CheckInDate = entity.CheckInDate,
                CheckOutDate = entity.CheckOutDate,
                Room = entity.Room,
                NumberOfAdults = entity.NumberOfAdults,
                NumberOfChildren = entity.NumberOfChildren,
                GuestFirstName = entity.GuestFirstName,
                GuestLastName = entity.GuestLastName,
                GuestEmail = entity.GuestEmail,
                GuestPhone = entity.GuestPhone,
                Comments = entity.Comments
            };
        }
        #endregion
    }
}
