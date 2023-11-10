namespace Service.Interfaces.Azure
{
    public interface IBlobService
    {
        Task UploadAsync(string containerName, byte[] fileInBytes, string fileName);
        Task DeleteAsync(string containerName, string fileName);
        Task<byte[]> DownloadAsync(string containerName, string fileName);
        string GenerateSasLink(string containerName, string fileName);
    }
}
