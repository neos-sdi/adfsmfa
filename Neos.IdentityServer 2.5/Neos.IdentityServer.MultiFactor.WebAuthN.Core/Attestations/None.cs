using Neos.IdentityServer.MultiFactor.WebAuthN.Library.Cbor;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.AttestationFormat
{
    internal class None : AttestationFormat
    {
        public None(CBORObject attStmt, byte[] authenticatorData, byte[] clientDataHash) 
            : base(attStmt, authenticatorData, clientDataHash)
        {
        }

        public override void Verify()
        {
            if (0 != attStmt.Keys.Count && 0 != attStmt.Values.Count)
                throw new VerificationException("Attestation format none should have no attestation statement");
        }
    }
}
