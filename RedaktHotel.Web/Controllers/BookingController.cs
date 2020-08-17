using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Redakt.ContentManagement.Publishing;
using Redakt.Data.DocumentStore;
using RedaktHotel.BackOfficeExtensions.Bookings;
using RedaktHotel.Web.Models.Forms;

namespace RedaktHotel.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IRepository<BookingEntity> _repository;
        private readonly IPublishService _publishService;

        public BookingController(IRepository<BookingEntity> repository, IPublishService publishService)
        {
            _repository = repository;
            _publishService = publishService;
        }

        [HttpPost("/forms/booking")]
        public async Task<IActionResult> PostForm([FromForm]BookingModel model)
        {
            var entity = new BookingEntity
            {
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfAdults = model.NumberOfAdults,
                NumberOfChildren = model.NumberOfChildren ?? 0,
                GuestFirstName = model.FirstName,
                GuestLastName = model.LastName,
                GuestEmail = model.EmailAddress,
                GuestPhone = model.PhoneNumber,
                Comments = model.SpecialRequirements,
                Room = model.RoomName,
                Status = "open"
            };

            await _repository.UpsertAsync(entity).ConfigureAwait(false);

            return Redirect(model.RedirectToUrl);
        }
    }
}
