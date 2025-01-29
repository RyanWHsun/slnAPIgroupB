namespace prjGroupB.DTO
{
    public class TProductDetailDTO
    {
        public int FProductId { get; set; }
        public int? FProductCategoryId { get; set; }
        public string FProductName { get; set; }
        public decimal? FProductPrice { get; set; }
        public string? FProductDescription { get; set; }
        public bool? FIsOnSales { get; set; }
        public int? FStock { get; set; }
        public DateTime? FProductDateAdd { get; set; }
        public DateTime? FProductUpdated { get; set; }
        public string[] FImage { get; set; } = Array.Empty<string>();  // 若有圖片但無資料，可以返回空陣列；若無圖片，則返回 null。

        //public int? FUserId { get; set; }
        //public string? FUserNickName { get; set; }
        //public string? FUserImage { get; set; } // Base64 縮圖

    }
}
