using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Content;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.Web.Picture)]
    [TitleFormat("[Slider] {{Title}} - {{Subtitle}}")]
    public class SliderItem: IContentType
    {
        [CultureDependent]
        public string Caption { get; set; }

        [CultureDependent]
        public string Title { get; set; }

        [CultureDependent]
        public string Subtitle { get; set; }

        public Image BackgroundImage { get; set; }
    }
}
