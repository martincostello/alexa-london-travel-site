// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// A class representing the default implementation of <see cref="ITflServiceFactory"/>. This class cannot be inherited.
/// </summary>
public sealed class TflServiceFactory : ITflServiceFactory
{
    /// <summary>
    /// The <see cref="IServiceProvider"/> to use. This field is read-only.
    /// </summary>
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TflServiceFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use.</param>
    public TflServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public ITflService CreateService() => _serviceProvider.GetRequiredService<ITflService>();
}
