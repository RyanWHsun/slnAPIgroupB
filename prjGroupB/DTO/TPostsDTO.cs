namespace prjGroupB.DTO
{
    public class TPostsDTO
    {
        public int FPostId { get; set; }

        public int? FUserId { get; set; }

        public string? FTitle { get; set; }

        public string? FContent { get; set; }

        public DateTime? FCreatedAt { get; set; }

        public DateTime? FUpdatedAt { get; set; }

        public bool? FIsPublic { get; set; }

        public int? FCategoryId { get; set; }
    }
}