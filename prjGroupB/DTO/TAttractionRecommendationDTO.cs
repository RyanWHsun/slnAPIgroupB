namespace prjGroupB.DTO {
    public class TAttractionRecommendationDTO {
        public int FAttractionRecommendationId { get; set; } // Primary Key
        public int? FAttractionId { get; set; }
        public string? FAttractionName { get; set; }
        public int? FRecommendationId { get; set; } // 被推薦景點的 ID
        public string? FRecommendAttractionName { get; set; } // 被推薦景點的名稱
        public string? FReason { get; set; }
    }
}
