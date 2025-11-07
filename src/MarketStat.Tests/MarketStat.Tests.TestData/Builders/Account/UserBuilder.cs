using MarketStat.Common.Core.MarketStat.Common.Core.Account;

namespace MarketStat.Tests.TestData.Builders.Account;

public class UserBuilder
{
    private int _userId = 0;
    private string _username = "testuser";
    private string _passwordHash = "hashed_password";
    private string _email = "test@example.com";
    private string _fullName = "Test User";
    private bool _isActive = true;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _lastLoginAt = null;
    private bool _isAdmin = false;

    public UserBuilder WithId(int id)
    {
        _userId = id;
        return this;
    }

    public UserBuilder WithUsername(string username)
    {
        _username = username;
        return this;
    }
    
    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithFullName(string fullName)
    {
        _fullName = fullName;
        return this;
    }
    
    public UserBuilder WithPasswordHash(string hash)
    {
        _passwordHash = hash;
        return this;
    }

    public UserBuilder IsAdmin(bool isAdmin = true)
    {
        _isAdmin = isAdmin;
        return this;
    }
    
    public UserBuilder IsActive(bool isActive)
    {
        _isActive = isActive;
        return this;
    }

    public UserBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }
    
    public UserBuilder WithLastLoginAt(DateTimeOffset? lastLoginAt)
    {
        _lastLoginAt = lastLoginAt;
        return this;
    }

    public User Build()
    {
        return new User(
            _userId,
            _username,
            _passwordHash,
            _email,
            _fullName,
            _isActive,
            _createdAt,
            _lastLoginAt,
            _isAdmin
        );
    }
}