namespace MarketStat.Services.Auth.AuthService.Validators;

using System.Net.Mail;
using MarketStat.Common.Dto.Account.User;

public static class UserValidator
{
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 100;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;
    private const int MaxEmailLength = 255;
    private const int MaxFullNameLength = 255;

    public static void ValidateRegistration(RegisterUserDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required.", nameof(dto));
        }

        if (dto.Username.Length < MinUsernameLength || dto.Username.Length > MaxUsernameLength)
        {
            throw new ArgumentException($"Username must be between {MinUsernameLength} and {MaxUsernameLength} characters.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.", nameof(dto));
        }

        if (dto.Password.Length < MinPasswordLength || dto.Password.Length > MaxPasswordLength)
        {
            throw new ArgumentException($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email is required.", nameof(dto));
        }

        if (dto.Email.Length > MaxEmailLength)
        {
            throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(dto));
        }

        try
        {
            var mailAddress = new MailAddress(dto.Email);
            if (mailAddress.Address != dto.Email.Trim())
            {
                throw new ArgumentException("Email format is invalid (e.g. due to leading/trailing spaces not intended).", nameof(dto));
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email format is invalid.", nameof(dto), ex);
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException("Email format is invalid.", nameof(dto), ex);
        }

        if (string.IsNullOrWhiteSpace(dto.FullName))
        {
            throw new ArgumentException("Full name is required.", nameof(dto));
        }

        if (dto.FullName.Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.", nameof(dto));
        }
    }

    public static void ValidateLogin(LoginRequestDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required for login.", nameof(dto));
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required for login.", nameof(dto));
        }
    }

    public static void ValidateProfileUpdate(string? fullName, string? email)
    {
        if (fullName != null)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));
            }

            if (fullName.Length > MaxFullNameLength)
            {
                throw new ArgumentException(
                    $"Full name cannot exceed {MaxFullNameLength} characters.",
                    nameof(fullName));
            }
        }

        if (email != null)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email is required.", nameof(email));
            }

            if (email.Length > MaxEmailLength)
            {
                throw new ArgumentException($"Email cannot exceed {MaxEmailLength} characters.", nameof(email));
            }

            try
            {
                var mailAddress = new MailAddress(email);
                if (mailAddress.Address != email.Trim())
                {
                    throw new ArgumentException("Email format is invalid.", nameof(email));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Email format is invalid.", nameof(email), ex);
            }
        }
    }
}
