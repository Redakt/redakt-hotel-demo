using System.ComponentModel.DataAnnotations;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;

namespace RedaktHotel.Web.Models.Pages
{
    public abstract class PageBase: IContentType
    {
        [Section("General")]
        [Required]
        public string PageTitle { get; set; }

        [Section("General")]
        [Tooltip("The title for the page in navigation and breadcrumbs. Uses the page name if not specified.")]
        public string NavigationTitle { get; set; }

        [Section("General")]
        [HideLabel]
        [Checkbox(Label = "Hide in main navigation")]
        [CultureInvariant]
        public bool HideInNavigation { get; set; }
    }
}
