using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Services.Auth.AuthService.Validators;

public static class UserValidator
{
    private const int MaxUsernameLength = 100;
    private const int MaxEmailLength = 255;
    private const int MaxFullNameLength = 255;
    private const int MinPasswordLength = 8;

    public static void ValidateRegistration(RegisterUserDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "Registration data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required.", nameof(dto.Username));
        }
        if (dto.Username.Length > MaxUsernameLength)
        {
            throw new ArgumentException($"Username cannot exceed {MaxUsernameLength} characters.", nameof(dto.Username));
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.", nameof(dto.Password));
        }
        if (dto.Password.Length < MinPasswordLength)
        {
            throw new ArgumentException($"Password must be at least {MinPasswordLength} characters long.", nameof(dto.Password));
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email is required.", nameof(dto.Email));
        }
        if (dto.Email.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(dto.Email));
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(dto.Email);
            if (addr.Address != dto.Email.Trim())
            {
                 throw new ArgumentException("Email format is invalid (extra spaces detected).", nameof(dto.Email));
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email format is invalid.", nameof(dto.Email));
        }


        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(dto.FullName));
        }
        if (dto.FullName.Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.", nameof(dto.FullName));
        }
    }

    public static void ValidateLogin(LoginRequestDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "Login data cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required.", nameof(dto.Username));
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.", nameof(dto.Password));
        }
    }
}