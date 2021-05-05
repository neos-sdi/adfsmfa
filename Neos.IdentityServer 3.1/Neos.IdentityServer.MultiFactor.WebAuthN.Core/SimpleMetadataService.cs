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
// Copyright (c) 2020 @redhook62 (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Neos.IdentityServer.MultiFactor.WebAuthN.Metadata;
using System.Diagnostics;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class SimpleMetadataService : IMetadataService
    {

        protected readonly List<IMetadataRepository> _repositories;
        protected readonly ConcurrentDictionary<Guid, MetadataStatement> _metadataStatements;
        protected readonly ConcurrentDictionary<Guid, MetadataTOCPayloadEntry> _entries;
        protected bool _initialized = true;

        public SimpleMetadataService(IEnumerable<IMetadataRepository> repositories)
        {
            _repositories = repositories.ToList();
            _metadataStatements = new ConcurrentDictionary<Guid, MetadataStatement>();
            _entries = new ConcurrentDictionary<Guid, MetadataTOCPayloadEntry>();
        }

        public bool ConformanceTesting()
        {
            return _repositories.First().GetType() == typeof(ConformanceMetadataRepository);
        }

        public MetadataTOCPayloadEntry GetEntry(Guid aaguid)
        {
            if (!IsInitialized())
                throw new InvalidOperationException("MetadataService must be initialized");

            if (_entries.ContainsKey(aaguid))
            {
                var entry = _entries[aaguid];

                if (_metadataStatements.ContainsKey(aaguid))
                {
                    if (entry.Hash != _metadataStatements[aaguid].Hash)
                        throw new VerificationException("Authenticator metadata statement has invalid hash");
                    entry.MetadataStatement = _metadataStatements[aaguid];
                }

                return entry;
            }
            else
            {
                return null;
            }
        }

        protected virtual async Task LoadEntryStatement(IMetadataRepository repository, MetadataTOCPayloadEntry entry)
        {
            if (entry.AaGuid != null)
            {
                var statement = await repository.GetMetadataStatement(entry);

                if (!string.IsNullOrWhiteSpace(statement.AaGuid))
                {
                    _metadataStatements.TryAdd(Guid.Parse(statement.AaGuid), statement);

                    // TODO : This seems undone
                    var statementJson = JsonConvert.SerializeObject(statement, Formatting.Indented);
                }
            }
        }

        protected virtual async Task InitializeClient(IMetadataRepository repository)
        {
            Trace.WriteLine("SimpleMetadataService InitializeClient Start");
            var toc = await repository.GetToc();

            foreach (var entry in toc.Entries)
            {
                if (!string.IsNullOrEmpty(entry.AaGuid))
                {
                    if (_entries.TryAdd(Guid.Parse(entry.AaGuid), entry))
                    {
                        //Load if it doesn't already exist
                        await LoadEntryStatement(repository, entry);
                    }
                }
            }
            Trace.WriteLine("SimpleMetadataService InitializeClient End");
        }

        public virtual async Task Initialize()
        {
            Trace.WriteLine("IMetadataService Initialize");
            _initialized = true;
            foreach (var client in _repositories)
            {
                await InitializeClient(client);
            }
            Trace.WriteLine("IMetadataService Initialized");
        }

        public virtual bool IsInitialized()
        {
            return _initialized;
        }
    }
}
