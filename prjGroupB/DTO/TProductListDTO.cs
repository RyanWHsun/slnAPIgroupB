using System.ComponentModel.DataAnnotations;

namespace prjGroupB.DTO
{
    public class TProductListDTO
    {
        public int FProductId { get; set; }
        public int? FProductCategoryId { get; set; }
        public required string FProductName { get; set; }
        public decimal? FProductPrice { get; set; }
        public bool? FIsOnSales { get; set; }
        public int? FStock { get; set; }
        public DateTime? FProductDateAdd { get; set; }
        public DateTime? FProductUpdated { get; set; }
        public string? FSingleImage { get; set; } // Base64 縮圖,單張圖片
    }
}
