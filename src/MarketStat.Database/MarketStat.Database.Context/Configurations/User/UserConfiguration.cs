using MarketStat.Database.Models.Account;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarketStat.Database.Context.Configurations.User;

public class UserConfiguration : IEntityTypeConfiguration<UserDbModel>
{
    public void Configure(EntityTypeBuilder<UserDbModel> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("users");
        builder.HasKey(u => u.UserId);
        builder.Property(u => u.UserId).HasColumnName("user_id").UseIdentityByDefaultColumn();

        builder.Property(u => u.Username).HasColumnName("username").HasMaxLength(100).IsRequired();
        builder.HasIndex(u => u.Username).IsUnique().HasDatabaseName("uq_users_username");

        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasColumnType("text").IsRequired();

        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("uq_users_email");

        builder.Property(u => u.FullName).HasColumnName("full_name").HasMaxLength(255).IsRequired();
        builder.Property(u => u.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at").IsRequired(false);
        builder.Property(u => u.IsAdmin).HasColumnName("is_admin").IsRequired().HasDefaultValue(false);
    }
}
