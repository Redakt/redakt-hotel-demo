using Redakt.ContentManagement.Annotations;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Web.Models.Embedded
{
    public abstract class ModuleWithIntroBase: ModuleBase
    {
        [CultureDependent]
        [DisplayOrder(0)]
        [Required]
        public string Heading { get; set; }

        [CultureDependent]
        [DisplayOrder(1)]
        public string HeadingCaption { get; set; }

        [CultureDependent]
        [DisplayOrder(2)]
        public string IntroText { get; set; }
    }
}
