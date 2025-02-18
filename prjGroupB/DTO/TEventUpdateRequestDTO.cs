public class TEventUpdateRequestDTO
{
    public string? Name { get; set; }  // 允許更新部分欄位
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IFormFile? Image { get; set; } // 可選的圖片
}