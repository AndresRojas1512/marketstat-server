using MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.Dimensions;

public static class DimEmployerConverter
{
    public static DimEmployerDbModel ToDbModel(DimEmployer domainEmployer)
    {
        ArgumentNullException.ThrowIfNull(domainEmployer);

        return new DimEmployerDbModel
        {
            EmployerId = domainEmployer.EmployerId,
            EmployerName = domainEmployer.EmployerName,
            Inn = domainEmployer.Inn,
            Ogrn = domainEmployer.Ogrn,
            Kpp = domainEmployer.Kpp,
            RegistrationDate = domainEmployer.RegistrationDate,
            LegalAddress = domainEmployer.LegalAddress,
            ContactEmail = domainEmployer.ContactEmail,
            ContactPhone = domainEmployer.ContactPhone,
            IndustryFieldId = domainEmployer.IndustryFieldId,
        };
    }

    public static DimEmployer ToDomain(DimEmployerDbModel dbEmployer)
    {
        ArgumentNullException.ThrowIfNull(dbEmployer);

        return new DimEmployer
        {
            EmployerId = dbEmployer.EmployerId,
            EmployerName = dbEmployer.EmployerName,
            Inn = dbEmployer.Inn,
            Ogrn = dbEmployer.Ogrn,
            Kpp = dbEmployer.Kpp,
            RegistrationDate = dbEmployer.RegistrationDate,
            LegalAddress = dbEmployer.LegalAddress,
            ContactEmail = dbEmployer.ContactEmail,
            ContactPhone = dbEmployer.ContactPhone,
            IndustryFieldId = dbEmployer.IndustryFieldId,
        };
    }
}
