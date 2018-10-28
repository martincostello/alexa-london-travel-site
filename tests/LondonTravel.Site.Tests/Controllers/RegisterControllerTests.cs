// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
namespace MartinCostello.LondonTravel.Site.Controllers
{
    using System;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using MartinCostello.LondonTravel.Site.Models;
    using MartinCostello.LondonTravel.Site.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Shouldly;
    using Xunit;

    /// <summary>
    /// A class containing tests for the <see cref="RegisterController"/> class. This class cannot be inherited.
    /// </summary>
    public static class RegisterControllerTests
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

            using (RegisterController target = CreateTarget(service: mock.Object))
            {
                // Act
                var actual = await target.Index();

                // Assert
                actual.ShouldNotBeNull();

                var view = actual.ShouldBeOfType<ViewResult>();
                var model = view.Model.ShouldBeOfType<RegisterViewModel>();

                model.ShouldNotBeNull();
                model.RegisteredUsers.ShouldBe(expected);
            }
        }

        [Fact]
        public static async Task Register_Returns_Correct_Fallback_Value()
        {
            // Arrange
            var mock = new Mock<IAccountService>();

            mock.Setup((p) => p.GetUserCountAsync(true))
                .ThrowsAsync(new InvalidOperationException());

            using (RegisterController target = CreateTarget(service: mock.Object))
            {
                // Act
                var actual = await target.Index();

                // Assert
                actual.ShouldNotBeNull();

                var view = actual.ShouldBeOfType<ViewResult>();
                var model = view.Model.ShouldBeOfType<RegisterViewModel>();

                model.ShouldNotBeNull();
                model.RegisteredUsers.ShouldBe(7000);
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="RegisterController"/> using mock dependencies.
        /// </summary>
        /// <param name="service">An optional instance of <see cref="IAccountService"/>.</param>
        /// <returns>
        /// The created instance of <see cref="RegisterController"/>.
        /// </returns>
        private static RegisterController CreateTarget(IAccountService service = null)
        {
            var httpContext = new Mock<HttpContext>();

            httpContext
                .Setup((p) => p.User)
                .Returns(new ClaimsPrincipal(new ClaimsIdentity()));

            var actionContext = new ActionContext()
            {
                ActionDescriptor = new ControllerActionDescriptor(),
                HttpContext = httpContext.Object,
                RouteData = new RouteData(),
            };

            var controllerContext = new ControllerContext(actionContext);

            return new RegisterController(service, Mock.Of<ILogger<RegisterController>>())
            {
                ControllerContext = controllerContext,
            };
        }
    }
}
