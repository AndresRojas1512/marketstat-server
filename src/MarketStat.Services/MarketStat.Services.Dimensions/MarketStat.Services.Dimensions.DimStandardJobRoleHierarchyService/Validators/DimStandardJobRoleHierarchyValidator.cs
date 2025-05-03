namespace MarketStat.Services.Dimensions.DimStandardJobRoleHierarchyService.Validators;

public static class DimStandardJobRoleHierarchyValidator
{
    public static void ValidateParameters(int standardJobRoleId, int hierarchyLevelId)
    {
        if (standardJobRoleId <= 0)
            throw new ArgumentException("StandardJobRoleId must be grater than 0.");
        if (hierarchyLevelId <= 0)
            throw new ArgumentException("HierarchyLevelId must be grater than 0.");
    }
}