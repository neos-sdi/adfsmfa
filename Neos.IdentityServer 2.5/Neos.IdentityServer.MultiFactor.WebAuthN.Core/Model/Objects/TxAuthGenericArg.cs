using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Objects
{
    public class TxAuthGenericArg
    {
        [JsonProperty("contentType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContentType { get; set; }
        [JsonProperty("content", NullValueHandling = NullValueHandling.Ignore)]
        public byte[] Content { get; set; }
    }
}

