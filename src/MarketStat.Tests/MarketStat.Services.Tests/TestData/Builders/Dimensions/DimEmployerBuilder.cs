using MarketStat.Common.Core.MarketStat.Common.Core.Dimensions;

namespace MarketStat.Services.Tests.TestData.Builders;

public class DimEmployerBuilder
{
    private int _id = 0;
    private string _name = "OOO TestDefault";
    private string _inn = "7700000000";
    private string _ogrn = "1027700000000";
    private string _kpp = "770001001";
    private DateOnly _registrationDate = new(2020, 1, 1);
    private string _legalAddress = "Moscow, street. Testing, 1";
    private string _contactEmail = "hr@test.com";
    private string _contactPhone = "+7 111 111-11-67";
    private int _industryId = 1;

    public DimEmployerBuilder WithId(int id)
    {
        _id = id;
        return this;
    }

    public DimEmployerBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DimEmployerBuilder WithInn(string inn)
    {
        _inn = inn;
        return this;
    }

    public DimEmployerBuilder WithOgrn(string ogrn)
    {
        _ogrn = ogrn;
        return this;
    }

    public DimEmployerBuilder WithKpp(string kpp)
    {
        _kpp = kpp;
        return this;
    }
    
    public DimEmployerBuilder WithRegistrationDate(DateOnly registrationDate)
    {
        _registrationDate = registrationDate;
        return this;
    }
    
    public DimEmployerBuilder WithLegalAddress(string legalAddress)
    {
        _legalAddress = legalAddress;
        return this;
    }
    
    public DimEmployerBuilder WithContactEmail(string contactEmail)
    {
        _contactEmail = contactEmail;
        return this;
    }
    
    public DimEmployerBuilder WithContactPhone(string contactPhone)
    {
        _contactPhone = contactPhone;
        return this;
    }
    
    public DimEmployerBuilder WithIndustryFieldId(int id)
    {
        _industryId = id;
        return this;
    }

    public DimEmployer Build()
    {
        return new DimEmployer
        {
            EmployerId = _id,
            EmployerName = _name,
            Inn = _inn,
            Ogrn = _ogrn,
            Kpp = _kpp,
            RegistrationDate = _registrationDate,
            LegalAddress = _legalAddress,
            ContactEmail = _contactEmail,
            ContactPhone = _contactPhone,
            IndustryFieldId = _industryId
        };
    }
}