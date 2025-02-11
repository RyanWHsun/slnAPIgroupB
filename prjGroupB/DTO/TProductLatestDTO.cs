namespace prjGroupB.DTO
{
    public class TProductLatestDTO
    {
        public int FProductId { get; set; }
        public required string FProductName { get; set; }
        public DateTime? FProductDateAdd { get; set; }
        public string? FSingleImage { get; set; } // Base64 縮圖,單張圖片
    }
}
