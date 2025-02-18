using System.ComponentModel.DataAnnotations;

public class TEventCreateRequestDTO
{
    [Required]
    public string Name { get; set; }

    public string Description { get; set; }

    [Required]
    public DateTime? StartDate { get; set; }

    [Required]
    public DateTime? EndDate { get; set; }

    public IFormFile Image { get; set; } // ✅ 確保這個欄位可以接收圖片
}
