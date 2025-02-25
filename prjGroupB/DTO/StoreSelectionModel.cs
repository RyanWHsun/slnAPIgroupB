namespace prjGroupB.DTO
{
    public class StoreSelectionModel
    {
        public string StoreID { get; set; } // 是否為離島店鋪
        public string StoreName { get; set; } // 額外資訊
    }

    public class ECPaySettings
    {
        public string MerchantID { get; set; }
        public string HashKey { get; set; }
        public string HashIV { get; set; }
        public string ServerReplyURL { get; set; }
    }

}
