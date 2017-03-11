// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// A class representing the container for site resource strings.
    /// </summary>
    public class SiteResources
    {
        private readonly IStringLocalizer<SiteResources> _localizer;

        public SiteResources(IStringLocalizer<SiteResources> localizer)
        {
            _localizer = localizer;
        }

        public string HomepageLead()
        {
            return _localizer["An Amazon Alexa skill for checking the status of travel in London."];
        }
    }
}
