using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Domain.Configuration.Interfaces;
using Service.Interfaces.Azure;

namespace Service.Azure
{
    public class BlobService : IBlobService
    {
        private IAppConfiguration _appConfiguration;
        private BlobServiceClient _blobServiceClient;
        public BlobService(IAppConfiguration appConfiguration)
        {
            _appConfiguration = appConfiguration;
            _blobServiceClient = new BlobServiceClient(_appConfiguration.BlobConfig.BlobConnectionString);
        }
        public async Task UploadAsync(string containerName, byte[] fileInBytes, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            var contentType = new BlobHttpHeaders { ContentType = "image/jpeg" };

            await using var memoryStream = new MemoryStream(fileInBytes);

            await blobClient.UploadAsync(memoryStream, contentType);
        }

        public async Task DeleteAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.DeleteAsync();
        }

        public async Task<byte[]> DownloadAsync(string containerName, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            using var memoryStream = new MemoryStream();

            await blobClient.DownloadToAsync(memoryStream);

            return memoryStream.ToArray();
        }

        public string GenerateSasLink(string containerName, string fileName)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            BlobSasBuilder sasBuilder = new BlobSasBuilder
            {
                BlobContainerName = containerName,
                BlobName = fileName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
                Protocol = SasProtocol.None
            };

            sasBuilder.SetPermissions(BlobAccountSasPermissions.Read);

            Uri sasUri = blobClient.GenerateSasUri(sasBuilder);

            return sasUri.ToString();
        }
    }
}
