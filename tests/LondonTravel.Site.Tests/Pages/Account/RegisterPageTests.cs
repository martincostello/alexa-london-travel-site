// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public static class RegisterPageTests
{
    [Theory]
    [InlineData(5000, 4000)]
    [InlineData(5001, 5000)]
    [InlineData(5999, 5000)]
    [InlineData(6000, 5000)]
    [InlineData(6001, 6000)]
    public static async Task Register_Rounds_User_Count_Correctly(int userCount, long expected)
    {
        // Arrange
        var mock = new Mock<IAccountService>();

        mock.Setup((p) => p.GetUserCountAsync(true))
            .ReturnsAsync(userCount);

        Register page = CreatePage(service: mock.Object);

        // Act
        await page.OnGet();

        // Assert
        page.RegisteredUsers.ShouldBe(expected);
    }

    [Fact]
    public static async Task Register_Returns_Correct_Fallback_Value()
    {
        // Arrange
        var mock = new Mock<IAccountService>();

        mock.Setup((p) => p.GetUserCountAsync(true))
            .ThrowsAsync(new InvalidOperationException());

        Register page = CreatePage(service: mock.Object);

        // Act
        await page.OnGet();

        // Assert
        page.RegisteredUsers.ShouldBe(9_500);
    }

    private static Register CreatePage(IAccountService? service = null)
    {
        var httpContext = new Mock<HttpContext>();

        httpContext
            .Setup((p) => p.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity()));

        var pageContext = new PageContext()
        {
            ActionDescriptor = new CompiledPageActionDescriptor(),
            HttpContext = httpContext.Object,
            RouteData = new RouteData(),
        };

        return new(service ?? Mock.Of<IAccountService>())
        {
            PageContext = pageContext,
        };
    }
}
