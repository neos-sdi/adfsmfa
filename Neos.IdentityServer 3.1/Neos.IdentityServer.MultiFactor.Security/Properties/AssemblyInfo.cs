﻿//******************************************************************************************************************************************************************************************//
// Copyright (c) 2024 redhook (adfsmfa@gmail.com)                                                                                                                                    //                        
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
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Les informations générales relatives à un assembly dépendent de
// l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
// associées à un assembly.
[assembly: AssemblyTitle("Neos.IdentityServer.MultiFactor.Security")]
[assembly: AssemblyDescription("ADFS MFA Provider")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("neos-sdi")]
[assembly: AssemblyProduct("Neos.IdentityServer.MultiFactor.Security")]
[assembly: AssemblyCopyright("Copyright redhook © 2024")]
[assembly: AssemblyTrademark("neos-sdi")]
[assembly: AssemblyCulture("")]

// L'affectation de la valeur false à ComVisible rend les types invisibles dans cet assembly
// aux composants COM. Si vous devez accéder à un type dans cet assembly à partir de
// COM, affectez la valeur true à l'attribut ComVisible sur ce type.
[assembly: ComVisible(false)]

// Le GUID suivant est pour l'ID de la typelib si ce projet est exposé à COM
[assembly: Guid("9ec6d40a-4366-4066-bdfc-8af65bce7ead")]

// Les informations de version pour un assembly se composent des quatre valeurs suivantes :
//
//      Version principale
//      Version secondaire
//      Numéro de build
//      Révision
//
// Vous pouvez spécifier toutes les valeurs ou indiquer les numéros de build et de révision par défaut
// en utilisant '*', comme indiqué ci-dessous :
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.1.2411.0")]
[assembly: AssemblyInformationalVersion("3.0.0.0")]
[assembly: NeutralResourcesLanguage("en")]

// To Generate PubliKey 
//
// 1)
// sn -k FriendAssemblies.snk
//   Or 
// sn -k FriendAssemblies.pfx
//
// 2)
// sn -p FriendAssemblies.snk FriendAssemblies.publickey
//   Or
// sn -p FriendAssemblies.pfx FriendAssemblies.publickey
//
// 3)
// sn -tp FriendAssemblies.publickey 
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor.Data, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor.Common, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor.Administration, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor.NotificationHub, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]
[assembly: InternalsVisibleTo("Neos.IdentityServer.MultiFactor.WebAuthN.Core, PublicKey=0024000004800000940000000602000000240000525341310004000001000100136c66e4526621536817a4316e736760a2c6511c4ad8944be23e694c66bf6d91632687c3d52912d1dcb33bbf8970966dc85872649dbd11d6e326f8801f5748c162a9e0a0f0e5a5ccc05d9f5e40d75330a6fb3950dd4304c77f453b9cbbc36919de99e2b88aef1ba71bc5d4dc9d81243cd9f5fc6ede161ae639fd60e20bb264c0")]


