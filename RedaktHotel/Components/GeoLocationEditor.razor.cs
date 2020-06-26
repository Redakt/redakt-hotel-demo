using Microsoft.AspNetCore.Components;
using Redakt.BackOffice;
using Redakt.BackOffice.Components.Dialog;
using RedaktHotel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedaktHotel.Components
{
    public partial class GeoLocationEditor
    {
        // The BackOfficeHelper contains convenience methods for localization, dialogs, navigation, etc.
        [Inject]
        private BackOfficeHelper Helper {  get; set; }

        private string Label { get; set; }

        protected override void OnInitialized()
        {
            this.Label = this.SingleValue != null ? $"{SingleValue.Latitude}, {SingleValue.Longitude}" : null;
        }

        private async Task OpenPicker()
        {
            // Open the geo location dialog so the user can selecxt a location.
            var dialogResult = await this.Helper.ModalDialog.ShowAsync<GeoLocationDialog>(new ModalOptions().SetParameter(nameof(GeoLocationDialog.Location), this.SingleValue));
            if (dialogResult.Cancelled) return;  // User cancelled out of the dialog.

            // The dialog has set the (new) geo location in the result data; update the content value.
            this.SingleValue = dialogResult.Data as GeoLocation;
        }
    }
}
