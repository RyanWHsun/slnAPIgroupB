using prjGroupB.Models;

namespace prjGroupB.DTO {
    public class TAttractionCommentDTO {
        public int FCommentId { get; set; }
        public int? FAttractionId { get; set; }
        public string? FAttractionName { get; set; }
        public int? FUserId { get; set; }
        public string? FUserName { get; set; }
        public string? FUserNickName { get; set; }
        public string? FUserImage { get; set; }
        public int? FRating { get; set; }
        public string? FComment { get; set; }
        public DateTime? FCreatedDate { get; set; }
    }
}
