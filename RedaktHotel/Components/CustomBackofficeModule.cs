using System.Collections.Generic;
using Redakt.BackOffice;

namespace RedaktHotel.Components
{
    /// <summary>
    /// Classes implementing <see cref="IBackOfficeModule"/> are automatically scanned on startup and can configure services and resources to load into the back office application.
    /// In this example, back office components are included in the main web project. For larger projects, it is recommended to move back office components into a separate project.
    /// </summary>
    public class CustomBackOfficeModule : IBackOfficeModule
    {
        // These scripts will be included in the back office.
        public ICollection<string> ScriptAssetPaths { get; } = new List<string>
        {
            "https://api.mapbox.com/mapbox-gl-js/v1.10.0/mapbox-gl.js",
            "/assets/backoffice/custom.js"
        };

        // These styles will be included in the backoffice.
        public ICollection<string> CssAssetPaths { get; } = new List<string>
        {
            "https://api.mapbox.com/mapbox-gl-js/v1.10.0/mapbox-gl.css"
        };
    }
}
