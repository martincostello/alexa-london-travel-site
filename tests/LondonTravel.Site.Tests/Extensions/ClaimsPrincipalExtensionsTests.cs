// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;

namespace MartinCostello.LondonTravel.Site.Extensions;

/// <summary>
/// A class containing unit tests for the <see cref="ClaimsPrincipalExtensions"/> class.
/// </summary>
public static class ClaimsPrincipalExtensionsTests
{
    private const string FallbackImageUrl = "https://www.mypicture.com/Hello World";

    [Fact]
    public static void GetAvatarUrl_Throws_If_Required_Parameters_Are_Null()
    {
        Assert.Throws<ArgumentNullException>(() => ClaimsPrincipalExtensions.GetAvatarUrl(null!, string.Empty)).ParamName.ShouldBe("value");
        Assert.Throws<ArgumentNullException>(() => ClaimsPrincipalExtensions.GetAvatarUrl(new ClaimsPrincipal(), null!)).ParamName.ShouldBe("fallbackImageUrl");
    }

    [Fact]
    public static void GetAvatarUrl_Returns_Correct_Url_If_No_Email_Claim()
    {
        // Arrange
        var principal = new ClaimsPrincipal();

        // Act
        string actual = principal.GetAvatarUrl(FallbackImageUrl);

        // Assert
        actual.ShouldBe(FallbackImageUrl);
    }

    [Theory]
    [InlineData("", FallbackImageUrl)]
    [InlineData("MyEmailAddress@example.com ", "https://www.gravatar.com/avatar/0bc83cb571cd1c50ba6f3e8a78ef1346?s=24&d=https%3A%2F%2Fwww.mypicture.com%2FHello%20World")]
    public static void GetAvatarUrl_Returns_Correct_Url(string email, string expected)
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Email, email)]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        string actual = principal.GetAvatarUrl(FallbackImageUrl);

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public static void GetAvatarUrl_Returns_Correct_Url_With_Explicit_Size()
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Email, "MyEmailAddress@example.com")]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        string actual = principal.GetAvatarUrl(FallbackImageUrl, 42);

        // Assert
        actual.ShouldBe("https://www.gravatar.com/avatar/0bc83cb571cd1c50ba6f3e8a78ef1346?s=42&d=https%3A%2F%2Fwww.mypicture.com%2FHello%20World");
    }

    [Fact]
    public static void GetDisplayName_Throws_If_Value_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => ClaimsPrincipalExtensions.GetDisplayName(null!)).ParamName.ShouldBe("value");
    }

    [Fact]
    public static void GetDisplayName_Returns_Correct_Value_If_No_GivenName_Or_Name()
    {
        // Arrange
        var principal = new ClaimsPrincipal();

        // Act
        string actual = principal.GetDisplayName();

        // Assert
        actual.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData("martin costello", "martin")]
    [InlineData("m costello", "m")]
    public static void GetDisplayName_Returns_Correct_Value_If_No_GivenName_But_Name(string name, string expected)
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, name)]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        string actual = principal.GetDisplayName();

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("  martin costello  ", "martin")]
    [InlineData("  m  costello  ", "m")]
    public static void GetDisplayName_Returns_Correct_Value_If_Blank_GivenName_But_Name(string name, string expected)
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, name), new Claim(ClaimTypes.GivenName, string.Empty)]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        string actual = principal.GetDisplayName();

        // Assert
        actual.ShouldBe(expected);
    }

    [Theory]
    [InlineData("martin ", "martin")]
    [InlineData(" Martin", "Martin")]
    public static void GetDisplayName_Returns_Correct_Value_If_GivenName(string givenName, string expected)
    {
        // Arrange
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.GivenName, givenName)]);
        var principal = new ClaimsPrincipal(identity);

        // Act
        string actual = principal.GetDisplayName();

        // Assert
        actual.ShouldBe(expected);
    }
}
