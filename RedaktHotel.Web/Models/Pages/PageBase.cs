using System.ComponentModel.DataAnnotations;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Pages
{
    public abstract class PageBase: IContentType
    {
        [Section("General")]
        [Required]
        [CultureDependent]
        public string PageTitle { get; set; }

        [Section("General")]
        [Tooltip("The title for the page in navigation and breadcrumbs. Uses the page name if not specified.")]
        [CultureDependent]
        public string NavigationTitle { get; set; }

        [Section("General")]
        [HideLabel]
        [Checkbox(Label = "Hide in main navigation")]
        public bool HideInNavigation { get; set; }
    }
}
