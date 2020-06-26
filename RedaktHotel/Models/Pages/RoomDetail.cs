using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using RedaktHotel.Data;
using RedaktHotel.Models.Assets;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RedaktHotel.Models.Pages
{
    [Page]
    [Icon(ContentIcons.HotelSingleBed1)]
    [AllowView("RoomDetail")]
    public class RoomDetail: SimplePage
    {
        [Section("Room")]
        [CultureInvariant]
        [Required]
        [Inline("room-images", FieldWidth = "auto")]
        public Image MainImage { get; set; }

        [Section("Room")]
        [CultureInvariant]
        [Inline("room-images")]
        [MaxCount(5)]
        public ICollection<Image> AdditionalImages { get; set; }

        [Section("Room")]
        [Multiline]
        [Required]
        [Tooltip("This description will be used when rooms are displayed as a list")]
        public string ListDescription { get; set; }

        [Section("Room")]
        [RichTextEditor]
        [Required]
        public string LongDescription { get; set; }

        [Section("Room")]
        [SelectList(typeof(RoomFeatureOptions))]
        //[Width("100%")]
        public IReadOnlyList<string> Features { get; set; }

        [Section("Room")]
        [Inline("room-rate", FieldWidth = "240px")]
        [CultureInvariant]
        [Range(20, 2000)]
        [NumberEditor(Decimals = 2, Prefix = "€")]
        public decimal NightlyRate { get; set; }

        [Section("Room")]
        [Inline("room-rate", FieldWidth = "240px")]
        [CultureInvariant]
        [Range(20, 2000)]
        [NumberEditor(Decimals = 2, Prefix = "€")]
        public decimal? DiscountedFrom { get; set; }
    }
}
