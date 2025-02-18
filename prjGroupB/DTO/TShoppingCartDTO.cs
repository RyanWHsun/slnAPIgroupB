namespace prjGroupB.DTO
{
    public class TShoppingCartDTO
    {
        public int? FUserId { get; set; }
        public int FCartItemId { get; set; }
        public required string FItemType { get; set; }
        public int? FItemId { get; set; } 
        public required string FItemName { get; set; }  //名稱
        public string? FSpecification   { get; set; }  = null; //規格or票種等等
        public int? FQuantity { get; set; }
        public decimal? FPrice { get; set; }
        public string? FSingleImage { get; set; }
        public int? FSellerId { get; set; } //賣家ID
        public string? FSellerName { get; set; } //賣家名稱
        public int? FProductStock { get; set; } //商品庫存
    }
}
