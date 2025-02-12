using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IImageService
{
    Task<byte[]> SaveImage(IFormFile image);
}
