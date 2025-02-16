namespace prjGroupB.DTO
{
    public class TOrderBuyerAllDTO
    {
        public int FOrderId { get; set; }
        public int? FOrderStatusId { get; set; }
        public string FStatusName { get; set; }
        public string FShipAddress { get; set; }
        public DateTime? FOrderDate { get; set; }
        public int FOrderAmount { get; set; }
        public string SellerName { get; set; }
        public required List<string> FProductName { get; set; }
    }
}
