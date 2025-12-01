namespace MarketStat.Services.Storage.Settings;

public class StorageSettings
{
    public Uri? ServiceUrl { get; set; }

    public string BucketName { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool ForcePathStyle { get; set; }
}
