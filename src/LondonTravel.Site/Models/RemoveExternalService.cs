// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models;

public class RemoveExternalService
{
    public string LoginProvider { get; set; } = string.Empty;

    public string ProviderKey { get; set; } = string.Empty;
}
