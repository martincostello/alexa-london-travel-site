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

        public string AccountCreatedTitle => _localizer[nameof(AccountCreatedTitle)];

        public string AccountDeletedTitle => _localizer[nameof(AccountDeletedTitle)];

        public string AccountDeletedContent => _localizer[nameof(AccountDeletedContent)];

        public string AccessDeniedTitle => _localizer[nameof(AccessDeniedTitle)];

        public string AccessDeniedSubtitle => _localizer[nameof(AccessDeniedSubtitle)];

        public string AccountLinkDeniedTitle => _localizer[nameof(AccountLinkDeniedTitle)];

        public string AccountLinkDeniedContent => _localizer[nameof(AccountLinkDeniedContent)];

        public string AccountLinkFailedTitle => _localizer[nameof(AccountLinkFailedTitle)];

        public string AccountLinkFailedContent => _localizer[nameof(AccountLinkFailedContent)];

        public string AccountLockedTitle => _localizer[nameof(AccountLockedTitle)];

        public string AccountLockedSubtitle => _localizer[nameof(AccountLockedSubtitle)];

        public string AlreadyRegisteredTitle => _localizer[nameof(AlreadyRegisteredTitle)];

        public string AlreadyRegisteredContent1 => _localizer[nameof(AlreadyRegisteredContent1)];

        public string AlreadyRegisteredContent2 => _localizer[nameof(AlreadyRegisteredContent2)];

        public string BrandName => _localizer[nameof(BrandName)];

        public string CancelButtonText => _localizer[nameof(CancelButtonText)];

        public string CancelButtonAltText => _localizer[nameof(CancelButtonAltText)];

        public string CloseButtonText => _localizer[nameof(CloseButtonText)];

        public string CommuteSkillInvocation => _localizer[nameof(CommuteSkillInvocation)];

        public LocalizedHtmlString CopyrightText => _htmlLocalizer[nameof(CopyrightText), _options.Metadata.Author.Name, DateTimeOffset.UtcNow.Year];

        public string DeleteAccountButtonText => _localizer[nameof(DeleteAccountButtonText)];

        public string DeleteAccountButtonAltText => _localizer[nameof(DeleteAccountButtonAltText)];

        public string DeleteAccountConfirmationButtonText => _localizer[nameof(DeleteAccountConfirmationButtonText)];

        public string DeleteAccountConfirmationButtonAltText => _localizer[nameof(DeleteAccountConfirmationButtonAltText)];

        public string DeleteAccountModalTitle => _localizer[nameof(DeleteAccountModalTitle)];

        public string DeleteAccountWarning => _localizer[nameof(DeleteAccountWarning)];

        public LocalizedHtmlString DeleteAccountParagraph1Html => _htmlLocalizer[nameof(DeleteAccountParagraph1Html)];

        public string DeleteAccountParagraph2 => _localizer[nameof(DeleteAccountParagraph2)];

        public LocalizedHtmlString DeleteAccountParagraph3Html => _htmlLocalizer[nameof(DeleteAccountParagraph3Html)];

        public string DeleteInProgressText => _localizer[nameof(DeleteInProgressText)];

        public string ErrorTitle => _localizer[nameof(ErrorTitle)];

        public string ErrorMessage => _localizer[nameof(ErrorMessage)];

        public string HelpTitle => _localizer[nameof(HelpTitle)];

        public string HelpMetaDescription => _localizer[nameof(HelpMetaDescription)];

        public string HelpLinkText => _localizer[nameof(HelpLinkText)];

        public string HelpLinkAltText => _localizer[nameof(HelpLinkAltText)];

        public string HomepageTitle => _localizer[nameof(HomepageTitle)];

        public string HomepageLinkText => _localizer[nameof(HomepageLinkText)];

        public string HomepageLinkAltText => _localizer[nameof(HomepageLinkAltText)];

        public string HomepageLead => _localizer[nameof(HomepageLead)];

        public string InstallLinkText => _localizer[nameof(InstallLinkText)];

        public string InstallLinkAltText => _localizer[nameof(InstallLinkAltText)];

        public string ManageTitle => _localizer[nameof(ManageTitle)];

        public string ManageLinkText => _localizer[nameof(ManageLinkText)];

        public string ManageLinkAltText => _localizer[nameof(ManageLinkAltText)];

        public string ManageMetaDescription => _localizer[nameof(ManageMetaDescription)];

        public string ManageLinkedAccountsSubtitle => _localizer[nameof(ManageLinkedAccountsSubtitle)];

        public string ManageLinkedAccountsContent => _localizer[nameof(ManageLinkedAccountsContent)];

        public string ManageLinkOtherAccountsSubtitle => _localizer[nameof(ManageLinkOtherAccountsSubtitle)];

        public string ManagePreferencesTitle => _localizer[nameof(ManagePreferencesTitle)];

        public string ManageLinkedToAlexa => _localizer[nameof(ManageLinkedToAlexa)];

        public string ManageNotLinkedToAlexa => _localizer[nameof(ManageNotLinkedToAlexa)];

        public string ManageUnlinkAlexaButtonText => _localizer[nameof(ManageUnlinkAlexaButtonText)];

        public string ManageUnlinkAlexaButtonAltText => _localizer[nameof(ManageUnlinkAlexaButtonAltText)];

        public string ManageUnlinkAlexaModalTitle => _localizer[nameof(ManageUnlinkAlexaModalTitle)];

        public string ManageUnlinkAlexaModalContent1 => _localizer[nameof(ManageUnlinkAlexaModalContent1)];

        public string ManageUnlinkAlexaModalContent2 => _localizer[nameof(ManageUnlinkAlexaModalContent2)];

        public string ManageUnlinkAlexaModalLoading => _localizer[nameof(ManageUnlinkAlexaModalLoading)];

        public string ManageUnlinkAlexaModalConfirmButtonText => _localizer[nameof(ManageUnlinkAlexaModalConfirmButtonText)];

        public string ManageUnlinkAlexaModalConfirmButtonAltText => _localizer[nameof(ManageUnlinkAlexaModalConfirmButtonAltText)];

        public string NavbarCollapseAltText => _localizer[nameof(NavbarCollapseAltText)];

        public string NavbarMenuText => _localizer[nameof(NavbarMenuText)];

        public string PermissionDeniedTitle => _localizer[nameof(PermissionDeniedTitle)];

        public string PermissionDeniedContent => _localizer[nameof(PermissionDeniedContent)];

        public string PrivacyPolicyTitle => _localizer[nameof(PrivacyPolicyTitle)];

        public string PrivacyPolicyMetaTitle => _localizer[nameof(PrivacyPolicyMetaTitle)];

        public string PrivacyPolicyMetaDescription => _localizer[nameof(PrivacyPolicyMetaDescription)];

        public string PrivacyPolicyLinkText => _localizer[nameof(PrivacyPolicyLinkText)];

        public string PrivacyPolicyLinkAltText => _localizer[nameof(PrivacyPolicyLinkAltText)];

        public string RegisterTitle => _localizer[nameof(RegisterTitle)];

        public string RegisterSubtitle => _localizer[nameof(RegisterSubtitle)];

        public string RegisterMetaDescription => _localizer[nameof(RegisterMetaDescription)];

        public string RegisterLead => _localizer[nameof(RegisterLead)];

        public string RegisterParagraph1 => _localizer[nameof(RegisterParagraph1)];

        public string RegisterParagraph2 => _localizer[nameof(RegisterParagraph2)];

        public string RegisterParagraph3 => _localizer[nameof(RegisterParagraph3)];

        public string RegisterLinkText => _localizer[nameof(RegisterLinkText)];

        public string RegisterLinkAltText => _localizer[nameof(RegisterLinkAltText)];

        public string RegisterSignInSubtitle => _localizer[nameof(RegisterSignInSubtitle)];

        public string RemoveAccountButtonText => _localizer[nameof(RemoveAccountButtonText)];

        public string RemoveAccountLinkModalTitle => _localizer[nameof(RemoveAccountLinkModalTitle)];

        public string RemoveAccountLinkModalDescription => _localizer[nameof(RemoveAccountLinkModalDescription)];

        public string SavePreferencesButtonText => _localizer[nameof(SavePreferencesButtonText)];

        public string SavePreferencesButtonAltText => _localizer[nameof(SavePreferencesButtonAltText)];

        public string SkillNotLinkedTitle => _localizer[nameof(SkillNotLinkedTitle)];

        public string SkillNotLinkedDescription => _localizer[nameof(SkillNotLinkedDescription)];

        public string ClearPreferencesButtonText => _localizer[nameof(ClearPreferencesButtonText)];

        public string ClearPreferencesButtonAltText => _localizer[nameof(ClearPreferencesButtonAltText)];

        public string ResetPreferencesButtonText => _localizer[nameof(ResetPreferencesButtonText)];

        public string ResetPreferencesButtonAltText => _localizer[nameof(ResetPreferencesButtonAltText)];

        public string SignInTitle => _localizer[nameof(SignInTitle)];

        public string SignInMetaDescription => _localizer[nameof(SignInMetaDescription)];

        public string SignInSubtitle => _localizer[nameof(SignInSubtitle)];

        public string SignInRegisterSubtitle => _localizer[nameof(SignInRegisterSubtitle)];

        public string SignInRegisterText => _localizer[nameof(SignInRegisterText)];

        public string SignInLinkText => _localizer[nameof(SignInLinkText)];

        public string SignInLinkAltText => _localizer[nameof(SignInLinkAltText)];

        public string SignInErrorTitle => _localizer[nameof(SignInErrorTitle)];

        public string SignInErrorSubtitle => _localizer[nameof(SignInErrorSubtitle)];

        public string SigningInModalTitle => _localizer[nameof(SigningInModalTitle)];

        public string SigningInModalDescription => _localizer[nameof(SigningInModalDescription)];

        public string SignOutLinkText => _localizer[nameof(SignOutLinkText)];

        public string StatusLinkText => _localizer[nameof(StatusLinkText)];

        public string StatusLinkAltText => _localizer[nameof(StatusLinkAltText)];

        public string TermsOfServiceTitle => _localizer[nameof(TermsOfServiceTitle)];

        public string TermsOfServiceMetaDescription => _localizer[nameof(TermsOfServiceMetaDescription)];

        public string TermsOfServiceMetaTitle => _localizer[nameof(TermsOfServiceMetaTitle)];

        public string TermsOfServiceLinkText => _localizer[nameof(TermsOfServiceLinkText)];

        public string TermsOfServiceLinkAltText => _localizer[nameof(TermsOfServiceLinkAltText)];

        public string UpdateFailureModalTitle => _localizer[nameof(UpdateFailureModalTitle)];

        public string UpdateFailureModalDescription => _localizer[nameof(UpdateFailureModalDescription)];

        public string UpdateSuccessModalTitle => _localizer[nameof(UpdateSuccessModalTitle)];

        public string UpdateProgressModalTitle => _localizer[nameof(UpdateProgressModalTitle)];

        public string UpdateProgressModalDescription => _localizer[nameof(UpdateProgressModalDescription)];

        public string LinePreferencesNoneLead => _localizer[nameof(LinePreferencesNoneLead)];

        public string LinePreferencesNoneContent => _localizer[nameof(LinePreferencesNoneContent), BrandName];

        public string LinePreferencesSingular => _localizer[nameof(LinePreferencesSingular)];

        public string AlexaSignInTitle => _localizer[nameof(AlexaSignInTitle)];

        public string AlexaSignInMetaDescription => _localizer[nameof(AlexaSignInMetaDescription)];

        public string AlexaSignInParagraph1 => _localizer[nameof(AlexaSignInParagraph1)];

        public string AlexaSignInParagraph2 => _localizer[nameof(AlexaSignInParagraph2)];

        public string AlexaSignInParagraph3 => _localizer[nameof(AlexaSignInParagraph3)];

        public string AlexaSignInFormTitle => _localizer[nameof(AlexaSignInFormTitle)];

        public string ErrorTitle400 => _localizer[nameof(ErrorTitle400)];

        public string ErrorSubtitle400 => _localizer[nameof(ErrorSubtitle400)];

        public string ErrorMessage400 => _localizer[nameof(ErrorMessage400)];

        public string ErrorTitle403 => _localizer[nameof(ErrorTitle403)];

        public string ErrorSubtitle403 => _localizer[nameof(ErrorSubtitle403)];

        public string ErrorMessage403 => _localizer[nameof(ErrorMessage403)];

        public string ErrorTitle404 => _localizer[nameof(ErrorTitle404)];

        public string ErrorSubtitle404 => _localizer[nameof(ErrorSubtitle404)];

        public string ErrorMessage404 => _localizer[nameof(ErrorMessage404)];

        public string ErrorTitle405 => _localizer[nameof(ErrorTitle405)];

        public string ErrorSubtitle405 => _localizer[nameof(ErrorSubtitle405)];

        public string ErrorMessage405 => _localizer[nameof(ErrorMessage405)];

        public string ErrorTitle408 => _localizer[nameof(ErrorTitle408)];

        public string ErrorSubtitle408 => _localizer[nameof(ErrorSubtitle408)];

        public string ErrorMessage408 => _localizer[nameof(ErrorMessage408)];

        public string TechnologyTitle => _localizer[nameof(TechnologyTitle)];

        public string TechnologyMetaDescription => _localizer[nameof(TechnologyMetaDescription)];

        public string TechnologyMetaTitle => _localizer[nameof(TechnologyMetaTitle)];

        public string TechnologyLinkText => _localizer[nameof(TechnologyLinkText)];

        public string TechnologyLinkAltText => _localizer[nameof(TechnologyLinkAltText)];

        public string ErrorSubtitle(int? httpCode) => _localizer[nameof(ErrorSubtitle), httpCode ?? 500];

        public LocalizedHtmlString AvailableLinesTitle(string classes, int count) => _htmlLocalizer[nameof(AvailableLinesTitle), classes, count];

        public LocalizedHtmlString FavoriteLinesTitle(string classes, int count) => _htmlLocalizer[nameof(FavoriteLinesTitle), classes, count];

        public string LinePreferencesPlural(int count) => _localizer[nameof(LinePreferencesPlural), count];

        public LocalizedHtmlString OtherLinesTitle(string classes, int count) => _htmlLocalizer[nameof(OtherLinesTitle), classes, count];

        public string RemoveAccountButtonAltText(string provider) => _localizer[nameof(RemoveAccountButtonAltText), provider];

        public string SignInButtonText(string diplayName) => _localizer[nameof(SignInButtonText), diplayName];

        public string SignInButtonAltText(string diplayName) => _localizer[nameof(SignInButtonAltText), diplayName];
    }
}
