namespace prjGroupB.DTO
{
    public class addProductToCartDTO
    {
        public int FUserId { get; set; }  // 用戶 ID
        public required string FItemType { get; set; } // "eventFee", "attractionTicket", "product"
        public int FItemId { get; set; }
        public int FQuantity { get; set; }
        public decimal FPrice { get; set; }
    }


}
