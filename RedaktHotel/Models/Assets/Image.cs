using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using Redakt.ContentManagement.Configuration;
using Redakt.Files;

namespace RedaktHotel.Models.Assets
{
    [Asset]
    [Key("Image")]
    [Icon(ContentIcons.FileImageLandscape)]
    [MediaUpload]
    public class Image: IContentType
    {
        [AcceptMimeType("image/jpeg", "image/gif", "image/png", "image/bmp", "image/tiff", "image/svg+xml")]
        [CultureInvariant]
        [Section("Image")]
        public ImageFile File { get; set; }

        [Section("Image")]
        public string AlternateText { get; set; }
    }
}
