//******************************************************************************************************************************************************************************************//
// Copyright (c) 2020 Neos-Sdi (http://www.neos-sdi.com)                                                                                                                                    //                        
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
using System.Reflection;

namespace Neos.IdentityServer.MultiFactor
{
    public static class CustomDataRepositoryCreator
    {
        /// <summary>
        /// CreateInstance method implementation
        /// </summary>
        public static DataRepositoryService CreateDataRepositoryInstance(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(host.DataRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(ParseType(host.DataRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(DataRepositoryService)))
                    if (_typetoload.IsClass && !_typetoload.IsAbstract && _typetoload.GetInterface("IWebAuthNDataRepositoryService") != null)
                        return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as DataRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// CreateKeyRepositoryInstance method implementation
        /// </summary>
        public static KeysRepositoryService CreateKeyRepositoryInstance(CustomStoreHost host, int deliverywindow)
        {
            try
            {
                Assembly assembly = Assembly.Load(ParseAssembly(host.KeysRepositoryFullyQualifiedImplementation));
                Type _typetoload = assembly.GetType(ParseType(host.KeysRepositoryFullyQualifiedImplementation));
                Type _ancestor = _typetoload.BaseType;
                if (_ancestor.IsClass && _ancestor.IsAbstract && (_ancestor == typeof(KeysRepositoryService)))
                   return (Activator.CreateInstance(_typetoload, new object[] { host, deliverywindow }) as KeysRepositoryService);
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseAssembly(string AssemblyFulldescription)
        {
            int cnt = AssemblyFulldescription.IndexOf(',');
            return AssemblyFulldescription.Remove(0, cnt).TrimStart(new char[] { ',', ' ' });
        }

        /// <summary>
        /// ParseType method implmentation
        /// </summary>
        private static string ParseType(string AssemblyFulldescription)
        {
            string[] type = AssemblyFulldescription.Split(new char[] { ',' });
            return type[0];
        }
    }
}
