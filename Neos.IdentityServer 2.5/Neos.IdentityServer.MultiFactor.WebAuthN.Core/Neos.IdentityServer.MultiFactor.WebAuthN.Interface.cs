using System.Collections.Generic;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.WebAuthN;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public interface IWebAuthN
    {
        AssertionOptions GetAssertionOptions(IEnumerable<PublicKeyCredentialDescriptor> allowedCredentials, UserVerificationRequirement? userVerification, AuthenticationExtensionsClientInputs extensions = null);
        AssertionVerificationResult SetAssertionResult(AuthenticatorAssertionRawResponse assertionResponse, AssertionOptions originalOptions, byte[] storedPublicKey, uint storedSignatureCounter, IsUserHandleOwnerOfCredentialId isUserHandleOwnerOfCredentialIdCallback, byte[] requestTokenBindingId = null);

        RegisterCredentialResult SetRegisterCredentialResult(AuthenticatorAttestationRawResponse attestationResponse, RegisterCredentialOptions origChallenge, IsCredentialIdUniqueToUserDelegate isCredentialIdUniqueToUser, byte[] requestTokenBindingId = null);
        RegisterCredentialOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticationExtensionsClientInputs extensions = null);
        RegisterCredentialOptions GetRegisterCredentialOptions(Fido2User user, List<PublicKeyCredentialDescriptor> excludeCredentials, AuthenticatorSelection authenticatorSelection, AttestationConveyancePreference attestationPreference, AuthenticationExtensionsClientInputs extensions = null);
    }
}
