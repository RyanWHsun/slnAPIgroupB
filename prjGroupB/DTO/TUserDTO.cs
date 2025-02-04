namespace prjGroupB.DTO
{
    public class TUserDTO
    {
        public int? FUserId { get; set; }

        public int FUserRankId { get; set; }

        public string FUserName { get; set; }

        public byte[]? FUserImage { get; set; }

        public string FUserNickName { get; set; }

        public string? FUserSex { get; set; }

        public DateTime? FUserBirthday { get; set; }

        public string? FUserPhone { get; set; }

        public string FUserEmail { get; set; }
        public string? FUserAddress { get; set; }

        public DateTime FUserComeDate { get; set; }

        public string FUserPassword { get; set; }

    }
}
