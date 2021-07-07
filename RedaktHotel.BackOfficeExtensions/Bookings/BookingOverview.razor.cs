using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Redakt.BackOffice;
using Redakt.BackOffice.Components;
using Redakt.BackOffice.Components.Dialog;
using Redakt.BackOffice.Components.GridView;
using Redakt.BackOffice.Components.Navigation;
using Redakt.BackOffice.Icons;
using Redakt.Data.DocumentStore;
using Redakt.Extensions;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    [Authorize]
    [NavigationItem("bookings", "Bookings", NavigationOrder = 10, Icon = ApplicationIcons.BookNumber)]
    public partial class BookingOverview: ComponentBase
    {
        #region [ Fields ]
        private string _currentStatusFilter;
        private GridView<BookingEntity> _grid;
        #endregion

        #region [ Dependency Injection
        [Inject]
        private BackOfficeContext Context { get; set; }

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

        private StaticListDataSource<BookingEntity> DataSource { get; set; }
        #endregion

        #region [ Initialization ]
        protected override void OnInitialized()
        {
            if (string.IsNullOrWhiteSpace(this.Status)) this.Context.Navigation.NavigateTo("bookings/open");
        }

        protected override async Task OnParametersSetAsync()
        {
            if (string.IsNullOrWhiteSpace(this.Status) || _currentStatusFilter == this.Status) return;

            _currentStatusFilter = this.Status;
            this.Bookings = await this.BookingRepository.FindManyAsync(x => x.Status == _currentStatusFilter).ToListAsync();
            this.DataSource = new StaticListDataSource<BookingEntity>(this.Bookings);

            this.IsInitialized = true;
        }
        #endregion

        #region [ Event Handlers ]
        private async Task OnNewBooking()
        {
            var model = new BookingEditModel { Status = this.Status };

            // Open booking edit dialog.
            var dialogResult = await this.Context.ModalDialog.ShowAsync<BookingEditDialog>(new ModalOptions().SetParameter(nameof(BookingEditDialog.Model), model));
            if (dialogResult.Cancelled) return;  // User has cancelled the action.

            // The model now contains validated data for the new booking.
            var entity = new BookingEntity();
            model.PopulateEntity(entity);

            await SaveBooking(entity);

            await this.Context.Notifications.ShowSuccessAsync(null, "The new booking has been created.");
        }

        private async Task OnBookingClicked(BookingEntity booking)
        {
            var model = BookingEditModel.FromEntity(booking);

            // Open booking edit dialog.
            var dialogResult = await this.Context.ModalDialog.ShowAsync<BookingEditDialog>(new ModalOptions().SetParameter(nameof(BookingEditDialog.Model), model));
            if (dialogResult.Cancelled) return;  // User has cancelled the action.

            if ((bool) dialogResult.Data)
            {
                try
                {
                    // Delete booking
                    await this.BookingRepository.DeleteAsync(booking.Id);

                    await this.Context.Notifications.ShowSuccessAsync(null, "The booking has been deleted.");
                }
                catch (Exception ex)
                {
                    await this.Context.Notifications.ShowErrorAsync("Error", ex.Message);
                }
                
                this.Bookings.Remove(booking);

                await _grid.ReloadAsync();
            }
            else
            {
                // The model now contains validated data for the booking.
                model.PopulateEntity(booking);

                await SaveBooking(booking);

                await this.Context.Notifications.ShowSuccessAsync(null, "The booking has been updated.");
            }
        }

        private async Task SaveBooking(BookingEntity entity)
        {
            try
            {
                // Save the new booking to the data store.
                await this.BookingRepository.UpsertAsync(entity);

                // Update the current booking list if required.
                if (entity.Status == this.Status && !this.Bookings.Contains(entity)) this.Bookings.Add(entity);
                else if (entity.Status != this.Status && this.Bookings.Contains(entity)) this.Bookings.Remove(entity);
            }
            catch (Exception ex)
            {
                await this.Context.Notifications.ShowErrorAsync("Error", ex.Message);
            }

            await _grid.ReloadAsync();
        }
        #endregion
    }
}
