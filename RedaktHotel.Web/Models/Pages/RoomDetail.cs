using Redakt.BackOffice.Icons;
using Redakt.ContentManagement.Annotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RedaktHotel.BackOfficeExtensions.Models;
using RedaktHotel.Web.Models.Assets;

namespace RedaktHotel.Web.Models.Pages
{
    [Page]
    [Icon(ContentIcons.HotelSingleBed1)]
    [AllowView("RoomDetail")]
    public class RoomDetail: SimplePage
    {
        [Section("Room")]
        [Required]
        [Inline("room-images", FieldWidth = "auto")]
        public Image MainImage { get; set; }

        [Section("Room")]
        [Inline("room-images")]
        [MaxCount(5)]
        public ICollection<Image> AdditionalImages { get; set; }

        [Section("Room")]
        [Multiline]
        [Required]
        [CultureDependent]
        [Tooltip("This description will be used when rooms are displayed as a list")]
        public string ListDescription { get; set; }

        [Section("Room")]
        [RichTextEditor]
        [Required]
        [CultureDependent]
        public string LongDescription { get; set; }

        [Section("Room")]
        [SelectList(typeof(RoomFeatureOptions))]
        //[Width("100%")]
        public IReadOnlyList<string> Features { get; set; }

        [Section("Room")]
        [Inline("room-rate", FieldWidth = "240px")]
        [Range(20, 2000)]
        [NumberEditor(Decimals = 2, Prefix = "€")]
        public decimal NightlyRate { get; set; }

        [Section("Room")]
        [Inline("room-rate", FieldWidth = "240px")]
        [Range(20, 2000)]
        [NumberEditor(Decimals = 2, Prefix = "€")]
        public decimal? DiscountedFrom { get; set; }
    }
}
