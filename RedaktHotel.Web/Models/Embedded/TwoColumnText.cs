using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;

namespace RedaktHotel.Web.Models.Embedded
{
    [TitleFormat("[Columns] {{LeftHeading}} - {{RightHeading}}")]
    [Icon(ContentIcons.ParagraphTwoColumn)]
    public class TwoColumnText: ModuleBase
    {
        [Inline("headingCaption")]
        public string LeftHeadingCaption { get; set; }

        [Inline("headingCaption")]
        public string RightHeadingCaption { get; set; }

        [Inline("heading")]
        public string LeftHeading { get; set; }
        
        [Inline("heading")]
        public string RightHeading { get; set; }

        [Inline("body")]
        [RichTextEditor]
        public string LeftBodyText { get; set; }

        [Inline("body")]
        [RichTextEditor]
        public string RightBodyText { get; set; }
    }
}
