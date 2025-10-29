using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;
using MarketStat.Database.Models;

namespace MarketStat.Common.Converter.MarketStat.Common.Converter.Dimensions;

public static class DimEmployerConverter
{
    public static DimEmployerDbModel ToDbModel(DimEmployer domainEmployer)
    {
        if (domainEmployer == null)
            throw new ArgumentNullException(nameof(domainEmployer));

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
            ContactPhone = domainEmployer.ContactPhone
        };
    }

    public static DimEmployer ToDomain(DimEmployerDbModel dbEmployer)
    {
        if (dbEmployer == null)
            throw new ArgumentNullException(nameof(dbEmployer));
        
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
            ContactPhone = dbEmployer.ContactPhone
        };
    }
}