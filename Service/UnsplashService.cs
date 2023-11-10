using Domain.Configuration.Interfaces;
using Service.Interfaces;
using Unsplasharp;
using Unsplasharp.Models;

namespace Service
{
    public class UnsplashService : IUnsplashService
    {
        private readonly IAppConfiguration _appConfiguration;
        private readonly UnsplasharpClient _unsplashClient;

        public UnsplashService(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
            _unsplashClient = new UnsplasharpClient(_appConfiguration.UnsplashConfig.AccessKey);
        }
        public async Task<Photo> FetchPhotoByIdAsync(string id)
        {
            return await _unsplashClient.GetPhoto(id);
        }
    }
}
