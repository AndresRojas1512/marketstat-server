namespace MarketStat.Services.Dimensions.DimDateService.Validators;

public class DimDateValidator
{
    public static void ValidateCreateParameters(DateOnly fullDate)
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

    public static void ValidateParameters(int dateId, DateOnly fullDate)
    {
        if (dateId <= 0)
        {
            throw new ArgumentException("DateId must be a positive integer.");
        }
        ValidateCreateParameters(fullDate);
    }
}