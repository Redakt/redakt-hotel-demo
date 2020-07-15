using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Redakt.BackOffice.Components.Dialog;

namespace RedaktHotel.BackOfficeExtensions.Bookings
{
    public partial class BookingEditDialog
    {
        #region [ Parameters ]
        [Parameter]
        public BookingEditModel Model { get; set; }
        #endregion

        #region [ Initialization ]
        protected override void OnInitialized()
        {
            if (this.Model == null) throw new InvalidOperationException($"{nameof(Model)} is a required parameter.");
        }
        #endregion

        #region [ Event Handlers ]
        private void OnSubmit()
        {
            this.Close(ModalResult.Ok(false));
        }

        private async Task Delete()
        {
            var confirmResult = await this.Context.ModalDialog.ShowMessageAsync("Delete Booking", "Are you sure you want to delete this booking?", MessageDialogOptions.ConfirmCancel);
            if (confirmResult.Cancelled) return;

            this.Close(ModalResult.Ok(true));
        }
        #endregion
    }
}
