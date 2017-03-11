// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using System;
    using MartinCostello.LondonTravel.Site.Options;
    using Microsoft.AspNetCore.Mvc.Localization;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// A class representing the container for site resource strings.
    /// </summary>
    public class SiteResources
    {
        private readonly IHtmlLocalizer<SiteResources> _htmlLocalizer;
        private readonly IStringLocalizer<SiteResources> _localizer;
        private readonly SiteOptions _options;

        public SiteResources(
            IHtmlLocalizer<SiteResources> htmlLocalizer,
            IStringLocalizer<SiteResources> localizer,
            SiteOptions options)
        {
            _htmlLocalizer = htmlLocalizer;
            _localizer = localizer;
            _options = options;
        }

        public string AccountCreatedTitle => _localizer["Your London Travel account has been created."];

        public string AccountDeletedTitle => _localizer["Your London Travel account has been deleted."];

        public string AccountDeletedContent => _localizer["Sorry to see you go."];

        public string AccessDeniedTitle => _localizer["Access denied"];

        public string AccessDeniedSubtitle => _localizer["You do not have access to this resource."];

        public string AccountLinkDeniedTitle => _localizer["Account link permission denied"];

        public string AccountLinkDeniedContent => _localizer["You did not grant permission for London Travel to access your other account."];

        public string AccountLinkFailedTitle => _localizer["Account link failed"];

        public string AccountLinkFailedContent => _localizer["Something went wrong when attempting to link your other account."];

        public string AccountLockedTitle => _localizer["Account locked"];

        public string AccountLockedSubtitle => _localizer["This account has been locked out. Please try again later."];

        public string AlreadyRegisteredTitle => _localizer["Already registered"];

        public string AlreadyRegisteredContent1 => _localizer["You have already created a London Travel account by signing in with a different service to the one you just tried to sign in with."];

        public string AlreadyRegisteredContent2 => _localizer["Sign in with one of the service(s) you've already used to sign-in, then you'll be able to link other service(s) to your account."];

        public string BrandName => _localizer["London Travel"];

        public string CancelButtonText => _localizer["Cancel"];

        public string CancelButtonAltText => CancelButtonText;

        public string CloseButtonText => _localizer["Close"];

        public LocalizedHtmlString CopyrightText => _htmlLocalizer["&copy; {0} {1}", _options.Metadata.Author.Name, DateTimeOffset.UtcNow.Year];

        public string DeleteAccountButtonText => _localizer["Delete your account"];

        public string DeleteAccountButtonAltText => _localizer["Permanently delete your London Travel account"];

        public string DeleteAccountConfirmationButtonText => _localizer["Delete"];

        public string DeleteAccountConfirmationButtonAltText => _localizer["Permanently delete your account"];

        public string DeleteAccountModalTitle => _localizer["Delete your account"];

        public string DeleteAccountWarning => _localizer["Warning!"];

        public LocalizedHtmlString DeleteAccountParagraph1Html => _htmlLocalizer["Clicking the <strong>Delete</strong> button will <strong><em>permanently</em></strong> delete your London Travel account and any data associated with it."];

        public string DeleteAccountParagraph2 => _localizer["If you have linked the London Travel Alexa skill to this account you will no longer be able to ask Alexa about your commute using the skill unless you create a new account and re-link the skill using an Alexa-enabled device."];

        public LocalizedHtmlString DeleteAccountParagraph3Html => _htmlLocalizer["Are you sure you want to delete your account? This action <strong>cannot</strong> be undone."];

        public string DeleteInProgressText => _localizer["Your account is being deleted..."];

        public string ErrorTitle => _localizer["Error"];

        public string ErrorMessage => _localizer["Sorry, something went wrong."];

        public string HelpTitle => _localizer["Help and Support"];

        public string HelpMetaDescription => _localizer["Help and support for the London Travel Alexa skill."];

        public string HelpLinkText => _localizer["Help & Support"];

        public string HelpLinkAltText => _localizer["View help and support information for the London Travel Alexa skill and this website"];

        public string HomepageTitle => _localizer["Home"];

        public string HomepageLinkText => HomepageTitle;

        public string HomepageLinkAltText => _localizer["View the homepage"];

        public string HomepageLead => _localizer["An Amazon Alexa skill for checking the status of travel in London."];

        public string InstallLinkText => _localizer["Install"];

        public string InstallLinkAltText => _localizer["Install London Travel for Amazon Alexa from amazon.co.uk"];

        public string ManageTitle => _localizer["Manage your account"];

        public string ManageLinkText => _localizer["Manage account"];

        public string ManageLinkAltText => _localizer["Manage your account"];

        public string ManageMetaDescription => _localizer["Manage your account details for the London Travel skill."];

        public string ManageLinkedAccountsSubtitle => _localizer["Linked Accounts"];

        public string ManageLinkedAccountsContent => _localizer["Listed below are the accounts you have linked to your London Travel account for signing in with."];

        public string ManageLinkOtherAccountsSubtitle => _localizer["Link another account"];

        public string NavbarCollapseAltText => _localizer["Toggle navigation"];

        public string NavbarMenuText => _localizer["Menu"];

        public string PermissionDeniedTitle => _localizer["Permission denied"];

        public string PermissionDeniedContent => _localizer["You did not grant permission for London Travel to access your email address when you signed in. An email address is required to register."];

        public string PrivacyPolicyTitle => _localizer["London Travel Privacy Policy"];

        public string PrivacyPolicyMetaTitle => _localizer["Privacy Policy"];

        public string PrivacyPolicyMetaDescription => _localizer["Privacy policy for user data with the London Travel Alexa skill."];

        public string PrivacyPolicyLinkText => _localizer["Privacy Policy"];

        public string PrivacyPolicyLinkAltText => _localizer["View the London Travel Alexa skill's Privacy Policy"];

        public string RegisterTitle => _localizer["Register"];

        public string RegisterSubtitle => _localizer["Register for an account"];

        public string RegisterMetaDescription => _localizer["Register for a London Travel account."];

        public string RegisterLead => _localizer["Registering for an account for the London Travel skill allows you to save your tube line and station preferences so that you can ask about your commute."];

        public string RegisterParagraph1 => _localizer["If you do not already have a London Travel account, you can create one by signing in with one of the services below."];

        public string RegisterParagraph2 => _localizer["The London Travel account created will use with email address associated with the service you sign in with."];

        public string RegisterParagraph3 => _localizer["Accounts and their data are managed by this service in line with the Privacy Policy and Terms of Service."];

        public string RegisterLinkText => _localizer["Register"];

        public string RegisterLinkAltText => _localizer["Register for a London Travel account"];

        public string RegisterQuote => _localizer["\"Alexa, ask London Travel what my commute's like today?\""];

        public string RegisterSignInSubtitle => _localizer["Choose a service to register using:"];

        public string RemoveAccountButtonText => _localizer["Remove"];

        public string RemoveAccountLinkModalTitle => _localizer["Removing account link..."];

        public string RemoveAccountLinkModalDescription => _localizer["Please wait while your account link is removed."];

        public string SignInTitle => _localizer["Sign in"];

        public string SignInMetaDescription => _localizer["Sign in to your London Travel account."];

        public string SignInSubtitle => _localizer["Choose a service to sign in with:"];

        public string SignInRegisterSubtitle => _localizer["...or register a new account"];

        public string SignInRegisterText => _localizer["If you do not already have a London Travel account, register for one by clicking the button below."];

        public string SignInLinkText => _localizer["Sign In"];

        public string SignInLinkAltText => _localizer["Sign in to your London Travel account"];

        public string SignInErrorTitle => _localizer["Sign-in error"];

        public string SignInErrorSubtitle => _localizer["Sorry, you couldn't be signed in. Please try again later."];

        public string SigningInModalTitle => _localizer["Signing in..."];

        public string SigningInModalDescription => _localizer["Please wait while you are redirected to sign-in."];

        public string SignOutLinkText => _localizer["Sign out"];

        public string StatusLinkText => _localizer["Site Status & Uptime"];

        public string StatusLinkAltText => _localizer["View site uptime information"];

        public string TermsOfServiceTitle => _localizer["London Travel Terms of Service"];

        public string TermsOfServiceMetaDescription => _localizer["The Terms of Service for the London Travel Alexa skill and website."];

        public string TermsOfServiceMetaTitle => _localizer["Terms of Service"];

        public string TermsOfServiceLinkText => _localizer["Terms of Service"];

        public string TermsOfServiceLinkAltText => _localizer["View the London Travel Alexa skill's Terms of Service"];

        public string ErrorSubtitle(int? httpCode) => _localizer["Error (HTTP {0})", httpCode ?? 500];

        public string RemoveAccountButtonAltText(string provider) => _localizer["Remove link to {0} from your account", provider];

        public string SignInButtonText(string diplayName) => _localizer["Sign in with {0}", diplayName];

        public string SignInButtonAltText(string diplayName) => _localizer["Sign in using your {0} account", diplayName];
    }
}
