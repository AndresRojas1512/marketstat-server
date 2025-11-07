using MarketStat.Common.Core.MarketStat.Common.Core.Account;
using MarketStat.Tests.TestData.Builders.Account;

namespace MarketStat.Tests.TestData.ObjectMothers.Account;

public static class UserObjectMother
{
    public static User ANewUser() =>
        new UserBuilder()
            .WithId(0)
            .WithUsername("newuser")
            .WithEmail("new@example.com")
            .Build();
    
    public static User AnExistingUser() =>
        new UserBuilder()
            .WithId(1)
            .WithUsername("existinguser")
            .WithEmail("existing@example.com")
            .Build();
    
    public static User ASecondExistingUser() =>
        new UserBuilder()
            .WithId(2)
            .WithUsername("anotheruser")
            .WithEmail("another@example.com")
            .Build();
    
    public static IEnumerable<User> SomeUsers()
    {
        return new List<User>
        {
            AnExistingUser(),
            ASecondExistingUser()
        };
    }
}