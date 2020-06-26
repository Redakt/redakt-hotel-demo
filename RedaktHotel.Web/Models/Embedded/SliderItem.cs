using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.AlignLandscape)]
    [NameFormat("[Slider] {Title} - {Subtitle}")]
    public class SliderItem: IContentType
    {
        public string Caption { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        [CultureInvariant]
        public Image BackgroundImage { get; set; }
    }
}
