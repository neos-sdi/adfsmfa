using System.Threading.Tasks;


namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public interface IMetadataRepository
    {
        Task<MetadataTOCPayload> GetToc();

        Task<MetadataStatement> GetMetadataStatement(MetadataTOCPayloadEntry entry);
    }
}
