using Redakt.ContentManagement.Content;

namespace RedaktHotel.Web.Models.Embedded
{
    public class TimelineItem: IContentType
    {
        public int Year { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
    }
}
