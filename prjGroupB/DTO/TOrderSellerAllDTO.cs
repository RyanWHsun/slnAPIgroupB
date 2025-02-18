namespace prjGroupB.DTO
{
    public class TOrderSellerAllDTO
    {
        public int FOrderId { get; set; }
        public int? FOrderStatusId { get; set; }
        public string FStatusName { get; set; }
        public string FShipAddress { get; set; }
        public DateTime? FOrderDate { get; set; }
        public int FOrderAmount { get; set; }
        public string BuyerName { get; set; }
        public string FExtraInfo { get; set; }
        public List<TOrderStatusHistoryDTO> StatusHistory { get; set; } // 訂單的狀態變更歷程
    }

    public class ShipOrderDTO
    {
        public string? extraInfo { get; set; } // 物流資訊 (如物流單號、快遞公司)
    }
}
