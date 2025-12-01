using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using MarketStat.Services.Storage.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketStat.Services.Storage;

public sealed class S3ReportStorageService : IReportStorageService, IDisposable
{
    private readonly AmazonS3Client _s3Client;
    private readonly StorageSettings _settings;
    private readonly ILogger<S3ReportStorageService> _logger;
    private bool _disposed;

    public S3ReportStorageService(IOptions<StorageSettings> settings, ILogger<S3ReportStorageService> logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);
        _settings = settings.Value;
        _logger = logger;

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl?.ToString(),
            ForcePathStyle = _settings.ForcePathStyle,
        };
        var credentials = new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
        _s3Client = new AmazonS3Client(credentials, config);
    }

    public async Task<string> UploadReportAsync(string fileName, byte[] content, string contentType)
    {
        _logger.LogInformation("Uploading report '{FileName}' to bucket '{BucketName}' at {ServiceUrl}", fileName, _settings.BucketName, _settings.ServiceUrl?.ToString() ?? "AWS Global");
        try
        {
            using var stream = new MemoryStream(content);
            var putRequest = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = fileName,
                InputStream = stream,
                ContentType = contentType,
                AutoCloseStream = false,
            };
            var response = await _s3Client.PutObjectAsync(putRequest).ConfigureAwait(false);
            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"S3 returned status code {response.HttpStatusCode}");
            }

            var baseUrl = _settings.ServiceUrl != null
                ? _settings.ServiceUrl.ToString()
                : $"https://{_settings.BucketName}.s3.amazonaws.com";
            var cleanBase = baseUrl.TrimEnd('/');
            var url = $"{cleanBase}/{_settings.BucketName}/{fileName}";
            _logger.LogInformation("Successfully uploaded report. URL: {Url}", url);
            return url;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 Error during upload: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error uploading report");
            throw new InvalidOperationException("Failed to upload report to storage provider.", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _s3Client?.Dispose();
        _disposed = true;
    }
}
