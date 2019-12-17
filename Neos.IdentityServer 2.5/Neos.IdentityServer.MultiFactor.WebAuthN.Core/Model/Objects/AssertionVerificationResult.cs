using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    /// <summary>
    /// Result of the MakeAssertion verification
    /// </summary>
    public class AssertionVerificationResult : Fido2ResponseBase
    {
        public byte[] CredentialId { get; set; }
        public uint Counter { get; set; }

        #region Serialization methods
        /// <summary>
        /// ToJson method implementation
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// FromJson method implementation
        /// </summary>
        public static AssertionVerificationResult FromJson(string json)
        {
            return JsonConvert.DeserializeObject<AssertionVerificationResult>(json);
        }
        #endregion
    }
}
