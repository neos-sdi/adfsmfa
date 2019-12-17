using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    public class AuthenticatorBiometricPerfBounds
    {
        [JsonProperty("FAR", NullValueHandling = NullValueHandling.Ignore)]
        public float FAR { get; set; }
        [JsonProperty("FRR", NullValueHandling = NullValueHandling.Ignore)]
        public float FRR { get; set; }
    }
}

