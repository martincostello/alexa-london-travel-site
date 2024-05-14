// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Services.Data;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Identity;

public static class UserStoreTests
{
    [Fact]
    public static async Task Methods_Validate_Parameters()
    {
        // Arrange
        var user = new LondonTravelUser();
        var login = new UserLoginInfo("loginProvider", "providerKey", "displayName");
        var cancellationToken = CancellationToken.None;

        using var target = CreateStore();

        // Act and Assert
#nullable disable
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.AddLoginAsync(null, login, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("login", () => target.AddLoginAsync(user, null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.CreateAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.DeleteAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("normalizedEmail", () => target.FindByEmailAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("userId", () => target.FindByIdAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("loginProvider", () => target.FindByLoginAsync(null, "b", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("providerKey", () => target.FindByLoginAsync("a", null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("normalizedUserName", () => target.FindByNameAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetEmailAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetEmailConfirmedAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetLoginsAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetNormalizedEmailAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetNormalizedUserNameAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetRolesAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetSecurityStampAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetUserIdAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetUserNameAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.RemoveLoginAsync(null, "a", "b", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("loginProvider", () => target.RemoveLoginAsync(user, null, "b", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("providerKey", () => target.RemoveLoginAsync(user, "a", null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetEmailAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetEmailConfirmedAsync(null, true, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetNormalizedEmailAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetNormalizedUserNameAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetSecurityStampAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetUserNameAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.SetEmailAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.UpdateAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.GetRolesAsync(null, cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("user", () => target.IsInRoleAsync(null, "a", cancellationToken));
        await Assert.ThrowsAsync<ArgumentNullException>("roleName", () => target.IsInRoleAsync(user, null, cancellationToken));
#nullable enable
    }

    [Fact]
    public static async Task Methods_That_Are_Not_Implemented_Throw()
    {
        // Arrange
        var user = new LondonTravelUser();
        string roleName = "RoleName";
        var cancellationToken = CancellationToken.None;

        using var target = CreateStore();

        // Act and Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => target.AddToRoleAsync(user, roleName, cancellationToken));
        await Assert.ThrowsAsync<NotImplementedException>(() => target.GetUsersInRoleAsync(roleName, cancellationToken));
        await Assert.ThrowsAsync<NotImplementedException>(() => target.RemoveFromRoleAsync(user, roleName, cancellationToken));
    }

    [Fact]
    public static async Task AddLoginAsync_Throws_If_Login_Exists_For_Provider()
    {
        // Arrange
        var user = new LondonTravelUser();
        user.Logins.Add(new LondonTravelLoginInfo() { LoginProvider = "acme" });

        var login = new UserLoginInfo("acme", "providerKey", "displayName");

        using var target = CreateStore();

        // Act and Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => target.AddLoginAsync(user, login, CancellationToken.None));
    }

    [Fact]
    public static async Task AddLoginAsync_Adds_Login()
    {
        // Arrange
        var user = new LondonTravelUser();
        var login = new UserLoginInfo("acme", "providerKey", "displayName");

        using var target = CreateStore();

        // Act
        await target.AddLoginAsync(user, login, CancellationToken.None);

        // Assert
        user.Logins.ShouldNotBeNull();
        user.Logins.ShouldNotBeEmpty();
        user.Logins.Count.ShouldBe(1);

        var added = user.Logins[0];

        added.ShouldNotBeNull();
        added.LoginProvider.ShouldBe(login.LoginProvider);
        added.ProviderDisplayName.ShouldBe(login.ProviderDisplayName);
        added.ProviderKey.ShouldBe(login.ProviderKey);
    }

    [Fact]
    public static async Task CreateAsync_Creates_User()
    {
        // Arrange
        var user = new LondonTravelUser();

        var service = Substitute.For<IDocumentService>();

        service.CreateAsync(user)
               .Returns(Task.FromResult("MyUserId"));

        using var target = CreateStore(service);

        // Act
        var actual = await target.CreateAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe(IdentityResult.Success);

        user.Id.ShouldNotBeNullOrEmpty();
        user.Id.ShouldBe("MyUserId");
    }

    [Fact]
    public static async Task DeleteAsync_If_User_Exists()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            Id = "MyUserId",
        };

        var service = Substitute.For<IDocumentService>();

        service.DeleteAsync(user.Id)
               .Returns(Task.FromResult(true));

        using var target = CreateStore(service);

        // Act
        var actual = await target.DeleteAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe(IdentityResult.Success);
    }

    [Fact]
    public static async Task DeleteAsync_If_User_Does_Not_Exist()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            Id = "MyUserId",
        };

        var service = Substitute.For<IDocumentService>();

        service.DeleteAsync(user.Id)
               .Returns(Task.FromResult(false));

        using var target = CreateStore(service);

        // Act
        var actual = await target.DeleteAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldNotBe(IdentityResult.Success);
        actual.Errors.ShouldNotBeEmpty();
        actual.Errors.First().Code.ShouldBe("UserNotFound");
    }

    [Fact]
    public static async Task DeleteAsync_Throws_If_No_User_Id()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            Id = string.Empty,
        };

        using var target = CreateStore();

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>("user", () => target.DeleteAsync(user, CancellationToken.None));
    }

    [Fact]
    public static async Task GetEmailAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            Email = "user@domain.com",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetEmailAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("user@domain.com");
    }

    [Fact]
    public static async Task GetEmailConfirmedAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            EmailConfirmed = true,
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetEmailConfirmedAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public static async Task GetNormalizedEmailAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            EmailNormalized = "user@domain.com",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetNormalizedEmailAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("user@domain.com");
    }

    [Fact]
    public static async Task GetNormalizedUserNameAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            UserNameNormalized = "user.name",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetNormalizedUserNameAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("user.name");
    }

    [Fact]
    public static async Task GetSecurityStampAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            SecurityStamp = "MySecurityStamp",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetSecurityStampAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("MySecurityStamp");
    }

    [Fact]
    public static async Task GetUserIdAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            Id = "123",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetUserIdAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("123");
    }

    [Fact]
    public static async Task GetUserNameAsync_Returns_Correct_Value()
    {
        // Arrange
        var user = new LondonTravelUser()
        {
            UserName = "user.name",
        };

        using var target = CreateStore();

        // Act
        string actual = await target.GetUserNameAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldBe("user.name");
    }

    [Fact]
    public static async Task SetEmailAsync_Sets_Correct_Property()
    {
        // Arrange
        string expected = "user@domain.com";
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetEmailAsync(user, expected, CancellationToken.None);

        // Assert
        user.Email.ShouldBe(expected);
    }

    [Fact]
    public static async Task SetEmailConfirmedAsync_Sets_Correct_Property()
    {
        // Arrange
        bool expected = true;
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetEmailConfirmedAsync(user, expected, CancellationToken.None);

        // Assert
        user.EmailConfirmed.ShouldBe(expected);
    }

    [Fact]
    public static async Task SetNormalizedEmailAsync_Sets_Correct_Property()
    {
        // Arrange
        string expected = "user@domain.com";
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetNormalizedEmailAsync(user, expected, CancellationToken.None);

        // Assert
        user.EmailNormalized.ShouldBe(expected);
    }

    [Fact]
    public static async Task SetNormalizedUserNameAsync_Sets_Correct_Property()
    {
        // Arrange
        string expected = "user.name";
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetNormalizedUserNameAsync(user, expected, CancellationToken.None);

        // Assert
        user.UserNameNormalized.ShouldBe(expected);
    }

    [Fact]
    public static async Task SetSecurityStampAsync_Sets_Correct_Property()
    {
        // Arrange
        string expected = "MySecurityStamp";
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetSecurityStampAsync(user, expected, CancellationToken.None);

        // Assert
        user.SecurityStamp.ShouldBe(expected);
    }

    [Fact]
    public static async Task SetUserNameAsync_Sets_Correct_Property()
    {
        // Arrange
        string expected = "user.name";
        var user = new LondonTravelUser();

        using var target = CreateStore();

        // Act
        await target.SetUserNameAsync(user, expected, CancellationToken.None);

        // Assert
        user.UserName.ShouldBe(expected);
    }

    [Fact]
    public static async Task GetRolesAsync_Returns_Expected_Names()
    {
        // Arrange
        var user = new LondonTravelUser();

        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "admin", ClaimValueTypes.String, "https://londontravel.martincostello.com/")));
        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "not-a-string", ClaimValueTypes.Email, "https://londontravel.martincostello.com/")));
        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "wrong-issuer", ClaimValueTypes.String, "google")));
        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, string.Empty, ClaimValueTypes.String, "https://londontravel.martincostello.com/")));

        var expected = new[]
        {
            "admin",
        };

        using var target = CreateStore();

        // Act
        var actual = await target.GetRolesAsync(user, CancellationToken.None);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldBe(expected);
    }

    [Fact]
    public static async Task IsInRoleAsync_Returns_Expected_Value()
    {
        // Arrange
        var user = new LondonTravelUser();
        var cancellationToken = CancellationToken.None;

        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "admin", ClaimValueTypes.String, "https://londontravel.martincostello.com/")));
        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "not-a-string", ClaimValueTypes.Email, "https://londontravel.martincostello.com/")));
        user.RoleClaims.Add(LondonTravelRole.FromClaim(new Claim(ClaimTypes.Role, "wrong-issuer", ClaimValueTypes.String, "google")));

        using var target = CreateStore();

        // Act and Assert
        (await target.IsInRoleAsync(user, "admin", cancellationToken)).ShouldBeTrue();
        (await target.IsInRoleAsync(user, "not-a-role", cancellationToken)).ShouldBeFalse();
        (await target.IsInRoleAsync(user, "not-a-string", cancellationToken)).ShouldBeFalse();
        (await target.IsInRoleAsync(user, "wrong-issuer", cancellationToken)).ShouldBeFalse();
        (await target.IsInRoleAsync(user, string.Empty, cancellationToken)).ShouldBeFalse();

        // Arrange
        user.RoleClaims = null!;

        // Act and Assert
        (await target.IsInRoleAsync(user, "admin", cancellationToken)).ShouldBeFalse();
    }

    private static UserStore CreateStore(IDocumentService? service = null)
        => new(service ?? Substitute.For<IDocumentService>());
}
