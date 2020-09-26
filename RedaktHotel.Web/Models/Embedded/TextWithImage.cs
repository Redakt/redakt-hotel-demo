using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Models;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Embedded
{
    [Icon(ContentIcons.ParagraphImageRight)]
    [TitleFormat("[Text] {{Heading}}")]
    public class TextWithImage: ModuleBase
    {
        [CultureDependent]
        public string Heading { get; set; }

        [CultureDependent]
        public string HeadingCaption { get; set; }

        [RichTextEditor]
        [CultureDependent]
        public string BodyText { get; set; }

        [HelpText("Optional value")]
        public Link ButtonLink { get; set; }

        [CultureDependent]
        public string ButtonText { get; set; }

        public Image Image { get; set; }
    }
}
