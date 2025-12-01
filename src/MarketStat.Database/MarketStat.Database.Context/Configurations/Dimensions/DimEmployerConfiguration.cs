using MarketStat.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.Dimensions;

public class DimEmployerConfiguration : IEntityTypeConfiguration<DimEmployerDbModel>
{
    public void Configure(EntityTypeBuilder<DimEmployerDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("dim_employer");
        builder.HasKey(e => e.EmployerId);
        builder.Property(e => e.EmployerId).HasColumnName("employer_id").UseIdentityByDefaultColumn();

        builder.Property(e => e.EmployerName).HasColumnName("employer_name").HasMaxLength(255).IsRequired();
        builder.HasIndex(e => e.EmployerName).IsUnique().HasDatabaseName("uq_dim_employer_name");

        builder.Property(e => e.Inn).HasColumnName("inn").HasMaxLength(12).IsRequired();
        builder.HasIndex(e => e.Inn).IsUnique().HasDatabaseName("uq_dim_employer_inn");

        builder.Property(e => e.Ogrn).HasColumnName("ogrn").HasMaxLength(13).IsRequired();
        builder.HasIndex(e => e.Ogrn).IsUnique().HasDatabaseName("uq_dim_employer_ogrn");

        builder.Property(e => e.Kpp).HasColumnName("kpp").HasMaxLength(9).IsRequired();
        builder.Property(e => e.RegistrationDate).HasColumnName("registration_date").HasColumnType("date").IsRequired();
        builder.Property(e => e.LegalAddress).HasColumnName("legal_address").HasColumnType("text").IsRequired();
        builder.Property(e => e.ContactEmail).HasColumnName("contact_email").HasMaxLength(255).IsRequired();
        builder.Property(e => e.ContactPhone).HasColumnName("contact_phone").HasMaxLength(50).IsRequired();
        builder.Property(e => e.IndustryFieldId).HasColumnName("industry_field_id").IsRequired();

        builder.HasOne(e => e.DimIndustryField)
            .WithMany(i => i.DimEmployers)
            .HasForeignKey(e => e.IndustryFieldId)
            .HasConstraintName("fk_dim_employer_industry")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
