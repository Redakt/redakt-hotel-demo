using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Redakt.BackOffice;
using Redakt.BackOffice.Annotations;
using Redakt.BackOffice.Components.Dialog;
using Redakt.BackOffice.ContentManagement.Nodes.Dialogs;
using Redakt.BackOffice.ContentManagement.Nodes.EditModels;
using Redakt.Data.DocumentStore;
using Redakt.Extensions;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    [Authorize]
    [NavigationItem("Bookings", "bookings", "modules", DisplayOrder = 10, Icon = "calendar|Application")]
    public partial class BookingOverview
    {
        #region [ Fields ]
        private string _currentStatus;
        #endregion

        #region [ Dependency Injection
        [Inject]
        private BackOfficeHelper Helper { get; set; }

        [Inject]
        private IRepository<BookingEntity> BookingRepository { get; set; }
        #endregion

        #region [ Parameters ]
        [Parameter]
        public string Status { get; set; }
        #endregion

        #region [ Properties ]
        private bool IsInitialized { get; set; }

        private IList<BookingEntity> Bookings { get; set; }
        #endregion

        #region [ Initialization ]
        protected override void OnInitialized()
        {
            if (string.IsNullOrWhiteSpace(this.Status)) this.Helper.Navigation.NavigateTo("bookings/open");
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrWhiteSpace(this.Status) && _currentStatus != this.Status)
            {
                _currentStatus = this.Status;
                this.Bookings = await this.BookingRepository.FindManyAsync(x => x.Status == _currentStatus).ToListAsync();
                
                this.IsInitialized = true;
            }
        }
        #endregion

        #region [ Event Handlers ]
        private async Task OnNewBooking()
        {
            var model = new BookingEditModel { Status = this.Status };

            // Open booking edit dialog.
            var dialogResult = await this.Helper.ModalDialog.ShowAsync<BookingEditDialog>(new ModalOptions().SetParameter(nameof(BookingEditDialog.Model), model));
            if (dialogResult.Cancelled) return;  // User has cancelled the action.

            // The model now contains validated data for the new booking.
            var entity = new BookingEntity();
            model.PopulateEntity(entity);

            await SaveBooking(entity);
        }

        private async Task OnBookingClicked(BookingEntity booking)
        {
            var model = BookingEditModel.FromEntity(booking);

            // Open booking edit dialog.
            var dialogResult = await this.Helper.ModalDialog.ShowAsync<BookingEditDialog>(new ModalOptions().SetParameter(nameof(BookingEditDialog.Model), model));
            if (dialogResult.Cancelled) return;  // User has cancelled the action.

            if ((bool) dialogResult.Data)
            {
                try
                {
                    // Delete booking
                    await this.BookingRepository.DeleteAsync(booking.Id);

                    await this.Helper.Notifications.ShowSuccessAsync(null, "The booking has been deleted.");
                }
                catch (Exception ex)
                {
                    await this.Helper.Notifications.ShowErrorAsync("Error", ex.Message);
                }
                
                this.Bookings.Remove(booking);
            }
            else
            {
                // The model now contains validated data for the booking.
                model.PopulateEntity(booking);

                await SaveBooking(booking);
            }
        }

        private async Task SaveBooking(BookingEntity entity)
        {
            try
            {
                // Save the new booking to the data store.
                await this.BookingRepository.UpsertAsync(entity);

                // Add the booking to the current collection if it is the same status.
                if (entity.Status == this.Status) this.Bookings.Add(entity);

                await this.Helper.Notifications.ShowSuccessAsync(null, "The new booking has been created.");
            }
            catch (Exception ex)
            {
                await this.Helper.Notifications.ShowErrorAsync("Error", ex.Message);
            }
        }
        #endregion
    }
}
