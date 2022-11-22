using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Redakt.BackOffice;

namespace RedaktHotel.BackOfficeExtensions
{
    /// <summary>
    /// Classes implementing <see cref="IBackOfficeModule"/> are automatically scanned on startup and can configure services and resources to load into the back office application.
    /// In this example, back office components are included in the main web project. For larger projects, it is recommended to move back office components into a separate project.
    /// </summary>
    public class CustomBackOfficeModule : IBackOfficeModule
    {
        // These scripts will be included when loading the back office.
        public ICollection<string> ScriptAssetPaths { get; } = new List<string>
        {
            "https://api.mapbox.com/mapbox-gl-js/v1.10.0/mapbox-gl.js",
            // Local assets path is based on the assembly name (see https://docs.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-3.1&tabs=visual-studio#consume-content-from-a-referenced-rcl).
            "/_content/RedaktHotel.BackOfficeExtensions/backoffice-custom.js"
        };

        // These styles will be included when loading the back office.
        public ICollection<string> CssAssetPaths { get; } = new List<string>
        {
            "https://api.mapbox.com/mapbox-gl-js/v1.10.0/mapbox-gl.css"
        };
    }
}
