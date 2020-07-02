using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Components;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    public partial class BookingDetail
    {
        #region [ Parameters ]
        [Parameter]
        public string BookingId { get; set; }
        #endregion
    }
}
