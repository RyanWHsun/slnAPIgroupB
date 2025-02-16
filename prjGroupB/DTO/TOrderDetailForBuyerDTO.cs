using prjGroupB.Models;

namespace prjGroupB.DTO
{
    public class TOrderDetailForBuyerDTO
    {
        public List<TOrderDetailDTO> OrderDetails { get; set; } // 訂單內的商品
        public List<TOrderStatusHistoryDTO> StatusHistory { get; set; } // 訂單的狀態變更歷程
    }
    public class TOrderDetailDTO
    {
        public int FOrderDetailsId { get; set; } // 訂單明細 ID
        public int FItemId { get; set; } // 商品 ID
        public int FOrderQty { get; set; } // 訂購數量
        public decimal FUnitPrice { get; set; } // 單價
        public string FProductName { get; set; } // 商品名稱
        public string FProductImage { get; set; } // 商品圖片 (Base64 或 URL)
    }

    public class TOrderStatusHistoryDTO
    {
        public int FOrderStatusId { get; set; } // 訂單狀態 ID
        public string FStatusName { get; set; } // 狀態名稱
        public DateTime? FTimestamp { get; set; } // 變更時間
    }

}
