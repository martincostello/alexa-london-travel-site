// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.LondonTravel.Site.Options;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace MartinCostello.LondonTravel.Site;

/// <summary>
/// A class representing the container for site resource strings.
/// </summary>
public class SiteResources(
    IHtmlLocalizer<SiteResources> htmlLocalizer,
    IStringLocalizer<SiteResources> localizer,
    SiteOptions options)
{
    public string? AccountCreatedTitle => localizer[nameof(AccountCreatedTitle)];

    public string? AccountDeletedTitle => localizer[nameof(AccountDeletedTitle)];

    public string? AccountDeletedContent => localizer[nameof(AccountDeletedContent)];

    public string? AccessDeniedTitle => localizer[nameof(AccessDeniedTitle)];

    public string? AccessDeniedSubtitle => localizer[nameof(AccessDeniedSubtitle)];

    public string? AccountLinkDeniedTitle => localizer[nameof(AccountLinkDeniedTitle)];

    public string? AccountLinkDeniedContent => localizer[nameof(AccountLinkDeniedContent)];

    public string? AccountLinkFailedTitle => localizer[nameof(AccountLinkFailedTitle)];

    public string? AccountLinkFailedContent => localizer[nameof(AccountLinkFailedContent)];

    public string? AccountLockedTitle => localizer[nameof(AccountLockedTitle)];

    public string? AccountLockedSubtitle => localizer[nameof(AccountLockedSubtitle)];

    public string? AlreadyRegisteredTitle => localizer[nameof(AlreadyRegisteredTitle)];

    public string? AlreadyRegisteredContent1 => localizer[nameof(AlreadyRegisteredContent1)];

    public string? AlreadyRegisteredContent2 => localizer[nameof(AlreadyRegisteredContent2)];

    public string? BrandName => localizer[nameof(BrandName)];

    public string? CancelButtonText => localizer[nameof(CancelButtonText)];

    public string? CancelButtonAltText => localizer[nameof(CancelButtonAltText)];

    public string? CloseButtonText => localizer[nameof(CloseButtonText)];

    public string? CommuteSkillInvocation => localizer[nameof(CommuteSkillInvocation)];

    public LocalizedHtmlString CopyrightText => htmlLocalizer[nameof(CopyrightText), options?.Metadata?.Author?.Name!, DateTimeOffset.UtcNow.Year];

    public string? DeleteAccountButtonText => localizer[nameof(DeleteAccountButtonText)];

    public string? DeleteAccountButtonAltText => localizer[nameof(DeleteAccountButtonAltText)];

    public string? DeleteAccountConfirmationButtonText => localizer[nameof(DeleteAccountConfirmationButtonText)];

    public string? DeleteAccountConfirmationButtonAltText => localizer[nameof(DeleteAccountConfirmationButtonAltText)];

    public string? DeleteAccountModalTitle => localizer[nameof(DeleteAccountModalTitle)];

    public string? DeleteAccountWarning => localizer[nameof(DeleteAccountWarning)];

    public LocalizedHtmlString DeleteAccountParagraph1Html => htmlLocalizer[nameof(DeleteAccountParagraph1Html)];

    public string? DeleteAccountParagraph2 => localizer[nameof(DeleteAccountParagraph2)];

    public LocalizedHtmlString DeleteAccountParagraph3Html => htmlLocalizer[nameof(DeleteAccountParagraph3Html)];

    public string? DeleteInProgressText => localizer[nameof(DeleteInProgressText)];

    public string? ErrorTitle => localizer[nameof(ErrorTitle)];

    public string? ErrorMessage => localizer[nameof(ErrorMessage)];

    public string? ErrorRequestId => localizer[nameof(ErrorRequestId)];

    public string? HelpTitle => localizer[nameof(HelpTitle)];

    public string? HelpMetaDescription => localizer[nameof(HelpMetaDescription)];

    public string? HelpLinkText => localizer[nameof(HelpLinkText)];

    public string? HelpLinkAltText => localizer[nameof(HelpLinkAltText)];

    public string? HomepageTitle => localizer[nameof(HomepageTitle)];

    public string? HomepageLinkText => localizer[nameof(HomepageLinkText)];

    public string? HomepageLinkAltText => localizer[nameof(HomepageLinkAltText)];

    public string? HomepageLead => localizer[nameof(HomepageLead)];

    public string? InstallLinkText => localizer[nameof(InstallLinkText)];

    public string? InstallLinkAltText => localizer[nameof(InstallLinkAltText)];

    public string? ManageTitle => localizer[nameof(ManageTitle)];

    public string? ManageLinkText => localizer[nameof(ManageLinkText)];

    public string? ManageLinkAltText => localizer[nameof(ManageLinkAltText)];

    public string? ManageMetaDescription => localizer[nameof(ManageMetaDescription)];

    public string? ManageLinkedAccountsSubtitle => localizer[nameof(ManageLinkedAccountsSubtitle)];

    public string? ManageLinkedAccountsContent => localizer[nameof(ManageLinkedAccountsContent)];

    public string? ManageLinkOtherAccountsSubtitle => localizer[nameof(ManageLinkOtherAccountsSubtitle)];

    public string? ManagePreferencesTitle => localizer[nameof(ManagePreferencesTitle)];

    public string? ManageLinkedToAlexa => localizer[nameof(ManageLinkedToAlexa)];

    public string? ManageNotLinkedToAlexa => localizer[nameof(ManageNotLinkedToAlexa)];

    public string? ManageUnlinkAlexaButtonText => localizer[nameof(ManageUnlinkAlexaButtonText)];

    public string? ManageUnlinkAlexaButtonAltText => localizer[nameof(ManageUnlinkAlexaButtonAltText)];

    public string? ManageUnlinkAlexaModalTitle => localizer[nameof(ManageUnlinkAlexaModalTitle)];

    public string? ManageUnlinkAlexaModalContent1 => localizer[nameof(ManageUnlinkAlexaModalContent1)];

    public string? ManageUnlinkAlexaModalContent2 => localizer[nameof(ManageUnlinkAlexaModalContent2)];

    public string? ManageUnlinkAlexaModalLoading => localizer[nameof(ManageUnlinkAlexaModalLoading)];

    public string? ManageUnlinkAlexaModalConfirmButtonText => localizer[nameof(ManageUnlinkAlexaModalConfirmButtonText)];

    public string? ManageUnlinkAlexaModalConfirmButtonAltText => localizer[nameof(ManageUnlinkAlexaModalConfirmButtonAltText)];

    public string? NavbarCollapseAltText => localizer[nameof(NavbarCollapseAltText)];

    public string? NavbarMenuText => localizer[nameof(NavbarMenuText)];

    public string? PermissionDeniedTitle => localizer[nameof(PermissionDeniedTitle)];

    public string? PermissionDeniedContent => localizer[nameof(PermissionDeniedContent)];

    public string? PrivacyPolicyTitle => localizer[nameof(PrivacyPolicyTitle)];

    public string? PrivacyPolicyMetaTitle => localizer[nameof(PrivacyPolicyMetaTitle)];

    public string? PrivacyPolicyMetaDescription => localizer[nameof(PrivacyPolicyMetaDescription)];

    public string? PrivacyPolicyLinkText => localizer[nameof(PrivacyPolicyLinkText)];

    public string? PrivacyPolicyLinkAltText => localizer[nameof(PrivacyPolicyLinkAltText)];

    public string? RegisterTitle => localizer[nameof(RegisterTitle)];

    public string? RegisterSubtitle => localizer[nameof(RegisterSubtitle)];

    public string? RegisterMetaDescription => localizer[nameof(RegisterMetaDescription)];

    public string? RegisterLead => localizer[nameof(RegisterLead)];

    public string? RegisterParagraph1 => localizer[nameof(RegisterParagraph1)];

    public string? RegisterParagraph2 => localizer[nameof(RegisterParagraph2)];

    public string? RegisterParagraph3 => localizer[nameof(RegisterParagraph3)];

    public string? RegisterLinkText => localizer[nameof(RegisterLinkText)];

    public string? RegisterLinkAltText => localizer[nameof(RegisterLinkAltText)];

    public string? RegisterSignInSubtitle => localizer[nameof(RegisterSignInSubtitle)];

    public string? RemoveAccountButtonText => localizer[nameof(RemoveAccountButtonText)];

    public string? RemoveAccountLinkModalTitle => localizer[nameof(RemoveAccountLinkModalTitle)];

    public string? RemoveAccountLinkModalDescription => localizer[nameof(RemoveAccountLinkModalDescription)];

    public string? SavePreferencesButtonText => localizer[nameof(SavePreferencesButtonText)];

    public string? SavePreferencesButtonAltText => localizer[nameof(SavePreferencesButtonAltText)];

    public string? SkillNotLinkedTitle => localizer[nameof(SkillNotLinkedTitle)];

    public string? SkillNotLinkedDescription => localizer[nameof(SkillNotLinkedDescription)];

    public string? ClearPreferencesButtonText => localizer[nameof(ClearPreferencesButtonText)];

    public string? ClearPreferencesButtonAltText => localizer[nameof(ClearPreferencesButtonAltText)];

    public string? ResetPreferencesButtonText => localizer[nameof(ResetPreferencesButtonText)];

    public string? ResetPreferencesButtonAltText => localizer[nameof(ResetPreferencesButtonAltText)];

    public string? SignInTitle => localizer[nameof(SignInTitle)];

    public string? SignInMetaDescription => localizer[nameof(SignInMetaDescription)];

    public string? SignInSubtitle => localizer[nameof(SignInSubtitle)];

    public string? SignInRegisterSubtitle => localizer[nameof(SignInRegisterSubtitle)];

    public string? SignInRegisterText => localizer[nameof(SignInRegisterText)];

    public string? SignInLinkText => localizer[nameof(SignInLinkText)];

    public string? SignInLinkAltText => localizer[nameof(SignInLinkAltText)];

    public string? SignInErrorTitle => localizer[nameof(SignInErrorTitle)];

    public string? SignInErrorSubtitle => localizer[nameof(SignInErrorSubtitle)];

    public string? SigningInModalTitle => localizer[nameof(SigningInModalTitle)];

    public string? SigningInModalDescription => localizer[nameof(SigningInModalDescription)];

    public string? SignOutLinkText => localizer[nameof(SignOutLinkText)];

    public string? TermsOfServiceTitle => localizer[nameof(TermsOfServiceTitle)];

    public string? TermsOfServiceMetaDescription => localizer[nameof(TermsOfServiceMetaDescription)];

    public string? TermsOfServiceMetaTitle => localizer[nameof(TermsOfServiceMetaTitle)];

    public string? TermsOfServiceLinkText => localizer[nameof(TermsOfServiceLinkText)];

    public string? TermsOfServiceLinkAltText => localizer[nameof(TermsOfServiceLinkAltText)];

    public string? UpdateFailureModalTitle => localizer[nameof(UpdateFailureModalTitle)];

    public string? UpdateFailureModalDescription => localizer[nameof(UpdateFailureModalDescription)];

    public string? UpdateSuccessModalTitle => localizer[nameof(UpdateSuccessModalTitle)];

    public string? UpdateProgressModalTitle => localizer[nameof(UpdateProgressModalTitle)];

    public string? UpdateProgressModalDescription => localizer[nameof(UpdateProgressModalDescription)];

    public string? LinePreferencesNoneLead => localizer[nameof(LinePreferencesNoneLead)];

    public string? LinePreferencesNoneContent => localizer[nameof(LinePreferencesNoneContent), BrandName!];

    public string? LinePreferencesSingular => localizer[nameof(LinePreferencesSingular)];

    public string? AlexaSignInTitle => localizer[nameof(AlexaSignInTitle)];

    public string? AlexaSignInMetaDescription => localizer[nameof(AlexaSignInMetaDescription)];

    public string? AlexaSignInParagraph1 => localizer[nameof(AlexaSignInParagraph1)];

    public string? AlexaSignInParagraph2 => localizer[nameof(AlexaSignInParagraph2)];

    public string? AlexaSignInParagraph3 => localizer[nameof(AlexaSignInParagraph3)];

    public string? AlexaSignInFormTitle => localizer[nameof(AlexaSignInFormTitle)];

    public string? ErrorTitle400 => localizer[nameof(ErrorTitle400)];

    public string? ErrorSubtitle400 => localizer[nameof(ErrorSubtitle400)];

    public string? ErrorMessage400 => localizer[nameof(ErrorMessage400)];

    public string? ErrorTitle403 => localizer[nameof(ErrorTitle403)];

    public string? ErrorSubtitle403 => localizer[nameof(ErrorSubtitle403)];

    public string? ErrorMessage403 => localizer[nameof(ErrorMessage403)];

    public string? ErrorTitle404 => localizer[nameof(ErrorTitle404)];

    public string? ErrorSubtitle404 => localizer[nameof(ErrorSubtitle404)];

    public string? ErrorMessage404 => localizer[nameof(ErrorMessage404)];

    public string? ErrorTitle405 => localizer[nameof(ErrorTitle405)];

    public string? ErrorSubtitle405 => localizer[nameof(ErrorSubtitle405)];

    public string? ErrorMessage405 => localizer[nameof(ErrorMessage405)];

    public string? ErrorTitle408 => localizer[nameof(ErrorTitle408)];

    public string? ErrorSubtitle408 => localizer[nameof(ErrorSubtitle408)];

    public string? ErrorMessage408 => localizer[nameof(ErrorMessage408)];

    public string? TechnologyTitle => localizer[nameof(TechnologyTitle)];

    public string? TechnologyMetaDescription => localizer[nameof(TechnologyMetaDescription)];

    public string? TechnologyMetaTitle => localizer[nameof(TechnologyMetaTitle)];

    public string? TechnologyLinkText => localizer[nameof(TechnologyLinkText)];

    public string? TechnologyLinkAltText => localizer[nameof(TechnologyLinkAltText)];

    public string? ApiDocumentationMetaTitle => localizer[nameof(ApiDocumentationMetaTitle)];

    public string? ApiDocumentationMetaDescription => localizer[nameof(ApiDocumentationMetaDescription)];

    public string? ErrorSubtitle(int? httpCode) => localizer[nameof(ErrorSubtitle), httpCode ?? 500];

    public LocalizedHtmlString AvailableLinesTitle(string classes, int count) => htmlLocalizer[nameof(AvailableLinesTitle), classes, count];

    public LocalizedHtmlString FavoriteLinesTitle(string classes, int count) => htmlLocalizer[nameof(FavoriteLinesTitle), classes, count];

    public string? LinePreferencesPlural(int count) => localizer[nameof(LinePreferencesPlural), count];

    public LocalizedHtmlString OtherLinesTitle(string classes, int count) => htmlLocalizer[nameof(OtherLinesTitle), classes, count];

    public string? RegisterParagraph4(long count) => localizer[nameof(RegisterParagraph4), count];

    public string? RemoveAccountButtonAltText(string? provider) => localizer[nameof(RemoveAccountButtonAltText), provider ?? string.Empty];

    public string? SignInButtonText(string diplayName) => localizer[nameof(SignInButtonText), diplayName];

    public string? SignInButtonAltText(string diplayName) => localizer[nameof(SignInButtonAltText), diplayName];
}
