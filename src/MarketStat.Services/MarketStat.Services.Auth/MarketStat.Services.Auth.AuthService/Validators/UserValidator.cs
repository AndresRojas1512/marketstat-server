using System.Net.Mail;
using MarketStat.Common.Dto.MarketStat.Common.Dto.Account.User;

namespace MarketStat.Services.Auth.AuthService.Validators;

public static class UserValidator
{
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 100;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;
    private const int MaxEmailLength = 255;
    private const int MaxFullNameLength = 255;

    // private static readonly Regex PasswordPolicyRegex = new Regex(
    //    @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
    //    RegexOptions.Compiled);

    public static void ValidateRegistration(RegisterUserDto dto)
    {
        if (dto == null)
        {
            throw new ArgumentNullException(nameof(dto), "Registration data (DTO) cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required.", nameof(dto.Username));
        }
        if (dto.Username.Length < MinUsernameLength || dto.Username.Length > MaxUsernameLength)
        {
            throw new ArgumentException($"Username must be between {MinUsernameLength} and {MaxUsernameLength} characters.", nameof(dto.Username));
        }
        // if (!Regex.IsMatch(dto.Username, @"^[a-zA-Z0-9_.-]+$"))
        // {
        //    throw new ArgumentException("Username contains invalid characters.", nameof(dto.Username));
        // }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.", nameof(dto.Password));
        }
        if (dto.Password.Length < MinPasswordLength || dto.Password.Length > MaxPasswordLength)
        {
            throw new ArgumentException($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.", nameof(dto.Password));
        }
        // if (!PasswordPolicyRegex.IsMatch(dto.Password))
        // {
        //     throw new ArgumentException(
        //         "Password does not meet complexity requirements. " +
        //         "It must contain at least one uppercase letter, one lowercase letter, one digit, " +
        //         "one special character, and be at least 8 characters long.", 
        //         nameof(dto.Password));
        // }

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
            var mailAddress = new MailAddress(dto.Email);
            if (mailAddress.Address != dto.Email.Trim()) 
            {
                 throw new ArgumentException("Email format is invalid (e.g. due to leading/trailing spaces not intended).", nameof(dto.Email));
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email format is invalid.", nameof(dto.Email), ex);
        }
        catch (ArgumentException ex)
        {
             throw new ArgumentException("Email format is invalid.", nameof(dto.Email), ex);
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
            throw new ArgumentNullException(nameof(dto), "Login data (DTO) cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(dto.Username))
        {
            throw new ArgumentException("Username is required for login.", nameof(dto.Username));
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required for login.", nameof(dto.Password));
        }
    }
}