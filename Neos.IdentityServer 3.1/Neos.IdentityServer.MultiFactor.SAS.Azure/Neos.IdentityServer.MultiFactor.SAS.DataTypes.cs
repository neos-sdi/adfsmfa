//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
//                                                                                                                                                                                          //
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),                                       //
// to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software,   //
// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:                                                                                   //
//                                                                                                                                                                                          //
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.                                                           //
//                                                                                                                                                                                          //
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,                                      //
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,                            //
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                               //
//                                                                                                                                                                                          //
//                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Neos.IdentityServer.MultiFactor.SAS
{
    internal interface ISasProvider
    {
        GetAvailableAuthenticationMethodsResponse GetAvailableAuthenticationMethods(GetAvailableAuthenticationMethodsRequest request);
        BeginTwoWayAuthenticationResponse BeginTwoWayAuthentication(BeginTwoWayAuthenticationRequest request);
        EndTwoWayAuthenticationResponse EndTwoWayAuthentication(EndTwoWayAuthenticationRequest request);
        GetActivationCodeResponse GetActivationCode(GetActivationCodeRequest request);
        GetActivationStatusResponse GetActivationStatus(GetActivationStatusRequest request);
    } 

    [DataContract(Namespace = "")]
	internal class Result
	{
		[DataMember(Order = 0)]
		internal string Value
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string Message
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal bool Retry
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		public string Error
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		public string Exception
		{
			get;
			set;
		}
		internal Result(string value, string message, bool retry)
		{
			this.Value = value;
			this.Message = message;
			this.Retry = retry;
		}
	}

	[DataContract(Namespace = "")]
	internal class AuthenticationMethod
	{
		[DataMember(Order = 0)]
		internal string Id
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal bool IsDefault
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal IDictionary<string, string> Properties
		{
			get;
			set;
		}
		internal AuthenticationMethod()
		{
			this.Properties = new Dictionary<string, string>();
		}
	}

	[DataContract(Namespace = "")]
	internal class BeginTwoWayAuthenticationRequest
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string UserPrincipalName
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string TenantId
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		internal string Lcid
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		internal string AuthenticationMethodId
		{
			get;
			set;
		}
		[DataMember(Order = 5)]
		internal IDictionary<string, string> AuthenticationMethodProperties
		{
			get;
			set;
		}
		[DataMember(Order = 6)]
		internal string AuthenticationPurpose
		{
			get;
			set;
		}
		[DataMember(Order = 7)]
		internal string ContextId
		{
			get;
			set;
		}
		[DataMember(Order = 8)]
		internal int OtpRetryCount
		{
			get;
			set;
		}
		[DataMember(Order = 9)]
		internal bool SyncCall
		{
			get;
			set;
		}
		[DataMember(Order = 10)]
		internal string ReplicationScope
		{
			get;
			set;
		}
		[DataMember(Order = 11)]
		internal string ObjectId
		{
			get;
			set;
		}
		[DataMember(Order = 12)]
		public string CompanyName
		{
			get;
			set;
		}
		internal BeginTwoWayAuthenticationRequest()
		{
			this.AuthenticationMethodProperties = new Dictionary<string, string>();
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class BeginTwoWayAuthenticationResponse
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal Result Result
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string SessionId
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		public string AuthenticationResult
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		public string UserPrincipalName
		{
			get;
			set;
		}
		internal BeginTwoWayAuthenticationResponse()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class EndTwoWayAuthenticationRequest
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string SessionId
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string ContextId
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		internal string AdditionalAuthData
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		public string UserPrincipalName
		{
			get;
			set;
		}
		internal EndTwoWayAuthenticationRequest()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class EndTwoWayAuthenticationResponse
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal Result Result
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string AuthenticationResult
		{
			get;
			set;
		}
		internal EndTwoWayAuthenticationResponse()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetActivationCodeRequest
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string UserPrincipalName
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string TenantId
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		internal string ContextId
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		internal string ReplicationScope
		{
			get;
			set;
		}
		[DataMember(Order = 5)]
		internal string ObjectId
		{
			get;
			set;
		}
		internal GetActivationCodeRequest()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetActivationCodeResponse
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal Result Result
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string ActivationCode
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		internal string Url
		{
			get;
			set;
		}
		internal GetActivationCodeResponse()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetActivationStatusRequest
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string ActivationCode
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string ContextId
		{
			get;
			set;
		}
		internal GetActivationStatusRequest()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetActivationStatusResponse
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal Result Result
		{
			get;
			set;
		}
		internal GetActivationStatusResponse()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetAvailableAuthenticationMethodsRequest
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal string UserPrincipalName
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal string ContextId
		{
			get;
			set;
		}
		internal GetAvailableAuthenticationMethodsRequest()
		{
			this.Version = 1.0;
		}
	}

	[DataContract(Namespace = "")]
	internal class GetAvailableAuthenticationMethodsResponse
	{
		[DataMember(Order = 0)]
		internal double Version
		{
			get;
			set;
		}
		[DataMember(Order = 1)]
		internal Result Result
		{
			get;
			set;
		}
		[DataMember(Order = 2)]
		internal IList<AuthenticationMethod> AuthenticationMethods
		{
			get;
			set;
		}
		[DataMember(Order = 3)]
		public long CreateTimeSeconds
		{
			get;
			set;
		}
		[DataMember(Order = 4)]
		public string UserPrincipalName
		{
			get;
			set;
		}
		internal GetAvailableAuthenticationMethodsResponse()
		{
			this.AuthenticationMethods = new List<AuthenticationMethod>();
			this.Version = 1.0;
		}
	}

    /*
    internal enum EventIds: int
    {
        FsServiceStart = 100,
        StartupException = 102,
        FsServiceStop,
        ArtifactServiceNotRunningForReplayDetectionCheck,
        AuthMethodLoadError,
        AuthMethodLoadSuccess,
        WsTrustRequestProcessingError = 111,
        BadConfigurationFormatError = 131,
        BadConfigurationValueMissing,
        BadConfigurationIdentityCertificateHasNoPrivateKey,
        BadConfigurationCertificateNotFound,
        BadConfigurationMultipleCertificatesMatch,
        ConfigurationErrorsException,
        UnableToCreateFederationMetadataDocument = 143,
        RequestBlocked,
        InvalidClaimsProviderError = 147,
        AttributeStoreLoadFailure = 149,
        MetadataListenerError = 155,
        TrustMonitoringInitiated,
        TrustMonitoringComplete,
        TrustMonitoringConfigurationDatabaseWriteError = 159,
        TrustMonitoringInitiationError = 163,
        TrustMonitoringConfigurationDatabaseError,
        TrustMonitoringGenericError,
        TrustMonitoringMetadataFormatError,
        TrustMonitoringMetadataProcessingError,
        TrustManagementMetadataRequestError,
        SuccessfulAutoUpdate = 171,
        SuccessfulAutoUpdateWithWarning = 173,
        AutoUpdateSkippedWithWarning,
        MinorVersionUpgradeError = 180,
        InvalidRelyingPartyError = 184,
        PolicyDuplicateNameIdentifier = 186,
        PolicyUnknownAuthenticationTypeError = 193,
        ConfigurationInvalidAuthenticationTypeError = 197,
        FsProxyServiceStart,
        ProxyStartupException,
        FsProxyServiceStop,
        ServiceHostOpenAddressAccessDeniedError,
        ServiceHostOpenError,
        ServiceHostAbortError,
        ServiceHostCloseError,
        EmptyOrMissingWSFederationPassiveEndpoint = 206,
        FailureWritingToAuditLog,
        InsufficientPrivilegesWritingToAuditLogError,
        AuditLogEventSourceCouldNotBeRegisteredError,
        FsProxyNoEndpointsConfigured = 215,
        BindingConfigurationError = 217,
        FsProxyServiceConnectionFailedServiceUnavailable,
        ServiceConfigurationInitializationError = 220,
        ServiceConfigurationReloadError,
        FsProxyServiceConnectionFailedTimeout,
        ClaimDescriptionReloadError,
        ProxyConfigurationRefreshError,
        ProxyCongestionWindowMinimumSize = 230,
        AttributeStoreFindDCFailedError = 238,
        MetadataExchangeListenerError = 244,
        ProxyConfigurationRefreshSuccess,
        LdapDCConnectionError,
        LdapGCConnectionError,
        ProxyEndpointsRetrievalError,
        AdditionalCertificateLoadWarning,
        ArtifactExpirationError,
        AttributeStoreLoadSuccess,
        ProxyHttpListenerStartupInfo,
        ProxyHttpListenerStartupError,
        ConfigurationMissingAssertionConsumerServicesError = 258,
        ConfigurationAssertionConsumerServiceIndexDoesNotMatchError,
        ConfigurationAssertionConsumerServiceProtocolBindingDoesNotMatchError,
        ConfigurationAssertionConsumerServiceUrlDoesNotMatchError,
        ArtifactResolutionFailed,
        ConfigurationAssertionConsumerServiceNotFoundError = 273,
        FsProxyEndpointListenerAccessDeniedError,
        FsProxySslTrustError,
        FsProxyServiceNotTrustedOnStsError,
        UnhandledExceptionError,
        ArtifactResolutionEndpointNotConfiguredError,
        SamlArtifactResolutionClaimsProviderNotFoundError,
        ClaimsProviderMissingArtifactServiceError,
        SamlArtifactResolutionEndpointNotFoundError,
        SamlArtifactResolutionSignatureVerificationFailureAudit,
        SamlArtifactResolutionRequestError,
        SamlArtifactResolutionResponseVerificationError,
        SamlArtifactResolutionBadResponseError,
        ArtifactStorageConnectionOpenError,
        ArtifactStorageAddError,
        ArtifactStorageGetError,
        ArtifactStorageRemoveError,
        ArtifactStorageExpireError,
        ArtifactServiceStartupException,
        ArtifactResolutionServiceSignatureVerificationFailureAudit,
        ArtifactRequestedButDisabledError,
        ArtifactResolutionServiceIdentityNotFoundError,
        ArtifactResolutionServiceNoSignatureFailureAudit = 296,
        ArtifactResolutionServiceBadEndpointIndexError,
        TokenIssuanceSuccessAudit = 299,
        WSTrustRequestProcessingGeneralTokenIssuanceFailureAudit,
        ActAsAuthorizationTokenIssuanceFailureAudit,
        ActAsAuthorizationError,
        SamlRequestProcessingError,
        SamlRequestProcessingGeneralTokenIssuanceFailureAudit,
        LdapDCServerError,
        LdapGCServerError,
        ConfigurationChangeSuccessAudit,
        ConfigurationChangeFailureAudit,
        WmiConfigurationChangeSuccessAudit,
        WmiConfigurationChangeFailureAudit,
        PerformanceCounterFailure,
        ClaimsProviderSigningCertificateCrlCheckFailure = 315,
        RelyingPartySigningCertificateCrlCheckFailure,
        RelyingPartyEncryptionCertificateCrlCheckFailure,
        ClientCertificateCrlCheckFailure = 319,
        SamlProtocolSignatureVerificationError,
        InvalidNameIdPolicyError,
        OnBehalfOfAuthorizationTokenIssuanceFailureAudit,
        OnBehalfOfAuthorizationError,
        CallerAuthorizationTokenIssuanceFailureAudit,
        CallerAuthorizationError,
        ClaimsPolicyInvalidPolicyTypeError,
        SamlSingleLogoutError,
        SamlArtifactResolutionNoAssertion,
        AdditionalBlobCertificateLoadWarning,
        CertificateManagementDecryptionError = 331,
        CertificateManagementEncryptionError,
        CertificateManagementConfigurationError,
        CertificateManagementWarning,
        CertificateManagementInfo,
        CertificateManagementInitiated,
        CertificateManagementComplete,
        CertificateManagementGenericError,
        CertificateManagementInitiationError,
        ArtifactResolutionSuccessAudit,
        SecurityTokenNotYetValidError,
        SecurityTokenValidationError,
        ConfigurationDatabaseSynchronizationInitiationError,
        ConfigurationDatabaseSynchronizationSyncError,
        ConfigurationDatabaseSynchronizationCommunicationError,
        ConfigurationDatabaseReadOnlyTransferError,
        ConfigurationDatabaseSynchronizationCompleted = 348,
        FsAdministrationServiceStart,
        PolicyStoreSynchronizationPropertiesGetError = 351,
        ConfigurationDatabaseSqlError,
        SamlArtifactResolutionSignatureVerificationError,
        ArtifactResolutionServiceSignatureVerificationError,
        SqlNotificationRegistrationError = 356,
        SqlNotificationRegistrationResumption,
        ServiceHostRestart,
        ServiceHostRestartError,
        ClientCertificateNotPresentOnProxyEndpointError,
        WSFederationPassiveSignOutError = 362,
        WSFederationPassiveServiceCommunicationError,
        WSFederationPassiveRequestFailedError,
        RelyingPartyNotEnabled,
        ClaimsProviderNotEnabled,
        AudienceUriValidationFailed,
        SamlLogoutNameIdentifierNotFoundError,
        WSFederationPassiveTtpRequestError,
        WSFederationPassiveTtpResponseError,
        AuthorityCertificateResolveError,
        WeakSignatureAlgorithmError,
        ArtifactResolutionServiceWeakSignatureAlgorithmError,
        AuthorityEncryptionCertificateCrlCheckFailure,
        PolicyStoreSynchronizationInitiated,
        SqlAttributeStoreQueryExecutionError,
        AttributeStoreError,
        SAMLRequestUnsupportedSignatureAlgorithm,
        InvalidIssuanceInstantError,
        BadConfigurationIdentityCertificateNotValid,
        AdditionalCertificateValidationFailure,
        SynchronizationThresholdViolation,
        WSFederationPassiveWebConfigMalformedError,
        WSFederationPassiveInvalidValueInWebConfigError,
        ConfigurationHasExpiredCertsWarning,
        ConfigurationHealthyCertsInfo,
        CertPrivateKeyInaccessibleError,
        CertPrivateKeyAccessibleInfo,
        TrustsHaveExpiredCertsWarning,
        TrustsHaveHealthyCertsInfo,
        FsProxyTrustTokenRenewalSuccess = 392,
        ProxyTrustTokenIssuanceFailure,
        FsProxyTrustTokenRenewalError,
        ProxyTrustTokenIssuanceSuccess,
        ProxyTrustTokenRenewalSuccess,
        HttpProxyConfigurationInfo,
        ConfigurationHasArchivedCertsWarning,
        ConfigurationHealthyUnarchivedCertsInfo,
        GiveUserVSSAccess,
        RevokeUserVSSAccess,
        CertificateClaimUnknownError,
        RequestReceivedSuccessAudit,
        ResponseSentSuccessAudit,
        PasswordChangeSuccessAudit,
        PasswordChangeFailureAudit,
        PasswordChangeError,
        DeviceAuthenticationFailureAudit,
        DeviceAuthenticationSuccessAudit,
        RequestContextHeadersSuccessAudit,
        SecurityTokenValidationFailureAudit,
        AuthenticationSuccessAudit,
        CallerIdFailureAudit,
        InvalidMsisHttpRequestAudit,
        UnregisteredDrsUpnSuffixes,
        WebConfigurationError,
        CertificateClaimError,
        StsProxyTrustRenewalAuditSuccess,
        StsProxyTrustRenewalAuditFailure,
        StsProxyTrustTokenEstablishmentAuditSuccess,
        StsProxyTrustTokenEstablishmentAuditFailure,
        ClientCertNotTrustedOnStsAuditFailure = 424,
        ApplicationProxyConfigurationStoreChangeAuditSuccess,
        ApplicationProxyConfigurationStoreChangeAuditFailure,
        ApplicationProxyTrustUpdateAuditSuccess,
        ApplicationProxyTrustUpdateAuditFailure,
        RelyingPartyTrustUpdateAuditSuccess,
        RelyingPartyTrustUpdateAuditFailure,
        ActiveRequestRSTSuccessAudit,
        ProxyConfigurationEndpointError,
        ProxyTrustTokenRenewalError,
        CertificateAuthorityExpirationCheckWarning,
        PrimarySigningCertificateRolloverCheckWarning,
        PrimaryDecryptionCertificateRolloverCheckWarning,
        CertificateRolloverCheckExceptionWarning,
        CertificateAuthorityRolloverExceptionWarning,
        EnrollmentCertificateReadFromTemplateError,
        EnrollmentCertificateSetInfo,
        TokenBindingKeyInvalid,
        IssuedIdentityClaims = 500,
        CallerIdentityClaims,
        OnBehalfOfUserIdentityClaims,
        ActAsUserIdentityClaims,
        ApplicationProxyConfigurationStoreChangeSuccess,
        ApplicationProxyConfigurationStoreChangeFailure,
        ApplicationProxyTrustUpdateSuccess,
        ApplicationProxyTrustUpdateFailure,
        RelyingPartyTrustUpdateSuccess,
        RelyingPartyTrustUpdateFailure,
        LongText,
        InvalidMsisHttpSigninRequestFailure,
        ExtranetLockoutAccountThrottledAudit,
        ArtifactRestEndpointRequestFailureAudit,
        ArtifactRestEndpointRequestSuccessAudit,
        ExtranetLockoutUserThrottleTransitionAudit,
        ExtranetLockoutAccountRestrictedAudit,
        TargetRelyingPartyPublishedButAppProxyDisabledFailure,
        TargetRelyingPartyPublishedButAppProxyDisabledFailureAudit,
        PrimayServerRequestHandlerResponseSuccessAudit,
        PrimayServerRequestHandlerResponseFailureAudit,
        RelyingPartyTokenRequestFailure,
        RelyingPartyTokenRequestAuditFailure,
        RelyingPartyTokenRequestAuditSuccess,
        LocalCPTrustReadWarning = 530,
        LocalCPTrustFirstReadError,
        UnableToCreateOAuthDiscoveryDocument = 540,
        ProxyConfigDataFarmBehaviorMalformedError,
        HeartbeatError,
        HeartbeatCommunicationError,
        HeartbeatWarning,
        HeartbeatInformation,
        AzureMfaCertificateNotFound,
        AzureMfaCertificateRenewed,
        AzureMfaCertificateExpirationWarning,
        AzureMfaCertificateExpired,
        CertKeySpecMissing,
        CallerId = 1000,
        OAuthAuthorizationRequestFailedError = 1020,
        OAuthTokenRequestFailedError,
        OAuthAuthorizationCodeIssuanceSuccessAudit,
        OAuthAccessTokenIssuanceSuccessAudit,
        OAuthRefreshTokenIssuanceSuccessAudit,
        OAuthAuthorizationCodeIssuanceFailureAudit,
        OAuthAccessTokenIssuanceFailureAudit,
        OAuthAccessTokenResponseIssuanceSuccessAudit,
        OAuthClientAuthenticationSuccessAudit,
        OAuthClientAuthenticationFaultAudit,
        OAuthClientCredentialsFaultAudit,
        OAuthClientCredentialsIssuanceSuccessAudit,
        OAuthIdTokenIssuanceFailureAudit,
        OAuthIdTokenIssuanceSuccessAudit,
        OAuthOnBehalfOfFaultAudit,
        OAuthOnBehalfOfIssuanceSuccessAudit,
        OAuthLogonCertificateFaultAudit,
        OAuthLogonCertificateIssuanceSuccessAudit,
        OAuthVPNCertificateFaultAudit,
        OAuthAuthCodeVPNCertificateIssuanceSuccessAudit,
        OAuthRefreshTokenVPNCertificateIssuanceSuccessAudit,
        OAuthPrimaryRefreshTokenIssuanceSuccessAudit,
        OAuthNextGenCredsIssuanceSuccessAudit,
        OAuthNextGenCredsIssuanceFailureAudit,
        WebFingerRequestError = 1080,
        UserInfoEndpointRequestFailureAudit = 1090,
        UserInfoEndpointRequestSuccessAudit,
        RestEndpointAuthorizationFailureError = 1100,
        RestEndpointAuthorizationFailureAudit,
        RestEndpointAuthorizationSuccessAudit,
        LdapStoreQueryUserDnFailureAudit,
        LdapStoreQueryUserDnSuccessAudit,
        LdapStoreBindFailureAudit,
        LdapStoreBindSuccessAudit,
        LdapStoreQueryUserAttrFailureAudit,
        LdapStoreQueryUserAttrSuccessAudit,
        LdapAccountStoreConnectionFailure,
        LdapAttributeStorePrimaryConnectionFailure,
        LdapAttributeStoreCompleteConnectionFailure,
        LdapAttributeStoreConnectionFailure,
        ClientJWKSyncingInitiated,
        ClientJWKSyncingComplete,
        ClientJWKSyncingError,
        ClientJWKSyncingDatabaseError,
        ClientJWKSyncingClientError,
        ClientJWKSyncingGenericError,
        JWTSigningKeysDownloadedSuccessAudit,
        JWTSigningKeysDownloadFailureAudit,
        TokenBindingKeyFailureAudit,
        AppTokenSuccessAudit = 1200,
        AppTokenFailureAudit,
        FreshCredentialSuccessAudit,
        FreshCredentialFailureAudit,
        PasswordChangeBasicSuccessAudit,
        PasswordChangeBasicFailureAudit,
        SignOutSuccessAudit,
        SignOutFailureAudit,
        ExtranetLockoutAudit = 1210
    }
     */
}

