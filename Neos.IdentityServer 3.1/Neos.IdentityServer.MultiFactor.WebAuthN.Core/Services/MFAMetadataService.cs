//******************************************************************************************************************************************************************************************//
// Copyright (c) 2021 abergs (https://github.com/abergs/fido2-net-lib)                                                                                                                      //                        
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
// Copyright (c) 2023 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.WebAuthN
{
    public class MFAMetadataService : IMetadataService
    {
        protected readonly List<IMetadataRepository> _repositories;
        protected readonly ConcurrentDictionary<Guid, MetadataStatement> _metadataStatements;
        protected readonly ConcurrentDictionary<Guid, MetadataBLOBPayloadEntry> _entries;
        protected bool _initialized;

        /// <summary>
        /// MFAMetadataService constructor
        /// </summary>
        public MFAMetadataService(IEnumerable<IMetadataRepository> repositories)
        {
            _repositories = repositories.ToList();
            _metadataStatements = new ConcurrentDictionary<Guid, MetadataStatement>();
            _entries = new ConcurrentDictionary<Guid, MetadataBLOBPayloadEntry>();
        }

        public uint Timeout { get; set; }
        public int TimestampDriftTolerance { get; set; }

        public bool NeedToReload { get; set; }

        #region Initialization
        /// <summary>
        /// InitializeRepository method implmentation
        /// </summary>
        protected virtual void InitializeRepository(IMetadataRepository repository)
        {
            try
            {
                var blob = repository.GetBLOB();
                foreach (var entry in blob.Entries)
                {
                    if (!string.IsNullOrEmpty(entry.AaGuid))
                    {
                        if (_entries.TryAdd(Guid.Parse(entry.AaGuid), entry))
                        {
                            //Load if it doesn't already exist
                            LoadEntryStatement(repository, blob, entry);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Initializing WebAuthN Metdata Repository : {0} /// {1}", ex.Message, ex.StackTrace), EventLogEntryType.Error, 2000);
                repository.IsInitialized = false;
            }
        }

        /// <summary>
        /// Initialize method implementation
        /// </summary>
        public virtual void Initialize()
        {
            foreach (var repository in _repositories)
            {
                InitializeRepository(repository);
                if (repository.NeedToReload)
                {
                    NeedToReload = true;
                    repository.NeedToReload = false;
                }
            }
            for (int i = _repositories.Count-1; i >=0; i--)
            {
                if (!_repositories[i].IsInitialized) // Remove repositories that are not well initialized
                    _repositories.Remove(_repositories[i]);
            }
            _initialized = true;
        }

        /// <summary>
        /// IsInitialized method implementation
        /// </summary>
        public virtual bool IsInitialized()
        {
            return _initialized;
        }
        #endregion

        /// <summary>
        /// ConformanceTesting method implmentation
        /// </summary>
        public bool ConformanceTesting()
        {
            return false;
        }

        /// <summary>
        /// GetEntry method implmentation
        /// </summary>
        public MetadataBLOBPayloadEntry GetEntry(Guid aaguid)
        {
            try
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
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Retreiving WebAuthN Metdata Repository Entry : {0} /// {1}", ex.Message, ex.StackTrace), EventLogEntryType.Error, 2001);
                throw ex;
            }
        }

        /// <summary>
        /// LoadEntryStatement method implmentation
        /// </summary>
        protected virtual void LoadEntryStatement(IMetadataRepository repository, MetadataBLOBPayload blob, MetadataBLOBPayloadEntry entry)
        {
            try
            {
                if (entry.AaGuid != null)
                {
                    var statement = repository.GetMetadataStatement(blob, entry);

                    if (!string.IsNullOrWhiteSpace(statement.AaGuid))
                    {
                        _metadataStatements.TryAdd(Guid.Parse(statement.AaGuid), statement);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Error Retreiving WebAuthN Metdata Repository Statement : {0} /// {1}", ex.Message, ex.StackTrace), EventLogEntryType.Error, 2002);
                throw ex;
            }
        }
    }
}
