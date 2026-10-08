using System.ComponentModel.DataAnnotations;
using Project.Models;
using Xunit;

namespace Project.Tests;

public class UserPasswordStorageTests
{
    [Fact]
    public void PasswordColumnLimit_AccommodatesBcryptHashWithoutChangingRequestRules()
    {
        var maxLength = typeof(User)
            .GetProperty(nameof(User.Password))!
            .GetCustomAttributes(typeof(MaxLengthAttribute), inherit: true)
            .Cast<MaxLengthAttribute>()
            .Single();

        var hash = BCrypt.Net.BCrypt.HashPassword("valid raw password");

        Assert.True(maxLength.Length >= hash.Length);
        Assert.Equal(60, hash.Length);
    }
}
