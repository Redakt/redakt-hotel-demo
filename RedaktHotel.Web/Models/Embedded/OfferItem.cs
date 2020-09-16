using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Models;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [NameFormat("[Offer] {Title}")]
    public class OfferItem: IContentType
    {
        public Image Image { get; set; }

        [CultureDependent]
        public string Title { get; set; }

        [CultureDependent]
        public string Text { get; set; }

        public Link ReadMore { get; set; }
    }
}
