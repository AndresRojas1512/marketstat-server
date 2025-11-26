using System.Net.Mail;

namespace MarketStat.Common.Validators.Auth;

public static class UserValidator
{
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 100;
    private const int MinPasswordLength = 8;
    private const int MaxPasswordLength = 100;
    private const int MaxEmailLength = 255;
    private const int MaxFullNameLength = 255;

    public static void ValidateRegistration(string username, string password, string email, string fullName)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required.", nameof(username));
        }
        if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
        {
            throw new ArgumentException($"Username must be between {MinUsernameLength} and {MaxUsernameLength} characters.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }
        if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
        {
            throw new ArgumentException($"Password must be between {MinPasswordLength} and {MaxPasswordLength} characters.", nameof(password));
        }

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
                 throw new ArgumentException("Email format is invalid (e.g. due to leading/trailing spaces not intended).", nameof(email));
            }
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Email format is invalid.", nameof(email), ex);
        }
        catch (ArgumentException ex)
        {
             throw new ArgumentException("Email format is invalid.", nameof(email), ex);
        }


        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException("Full name is required.", nameof(fullName));
        }
        if (fullName.Length > MaxFullNameLength)
        {
            throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.", nameof(fullName));
        }
    }

    public static void ValidateLogin(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required for login.", nameof(username));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required for login.", nameof(password));
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
                throw new ArgumentException($"Full name cannot exceed {MaxFullNameLength} characters.",
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