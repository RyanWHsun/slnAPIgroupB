namespace prjGroupB.DTO {
    public class TAttractionTicketShoppingCartDTO {
        public int FCartId { get; set; }
        public int FCartItemId { get; set; }
        public int FUserId { get; set; }
        public int FTicketId { get; set; }
        public int FAttractionId { get; set; }
        public string? FAttractionName { get; set; }
        public string? FImageSrc { get; set; }
        public string? FTicketType { get; set; }
        public decimal FPrice { get; set; }
        public int FQuantity { get; set; }
        public string? FDiscountInformation { get; set; }
        public DateTime? FCreatedDate { get; set; } 
    }
}
