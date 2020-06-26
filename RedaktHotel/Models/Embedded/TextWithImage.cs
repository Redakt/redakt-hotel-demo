using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Editors;
using RedaktHotel.Models.Assets;

namespace RedaktHotel.Models.Embedded
{
    [Icon(ContentIcons.ParagraphImageRight)]
    [NameFormat("[Text] {Heading}")]
    public class TextWithImage: ModuleBase
    {
        public string Heading { get; set; }

        public string HeadingCaption { get; set; }

        [RichTextEditor]
        public string BodyText { get; set; }

        [HelpText("Optional value")]
        public Link ButtonLink { get; set; }

        public string ButtonText { get; set; }

        [CultureInvariant]
        public Image Image { get; set; }
    }
}
