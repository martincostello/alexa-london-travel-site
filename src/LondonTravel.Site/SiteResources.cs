// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.LondonTravel.Site
{
    using Microsoft.AspNetCore.Mvc.Localization;
    using Microsoft.Extensions.Localization;

    /// <summary>
    /// A class representing the container for site resource strings.
    /// </summary>
    public class SiteResources
    {
        private readonly IHtmlLocalizer<SiteResources> _htmlLocalizer;
        private readonly IStringLocalizer<SiteResources> _localizer;

        public SiteResources(IHtmlLocalizer<SiteResources> htmlLocalizer, IStringLocalizer<SiteResources> localizer)
        {
            _htmlLocalizer = htmlLocalizer;
            _localizer = localizer;
        }

        public string AccountCreatedTitle => _localizer["Your London Travel account has been created."];

        public string AccountDeletedTitle => _localizer["Your London Travel account has been deleted."];

        public string AccountDeletedContent => _localizer["Sorry to see you go."];

        public string AccessDeniedTitle => _localizer["Access denied"];

        public string AccessDeniedSubtitle => _localizer["You do not have access to this resource."];

        public string AccountLockedTitle => _localizer["Account locked"];

        public string AccountLockedSubtitle => _localizer["This account has been locked out. Please try again later."];

        public string BrandName => _localizer["London Travel"];

        public string CancelButtonText => _localizer["Cancel"];

        public string CancelButtonAltText => CancelButtonText;

        public string CloseButtonText => _localizer["Close"];

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

        public string HelpTitle => _localizer["Help and Support"];

        public string HelpMetaDescription => _localizer["Help and support for the London Travel Alexa skill."];

        public string HomepageTitle => _localizer["Home"];

        public string HomepageLead => _localizer["An Amazon Alexa skill for checking the status of travel in London."];

        public string InstallLinkText => _localizer["Install"];

        public string InstallLinkAltText => _localizer["Install London Travel for Amazon Alexa from amazon.co.uk"];

        public string ManageTitle => _localizer["Manage your account"];

        public string ManageMetaDescription => _localizer["Manage your account details for the London Travel skill."];

        public string ManageLinkedAccountsSubtitle => _localizer["Linked Accounts"];

        public string ManageLinkedAccountsContent => _localizer["Listed below are the accounts you have linked to your London Travel account for signing in with."];

        public string ManageLinkOtherAccountsSubtitle => _localizer["Link another account"];

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

        public string TermsOfServiceTitle => _localizer["London Travel Terms of Service"];

        public string TermsOfServiceMetaDescription => _localizer["The Terms of Service for the London Travel Alexa skill and website."];

        public string TermsOfServiceMetaTitle => _localizer["Terms of Service"];

        public string TermsOfServiceLinkText => _localizer["Terms of Service"];

        public string TermsOfServiceLinkAltText => _localizer["View the London Travel Alexa skill's Terms of Service"];

        public string RemoveAccountButtonAltText(string provider) => _localizer["Remove link to {0} from your account", provider];
    }
}
