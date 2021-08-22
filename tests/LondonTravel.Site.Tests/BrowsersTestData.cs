// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections;

namespace MartinCostello.LondonTravel.Site;

public sealed class BrowsersTestData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { "chromium" };
        yield return new object[] { "chromium:chrome" };

        if (!OperatingSystem.IsLinux())
        {
            yield return new object[] { "chromium:msedge" };
        }

        yield return new object[] { "firefox" };

        if (OperatingSystem.IsMacOS())
        {
            yield return new object[] { "webkit" };
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
