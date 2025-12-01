namespace MarketStat.Services.Dimensions.DimDateService.Validators;

public static class DimDateValidator
{
    public static void ValidateForCreate(DateOnly fullDate)
    {
        if (fullDate == default)
        {
            throw new ArgumentException("FullDate is required.");
        }

        if (fullDate.Year < 1900 || fullDate.Year > DateTime.Now.Year + 1)
        {
            throw new ArgumentException($"Year {fullDate.Year} is out of range.");
        }
    }

    public static void ValidateForUpdate(int dateId, DateOnly fullDate)
    {
        if (dateId <= 0)
        {
            throw new ArgumentException("DateId must be a positive integer.");
        }

        ValidateForCreate(fullDate);
    }
}
