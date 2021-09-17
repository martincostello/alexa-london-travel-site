// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class containing the names of the routes for the site. This class cannot be inherited.
/// </summary>
public static class SiteRoutes
{
    public const string Account = nameof(Account);

    public const string AuthorizeAlexa = nameof(AuthorizeAlexa);

    public const string DeleteAccount = nameof(DeleteAccount);

    public const string ExternalSignIn = nameof(ExternalSignIn);

    public const string ExternalSignInCallback = nameof(ExternalSignInCallback);

    public const string Help = "/Help/Index";

    public const string Home = "/Home/Index";

    public const string LinkAccount = nameof(LinkAccount);

    public const string LinkAccountCallback = nameof(LinkAccountCallback);

    public const string Manage = nameof(Manage);

    public const string PrivacyPolicy = "/PrivacyPolicy/Index";

    public const string Register = "/Account/Register";

    public const string RemoveAccountLink = nameof(RemoveAccountLink);

    public const string RemoveAlexaLink = nameof(RemoveAlexaLink);

    public const string SignIn = nameof(SignIn);

    public const string SignOut = nameof(SignOut);

    public const string Technology = "/Technology/Index";

    public const string TermsOfService = "/Terms/Index";

    public const string UpdateLinePreferences = nameof(UpdateLinePreferences);
}
