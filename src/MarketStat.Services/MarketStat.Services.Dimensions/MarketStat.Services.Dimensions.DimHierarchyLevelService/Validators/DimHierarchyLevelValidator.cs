namespace MarketStat.Services.Dimensions.DimHierarchyLevelService.Validators;

public static class DimHierarchyLevelValidator
{
    private const int MaxCodeLength = 10;
    private const int MaxNameLength = 255;

    public static void ValidateForCreate(string hierarchyLevelCode, string hierarchyLevelName)
    {
        if (string.IsNullOrWhiteSpace(hierarchyLevelCode))
            throw new ArgumentException("HierarchyLevelCode is required.", nameof(hierarchyLevelCode));
        if (hierarchyLevelCode.Length > MaxCodeLength)
            throw new ArgumentException($"HierarchyLevelCode must be {MaxCodeLength} characters or fewer.", nameof(hierarchyLevelCode));

        if (string.IsNullOrWhiteSpace(hierarchyLevelName))
            throw new ArgumentException("HierarchyLevel name is required.", nameof(hierarchyLevelName));
        if (hierarchyLevelName.Length > MaxNameLength)
            throw new ArgumentException($"HierarchyLevel name must be {MaxNameLength} characters or fewer.", nameof(hierarchyLevelName));
    }
    
    public static void ValidateForUpdate(int hierarchyLevelId, string hierarchyLevelCode, string hierarchyLevelName)
    {
        if (hierarchyLevelId <= 0)
            throw new ArgumentException("Invalid hierarchy level id.", nameof(hierarchyLevelId));
            
        ValidateForCreate(hierarchyLevelCode, hierarchyLevelName);
    }
}