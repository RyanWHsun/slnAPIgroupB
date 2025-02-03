using prjGroupB.Models;
using System.ComponentModel.DataAnnotations;

namespace prjGroupB.DTO {
    public class TAttractionDTO {
        public int FAttractionId { get; set; }

        public string? FAttractionName { get; set; }

        public int? FCategoryId { get; set; }

        public string? FCategoryName { get; set; }

        public string? FDescription { get; set; }

        public string? FRegion { get; set; }

        public string? FAddress { get; set; }

        public string? FStatus { get; set; }

        public TimeOnly? FOpeningTime { get; set; }

        public TimeOnly? FClosingTime { get; set; }

        public string? FPhoneNumber { get; set; }

        public string? FWebsiteUrl { get; set; }

        public DateTime? FCreatedDate { get; set; }

        public DateTime? FUpdatedDate { get; set; }

        public string? FTrafficInformation { get; set; }

        public string? FLongitude { get; set; }

        public string? FLatitude { get; set; }
    }
}
