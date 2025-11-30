namespace MarketStat.Services.Storage;

public interface IReportStorageService
{
    Task<string> UploadReportAsync(string fileName, byte[] content, string contentType);
}