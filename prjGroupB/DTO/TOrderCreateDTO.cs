namespace prjGroupB.DTO
{
    public class TOrderCreateDTO
    {
        public class CheckoutRequestDTO
        {
            public UserInfoDTO UserInfo { get; set; }
            public List<ItemsForOrderDTO> SelectedItems { get; set; }
            public string FPaymentMethod { get; set; }
        }

        public class UserInfoDTO
        {
            public int FUserId { get; set; }
            public string FUserName { get; set; }
            public string FUserPhone { get; set; }
            public string FUserAddress { get; set; }
            public int TotalBalance { get; set; }
            public string FUserEmail { get; set; }
        }

        public class ItemsForOrderDTO
        {
            public int FCartItemId { get; set; }
            public required string FItemType { get; set; }  // product, attractionTicket, eventFee
            public int FItemId { get; set; }
            public int FQuantity { get; set; }
            public int FSellerId { get; set; }
        }

    }
}
