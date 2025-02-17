namespace prjGroupB.DTO
{
    public class TPostCommentsDTO
    {
        public int FCommentId { get; set; }

        public int? FPostId { get; set; }

        public string FUserName { get; set; }
        public string FUserImage { get; set; }
        public string FUserNickName { get; set; }

        public string FContent { get; set; }

        public DateTime? FCreatedAt { get; set; }

        public DateTime? FUpdatedAt { get; set; }

        public int? FParentCommentId { get; set; }
    }
}
