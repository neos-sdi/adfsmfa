using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public abstract class Fido2ResponseBase
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
