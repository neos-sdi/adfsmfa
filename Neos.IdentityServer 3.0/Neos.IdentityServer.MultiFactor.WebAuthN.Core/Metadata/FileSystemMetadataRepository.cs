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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Neos.IdentityServer.MultiFactor.WebAuthN.Metadata
{
    public class FileSystemMetadataRepository : IMetadataRepository
    {
        protected readonly string _path;

        protected readonly IDictionary<Guid, MetadataTOCPayloadEntry> _entries;
        protected MetadataTOCPayload _toc;

        public FileSystemMetadataRepository(string path)
        {
            _path = path;
            _entries = new Dictionary<Guid, MetadataTOCPayloadEntry>();
        }

        public async Task<MetadataStatement> GetMetadataStatement(MetadataTOCPayloadEntry entry)
        {
            if (_toc == null)
                await GetToc();

            if (!string.IsNullOrEmpty(entry.AaGuid) && Guid.TryParse(entry.AaGuid, out Guid parsedAaGuid))
            {
                if (_entries.ContainsKey(parsedAaGuid))
                    return _entries[parsedAaGuid].MetadataStatement;
            }

            return null;
        }

        public Task<MetadataTOCPayload> GetToc()
        {
            if (Directory.Exists(_path))
            {
                foreach (var filename in Directory.GetFiles(_path))
                {
                    var rawStatement = File.ReadAllText(filename);
                    var statement = JsonConvert.DeserializeObject<MetadataStatement>(rawStatement);
                    var conformanceEntry = new MetadataTOCPayloadEntry
                    {
                        AaGuid = statement.AaGuid,
                        MetadataStatement = statement,
                        StatusReports = new StatusReport[] 
                        { 
                            new StatusReport 
                            { 
                                Status = AuthenticatorStatus.NOT_FIDO_CERTIFIED 
                            } 
                        }
                    };
                    if (null != conformanceEntry.AaGuid) _entries.Add(new Guid(conformanceEntry.AaGuid), conformanceEntry);
                }
            }

            _toc = new MetadataTOCPayload()
            {
                Entries = _entries.Select(o => o.Value).ToArray(),
                NextUpdate = "", //Empty means it won't get cached
                LegalHeader = "Local FAKE",
                Number = 1
            };

            return Task.FromResult(_toc);
        }
    }
}
