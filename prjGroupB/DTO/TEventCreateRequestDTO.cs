public class TEventCreateRequestDTO
{
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public IFormFile? Image { get; set; } // 允許 null
}