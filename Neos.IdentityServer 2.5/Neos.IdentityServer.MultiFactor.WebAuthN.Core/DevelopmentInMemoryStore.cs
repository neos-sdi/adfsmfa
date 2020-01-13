//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
//******************************************************************************************************************************************************************************************//
// Copyright (c) 2019 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
// https://adfsmfa.codeplex.com                                                                                                                                                             //
// https://github.com/neos-sdi/adfsmfa                                                                                                                                                      //
//                                                                                                                                                                                          //
//******************************************************************************************************************************************************************************************//
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neos.IdentityServer.MultiFactor.WebAuthN.Objects;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class DevelopmentInMemoryStore
    {
        private readonly ConcurrentDictionary<string, Fido2User> _storedUsers = new ConcurrentDictionary<string, Fido2User>();
        private readonly List<StoredCredential> _storedCredentials = new List<StoredCredential>();

        public Fido2User GetOrAddUser(string username, Func<Fido2User> addCallback)
        {
            return _storedUsers.GetOrAdd(username, addCallback());
        }

        public Fido2User GetUser(string username)
        {
            _storedUsers.TryGetValue(username, out var user);
            return user;
        }

        public List<StoredCredential> GetCredentialsByUser(Fido2User user)
        {
            return _storedCredentials.Where(c => c.UserId.SequenceEqual(user.Id)).ToList();
        }

        public StoredCredential GetCredentialById(byte[] id)
        {
            return _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(id)).FirstOrDefault();
        }

        public List<StoredCredential> GetCredentialsByUserHandle(byte[] userHandle)
        {
            return _storedCredentials.Where(c => c.UserHandle.SequenceEqual(userHandle)).ToList();
        }

        public void UpdateCounter(byte[] credentialId, uint counter)
        {
            var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();
            cred.SignatureCounter = counter;
        }

        public void AddCredentialToUser(Fido2User user, StoredCredential credential)
        {
            credential.UserId = user.Id;
            _storedCredentials.Add(credential);
        }

        public void RemoveCredentialToUser(Fido2User user, string aaguid)
        {
            _storedCredentials.RemoveAll(c => c.UserId.SequenceEqual(user.Id) && c.AaGuid.ToString().Equals(aaguid));
        }

        public List<Fido2User> GetUsersByCredentialId(byte[] credentialId)
        {
            // our in-mem storage does not allow storing multiple users for a given credentialId. Yours shouldn't either.
            var cred = _storedCredentials.Where(c => c.Descriptor.Id.SequenceEqual(credentialId)).FirstOrDefault();

            if (cred == null)
                return new List<Fido2User>();

            return _storedUsers.Where(u => u.Value.Id.SequenceEqual(cred.UserId)).Select(u => u.Value).ToList();
        }
    }

    public class StoredCredential
    {
        public byte[] UserId { get; set; }
        public PublicKeyCredentialDescriptor Descriptor { get; set; }
        public byte[] PublicKey { get; set; }
        public byte[] UserHandle { get; set; }
        public uint SignatureCounter { get; set; }
        public string CredType { get; set; }
        public DateTime RegDate { get; set; }
        public Guid AaGuid { get; set; }
    }
}
