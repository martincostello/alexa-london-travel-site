// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using MartinCostello.LondonTravel.Site.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace MartinCostello.LondonTravel.Site.Pages.Account;

public static class RegisterPageTests
{
    [Theory]
    [InlineData(5000, 4000)]
    [InlineData(5001, 5000)]
    [InlineData(5999, 5000)]
    [InlineData(6000, 5000)]
    [InlineData(6001, 6000)]
    public static async Task Register_Rounds_User_Count_Correctly(long userCount, long expected)
    {
        // Arrange
        var service = Substitute.For<IAccountService>();

        service.GetUserCountAsync(true)
               .Returns(Task.FromResult(userCount));

        Register page = CreatePage(service: service);

        // Act
        await page.OnGet();

        // Assert
        page.RegisteredUsers.ShouldBe(expected);
    }

    [Fact]
    public static async Task Register_Returns_Correct_Fallback_Value()
    {
        // Arrange
        var service = Substitute.For<IAccountService>();

        service.When((p) => p.GetUserCountAsync(true))
               .Throw(new InvalidOperationException());

        Register page = CreatePage(service: service);

        // Act
        await page.OnGet();

        // Assert
        page.RegisteredUsers.ShouldBe(9_750);
    }

    private static Register CreatePage(IAccountService? service = null)
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()),
        };

        var pageContext = new PageContext()
        {
            ActionDescriptor = new CompiledPageActionDescriptor(),
            HttpContext = httpContext,
            RouteData = new RouteData(),
        };

        return new(service ?? Substitute.For<IAccountService>())
        {
            PageContext = pageContext,
        };
    }
}
