using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

public class ImageService : IImageService
{
    public async Task<byte[]> SaveImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentNullException(nameof(image), "上傳的圖片為空！");
        }

        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);

        return memoryStream.ToArray(); // ✅ 直接回傳 byte[]，存入資料庫
    }
}
