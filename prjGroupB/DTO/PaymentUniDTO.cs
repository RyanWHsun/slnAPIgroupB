namespace prjGroupB.DTO
{
    public class PaymentUniDTO
    {

    }

    public class UniPaySettings
    {
        public string MerID { get; set; }
        public string HashKey { get; set; }
        public string HashIV { get; set; }
        public string ServerReplyURL { get; set; }
    }

    public class MapStoreInfo
    {
        public string storeType { get; set; }
        public string storeID { get; set; }
        public string storeName { get; set; }
        public string address { get; set; }
    }


}
