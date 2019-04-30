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
using Microsoft.ManagementConsole;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Neos.IdentityServer.Console
{
    [RunInstaller(true)]
    public partial class SnapinInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SnapinInstaller()
        {
            InitializeComponent();
        }
        
            /// <summary>
            /// Install method override
            /// </summary>
            public override void Install(IDictionary stateSaver)
            {
                base.Install(stateSaver);
                var att = GetSettingsAttibutes();
                RegistryKey rkey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                try
                {
                    BuildSnapinRegistryEntries(rkey64, att);
                }
                finally
                {
                    rkey64.Close();
                }
                RegistryKey rkey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                try
                {
                    BuildSnapinRegistryEntries(rkey32, att);
                }
                finally
                {
                    rkey32.Close();
                }
            }

            /// <summary>
            /// UnInstall method override
            /// </summary>
            /// <param name="savedState"></param>
            public override void Uninstall(IDictionary savedState)
            {
                base.Uninstall(savedState);
                var att = GetSettingsAttibutes();
                RegistryKey rkey64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                try
                {
                    RemoveSnapinRegistryEntries(rkey64, att);
                }
                finally
                {
                    rkey64.Close();
                }
                RegistryKey rkey32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                try
                {
                    RemoveSnapinRegistryEntries(rkey32, att);
                }
                finally
                {
                    rkey32.Close();
                }
            }

            /// <summary>
            /// Rollback method override
            /// </summary>
            public override void Rollback(IDictionary savedState)
            {
                base.Rollback(savedState);
            }

            /// <summary>
            /// BuildSnapinRegistryEntries method implementation
            /// </summary>
            private void BuildSnapinRegistryEntries(RegistryKey root, List<SnapinAttributeSettings> attributes)
            {
                RegistryKey key = root.OpenSubKey(@"Software\Microsoft\MMC\Snapins", RegistryKeyPermissionCheck.ReadWriteSubTree);
                try
                {
                    foreach (SnapinAttributeSettings a in attributes)
                    {
                        RegistryKey xkey = key.OpenSubKey("FX:{" + a.Settings.Guid.ToString() + "}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                        try
                        {
                            if (xkey == null)
                            {
                                xkey = key.CreateSubKey("FX:{" + a.Settings.Guid.ToString() + "}", RegistryKeyPermissionCheck.ReadWriteSubTree);
                                xkey.CreateSubKey("NodeTypes");
                                xkey.CreateSubKey("Standalone");
                            }
                            xkey.SetValue("About", "{00000000-0000-0000-0000-000000000000}", RegistryValueKind.String);
                            xkey.SetValue("ApplicationBase", Path.GetDirectoryName(a.CodeBase) + Path.DirectorySeparatorChar, RegistryValueKind.ExpandString);
                            xkey.SetValue("AssemblyName", Path.GetFileNameWithoutExtension(a.CodeBase), RegistryValueKind.String);
                            xkey.SetValue("Description", a.Settings.Description, RegistryValueKind.String);
                            xkey.SetValue("FxVersion", "3.0.0.0", RegistryValueKind.String);
                            xkey.SetValue("NameString", a.Settings.DisplayName, RegistryValueKind.String);
                            xkey.SetValue("RuntimeVersion", "v" + Environment.Version.ToString(3), RegistryValueKind.String);
                            xkey.SetValue("Type", a.AssemblyQualifiedName, RegistryValueKind.String);
                            xkey.SetValue("UseCustomHelp", 1, RegistryValueKind.DWord);
                            if (!string.IsNullOrEmpty(a.Settings.Vendor))
                                xkey.SetValue("Provider", a.Settings.Vendor, RegistryValueKind.String);

                            string rootmodulename = string.Empty;
                            rootmodulename = Path.GetFileName(a.CodeBase);
                            xkey.SetValue("ModuleName", rootmodulename, RegistryValueKind.String);

                            string rootIndirect = "@"+Path.GetDirectoryName(a.CodeBase).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar + a.About.ResourceModule;

                            if (a.DescriptionId!=-1)
                                xkey.SetValue("DescriptionStringIndirect", rootIndirect + ",-" + a.DescriptionId.ToString(), RegistryValueKind.String);
                            if (a.IconId != -1)
                                xkey.SetValue("IconIndirect", rootIndirect + ",-" + a.IconId.ToString(), RegistryValueKind.String);
                            if (a.LargeFolderBitmapId != -1)
                                xkey.SetValue("LargeFolderBitmapIndirect", rootIndirect + ",-" + a.LargeFolderBitmapId.ToString(), RegistryValueKind.String);
                            if (a.SmallFolderBitmapId != -1)
                                xkey.SetValue("SmallFolderBitmapIndirect", rootIndirect + ",-" + a.SmallFolderBitmapId.ToString(), RegistryValueKind.String);
                            if (a.About.SmallFolderSelectedBitmapId != -1)
                                xkey.SetValue("SmallFolderSelectedBitmapIndirect", rootIndirect + ",-" + a.SmallFolderSelectedBitmapId.ToString(), RegistryValueKind.String);
                            if (a.DisplayNameId != -1)
                                xkey.SetValue("NameStringIndirect", rootIndirect + ",-" + a.DisplayNameId.ToString(), RegistryValueKind.String);
                            if (a.ProviderId != -1)
                                xkey.SetValue("ProviderStringIndirect", rootIndirect + ",-" + a.ProviderId.ToString(), RegistryValueKind.String);
                            if (a.VersionId != -1)
                                xkey.SetValue("VersionStringIndirect", rootIndirect + ",-" + a.VersionId.ToString(), RegistryValueKind.String);
                            xkey.Flush();
                        }
                        finally
                        {
                            xkey.Close();
                        }
                    }
                }
                finally
                {
                    key.Close();
                }
            }

            /// <summary>
            /// RemoveSnapinRegistryEntries method implmentation
            /// </summary>
            private void RemoveSnapinRegistryEntries(RegistryKey root, List<SnapinAttributeSettings> attributes)
            {
                RegistryKey key = root.OpenSubKey(@"Software\Microsoft\MMC\Snapins", RegistryKeyPermissionCheck.ReadWriteSubTree);
                try
                {
                    foreach (SnapinAttributeSettings a in attributes)
                    {
                        key.DeleteSubKeyTree("FX:{" + a.Settings.Guid.ToString() + "}", false);
                    }
                }
                finally
                {
                    key.Close();
                }
            }

            /// <summary>
            /// GetSettingsAttibutes method implementation
            /// </summary>
            private List<SnapinAttributeSettings> GetSettingsAttibutes()
            {
                List<SnapinAttributeSettings> lst = new List<SnapinAttributeSettings>();
                Assembly assembly = Assembly.GetExecutingAssembly();
                foreach (Type type in assembly.GetTypes())
                {
                    SnapinAttributeSettings att = new SnapinAttributeSettings();
                    if (type.IsDefined(typeof(SnapInSettingsAttribute)))
                    {
                        SnapInSettingsAttribute attribs = (SnapInSettingsAttribute)type.GetCustomAttribute(typeof(SnapInSettingsAttribute), false);
                        if (attribs != null)
                        {
                            att.Settings = attribs;
                            att.SnapinClass = type;
                            att.AssemblyQualifiedName = type.AssemblyQualifiedName;
                            att.FullName = type.FullName;
                            att.AssemblyName = type.Assembly.GetName().Name + ".dll";
                            att.CodeBase = new Uri(type.Assembly.CodeBase).LocalPath;
                            att.IsSet = true;
                        }
                    }
                    if (type.IsDefined(typeof(SnapInAboutAttribute)))
                    {
                        SnapInAboutAttribute attribs = (SnapInAboutAttribute)type.GetCustomAttribute(typeof(SnapInAboutAttribute), false);
                        if (attribs != null)
                        {
                            att.About = attribs;
                            att.ApplicationBaseRelative = attribs.ApplicationBaseRelative;
                            att.DescriptionId = attribs.DescriptionId;
                            att.DisplayNameId = attribs.DisplayNameId;
                            att.FolderBitmapsColorMask = attribs.FolderBitmapsColorMask;
                            att.IconId = attribs.IconId;
                            att.LargeFolderBitmapId = attribs.LargeFolderBitmapId;
                            att.SmallFolderBitmapId = attribs.SmallFolderBitmapId;
                            att.SmallFolderSelectedBitmapId = attribs.SmallFolderSelectedBitmapId;
                            att.ProviderId = attribs.VendorId;
                            att.VersionId = attribs.VersionId;
                        }
                    }
                    if (att.IsSet)
                        lst.Add(att);
                }
                return lst;
            }
        }

        public class SnapinAttributeSettings
        {
            public bool IsSet = false;

            public Type SnapinClass { get; set; }
            public string AssemblyQualifiedName { get; set; }
            public string FullName { get; set; }
            public string AssemblyName { get; set; }
            public string CodeBase { get; set; }

            public bool ApplicationBaseRelative { get; set; }
            public int DescriptionId { get; set; }
            public int DisplayNameId { get; set; }
            public int FolderBitmapsColorMask { get; set; }
            public int IconId { get; set; }
            public int LargeFolderBitmapId { get; set; }
            public int SmallFolderBitmapId { get; set; }
            public int SmallFolderSelectedBitmapId { get; set; }
            public int ProviderId { get; set; }
            public int VersionId { get; set; }

            public SnapInSettingsAttribute Settings;
            public SnapInAboutAttribute About;
        }

}
