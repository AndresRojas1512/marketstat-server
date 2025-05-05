namespace MarketStat.Services.Dimensions.DimHierarchyLevelService.Validators;

public static class DimHierarchyLevelValidator
{
    public static void ValidateForCreate(string hierarchyLevelName)
    {
        if (string.IsNullOrWhiteSpace(hierarchyLevelName))
            throw new ArgumentException("HierarchyLevel name is required.");
        if (hierarchyLevelName.Length > 255)
            throw new ArgumentException("HierarchyLevel name must be 255 characters or fewer.");
    }
    
    public static void ValidateForUpdate(int hierarchyLevelId, string hierarchyLevelName)
    {
        if (hierarchyLevelId <= 0)
            throw new ArgumentException("Invalid hierarchy level id.");
        ValidateForCreate(hierarchyLevelName);
    }
}