namespace MarketStat.Contracts.Facts;

public interface IFactSalarySubmittedEvent
{
    Guid SubmissionId { get; }
    bool Success { get; }
    string Message { get; }
}