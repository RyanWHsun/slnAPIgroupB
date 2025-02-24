namespace prjGroupB.DTO
{
    public class TChatsDTO
    {
        public int FChatId { get; set; }

        public int? FSenderId { get; set; }

        public int? FReceiverId { get; set; }

        public string? FMessageText { get; set; }

        public DateTime? FSentAt { get; set; }
    }
}
