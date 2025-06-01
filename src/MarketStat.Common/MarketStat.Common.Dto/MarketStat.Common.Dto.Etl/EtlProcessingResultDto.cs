namespace MarketStat.Common.Dto.MarketStat.Common.Dto.Etl;

public class EtlProcessingResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CsvRowsRead { get; set; }
    public int RowsStaged { get; set; }
    public int FactsInserted { get; set; }
    public int RowsSkippedOrFailedInProcedure { get; set; }

    public List<string> Errors { get; set; } = new List<string>();

    public EtlProcessingResultDto(bool success, string message)
    {
        Success = success;
        Message = message;
    }
}