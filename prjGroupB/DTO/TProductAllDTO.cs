using prjGroupB.Models;

namespace prjGroupB.DTO
{
    public class TProductAllDTO 
    {
        public int FProductId { get; set; }


        public int? FProductCategoryId { get; set; }

        public string FProductName { get; set; }
        public decimal? FProductPrice { get; set; }
        public bool? FIsOnSales { get; set; }
        public int? FStock { get; set; }
        
        public int? FUserId { get; set; }
        public string? FUserNickName { get; set; }
        public string? FUserImage { get; set; } // Base64 縮圖

        public string? FImage { get; set; } // Base64 縮圖

    }
}