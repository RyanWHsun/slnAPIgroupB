namespace prjGroupB.DTO {
    public class TAttractionTicketDTO {
        public int FAttractionTicketId { get; set; }

        public int? FAttractionId { get; set; }

        public string? FAttractionName { get; set; }

        public string? FTicketType { get; set; }

        public decimal? FPrice { get; set; }

        public string? FDiscountInformation { get; set; }

        public DateTime? FCreatedDate { get; set; }
    }
}
