using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using RedaktHotel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedaktHotel.Components
{
    public partial class GeoLocationDialog
    {
        private string MapId { get; } = "_" + Guid.NewGuid().ToString("N");

        [Inject]
        private IJSRuntime _js { get; set; }

        [Parameter]
        public GeoLocation Location { get; set; }

        protected override void OnInitialized()
        {
            // Create a new geo location object if none was passed from the caller.
            if (this.Location == null) this.Location = new GeoLocation();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                // This calls the map initialization script in /assets/backoffice/custom.js
                await _js.InvokeVoidAsync("redaktHotel.initializeMap", this.MapId);
            }

            await base.OnAfterRenderAsync(firstRender);
        }
    }
}
