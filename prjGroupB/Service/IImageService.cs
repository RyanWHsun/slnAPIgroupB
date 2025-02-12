using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public interface IImageService
{
    Task<string> SaveImage(IFormFile image);
}
