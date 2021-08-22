// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Services.Tfl;

/// <summary>
/// Defines a method for creating an instance of <see cref="ITflService"/>.
/// </summary>
public interface ITflServiceFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ITflService"/>.
    /// </summary>
    /// <returns>
    /// The created instance of <see cref="ITflService"/>.
    /// </returns>
    ITflService CreateService();
}
