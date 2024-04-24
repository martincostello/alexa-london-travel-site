// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.Playwright;

namespace MartinCostello.LondonTravel.Site;

public sealed class BrowsersTestData : TheoryData<string, string?>
{
    public BrowsersTestData()
    {
        Add(BrowserType.Chromium, null);
        Add(BrowserType.Chromium, "chrome");

        // HACK Skip on macOS. See https://github.com/microsoft/playwright-dotnet/issues/2920.
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Add(BrowserType.Chromium, "msedge");
        }

        Add(BrowserType.Firefox, null);

        if (OperatingSystem.IsMacOS())
        {
            Add(BrowserType.Webkit, null);
        }
    }
}
