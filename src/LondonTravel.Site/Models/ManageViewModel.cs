// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site.Models
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http.Authentication;
    using Microsoft.AspNetCore.Identity;

    public class ManageViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }

        public IList<AuthenticationDescription> OtherLogins { get; set; }

        public bool CanAddMoreLogins => OtherLogins?.Count > 0;

        public string ETag { get; set; }

        public bool IsLinkedToAlexa { get; set; }

        public bool ShowRemoveButton => CurrentLogins?.Count > 1;
    }
}
