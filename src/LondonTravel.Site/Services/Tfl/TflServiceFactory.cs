// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// A class representing the default implementation of <see cref="ITflServiceFactory"/>. This class cannot be inherited.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="TflServiceFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
public sealed class TflServiceFactory(IServiceProvider serviceProvider) : ITflServiceFactory
{
    /// <inheritdoc />
    public ITflService CreateService() => serviceProvider.GetRequiredService<ITflService>();
}
