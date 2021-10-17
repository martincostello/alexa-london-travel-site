// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections;
using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site;

public sealed class BrowsersTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { BrowserType.Chromium };
        yield return new object[] { $"{BrowserType.Chromium}:chrome" };

        if (!OperatingSystem.IsLinux())
        {
            yield return new object[] { $"{BrowserType.Chromium}:msedge" };
        }

        yield return new object[] { BrowserType.Firefox };

        if (OperatingSystem.IsMacOS())
        {
            yield return new object[] { BrowserType.Webkit };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
