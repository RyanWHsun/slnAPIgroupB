using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;

    public ImageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            throw new ArgumentNullException(nameof(image), "上傳的圖片為空！");
        }

        var uploadsFolder = Path.Combine(_env.WebRootPath ?? throw new ArgumentNullException("_env.WebRootPath 為 null"), "uploads");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await image.CopyToAsync(stream);
        }

        return $"/uploads/{fileName}"; // 確保返回的 URL 正確
    }
}