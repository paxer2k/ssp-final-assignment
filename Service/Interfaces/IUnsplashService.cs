using Unsplasharp.Models;

namespace Service.Interfaces
{
    public interface IUnsplashService
    {
        Task<Photo> FetchPhotoByIdAsync(string id);
    }
}
