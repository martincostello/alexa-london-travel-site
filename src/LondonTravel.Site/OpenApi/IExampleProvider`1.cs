// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.OpenApi;

#pragma warning disable CA1000

public interface IExampleProvider<in T>
    where T : IExampleProvider<T>
{
    static abstract object GenerateExample();
}
