namespace MarketStat.Contracts.Sales;

public interface ISalarySubmittedEvent
{
    Guid SubmissionId { get; }
    bool Success { get; }
    string Message { get; }
}