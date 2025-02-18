namespace prjGroupB.DTO
{
    public class TUserInfoForOrderDTO
    {
        public int FUserId { get; set; }

        public required string FUserName { get; set; }
        public string FUserPhone { get; set; }

        public string FUserEmail { get; set; }

        public string FUserAddress { get; set; }
        public int? TotalBalance { get; set; } //會員錢包剩餘金額
    }
}

