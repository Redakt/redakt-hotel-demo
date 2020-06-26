using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.ContentManagement.Editors;
using RedaktHotel.Models.Assets;

namespace RedaktHotel.Models.Embedded
{
    [NameFormat("[Offer] {Title}")]
    public class OfferItem: IContentType
    {
        [CultureInvariant]
        public Image Image { get; set; }

        public string Title { get; set; }

        public string Text { get; set; }

        public Link ReadMore { get; set; }
    }
}
