
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
using Neos.IdentityServer.MultiFactor;
using System;
using System.ComponentModel;
using res = Neos.IdentityServer.Console.Resources.Neos_IdentityServer_Console_DataContracts;

namespace Neos.IdentityServer.Console
{
    /// <summary>
    /// IMMCRefreshData
    /// </summary>
    public interface IMMCRefreshData
    {
        void DoRefreshData();
    }

    /// <summary>
    /// IMMCNotificationData
    /// </summary>
    public interface IMMCNotificationData
    {
        bool IsNotifsEnabled();
    }

    /// <summary>
    /// MMCFilterFieldItem class
    /// </summary>
    public class MMCFilterFieldItem
    {
        public DataFilterField ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCFilterFieldList class implémentation
    /// </summary>
    public class MMCFilterFieldList : BindingList<MMCFilterFieldItem>
    {
        public MMCFilterFieldList()
        {
            this.Add(new MMCFilterFieldItem() { ID = DataFilterField.UserName, Label = res.MMCFILTERUSERNAME });
            this.Add(new MMCFilterFieldItem() { ID = DataFilterField.Email, Label = res.MMCFILTEREMAIL });
            this.Add(new MMCFilterFieldItem() { ID = DataFilterField.PhoneNumber, Label = res.MMCFILTERPHONE });
        }
    }

    /// <summary>
    /// MMCFilterOperatorItem class
    /// </summary>
    public class MMCFilterOperatorItem
    {
        public DataFilterOperator ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCFilterOperatorList class implémentation
    /// </summary>
    public class MMCFilterOperatorList : BindingList<MMCFilterOperatorItem>
    {
        public MMCFilterOperatorList()
        {
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.Equal, Label = res.MMCOPEQUAL });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.StartWith, Label = res.MMCOPSTARTWITH });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.Contains, Label = res.MMCOPCONTAINS });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.NotEqual, Label = res.MMCOPNOTEQUAL });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.EndsWith, Label = res.MMCOPENDSWITH });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.NotContains, Label = res.MMCOPNOTCONTAINS });
        }
    }

    /// <summary>
    /// MMCFilterOperatorRestrictedList class implémentation
    /// </summary>
    public class MMCFilterOperatorRestrictedList : BindingList<MMCFilterOperatorItem>
    {
        public MMCFilterOperatorRestrictedList()
        {
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.Equal, Label = res.MMCOPEQUAL });
            this.Add(new MMCFilterOperatorItem() { ID = DataFilterOperator.NotEqual, Label = res.MMCOPNOTEQUAL });
        }
    }

    /// <summary>
    /// MMCPreferredMethodItem class implémentation
    /// </summary>
    public class MMCPreferredMethodItem
    {
        public PreferredMethod ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCPreferredMethodList class implémentation
    /// </summary>
    public class MMCPreferredMethodList : BindingList<MMCPreferredMethodItem>
    {

        public MMCPreferredMethodList() : this(false) { }

        public MMCPreferredMethodList(bool allowNone)
        {
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.Choose, Label = res.MMCMETHCHOOSE });
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.Code  , Label = res.MMCMETHAPP });
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.Email , Label = res.MMCMETHEMAIL });
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.External , Label = res.MMCMETHPHONE });
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.Azure,  Label = res.MMCMETHAZURE });
            if (allowNone)
                this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.None, Label = res.MMCMETHNONE });
        }
    }

    /// <summary>
    /// MMCTemplateModeItem class
    /// </summary>
    public class MMCTemplateModeItem
    {
        public UserTemplateMode ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            UserTemplateMode paramobj = UserTemplateMode.Custom;
            if (obj is UserTemplateMode)
                paramobj = (UserTemplateMode)obj;
            else if (obj is MMCTemplateModeItem)
                paramobj = ((MMCTemplateModeItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// MMCTemplateModeList class implémentation
    /// </summary>
    public class MMCTemplateModeList : BindingList<MMCTemplateModeItem>
    {
        public MMCTemplateModeList()
        {
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Free, Label = "Free template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Open, Label = "Open template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Default, Label = "Default template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Mixed, Label = "Mixed template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Managed, Label = "Managed template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Strict, Label = "Strict template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Administrative, Label = "Administrative template" });
            this.Add(new MMCTemplateModeItem() { ID = UserTemplateMode.Custom, Label = "Custom template" });
        }
    }

    /// <summary>
    /// MMCSecurityFormatItem class implémentation
    /// </summary>
    public class MMCSecurityFormatItem
    {
        public SecretKeyFormat ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCPreferredMethodList class implémentation
    /// </summary>
    public class MMCSecurityFormatList : BindingList<MMCSecurityFormatItem>
    {
        public MMCSecurityFormatList()
        {
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.RNG, Label = "RNG (random key generator)" });
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.RSA, Label = "RSA (Certificate)" });
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.CUSTOM, Label = "RSA CUSTOM (Certificate per user)" });
        }
    }

    /// <summary>
    /// MMCSecurityFormatItem class implémentation
    /// </summary>
    public class MMCSecurityKeySizeItem
    {
        public KeySizeMode ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCPreferredMethodList class implémentation
    /// </summary>
    public class MMCSecurityKeySizeist : BindingList<MMCSecurityKeySizeItem>
    {
        public MMCSecurityKeySizeist()
        {
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySizeDefault, Label = "DEFAULT (1024 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize512,  Label = " 512 ( 512 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize1024, Label = "1024 (1024 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize2048, Label = "2048 (2048 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize128,  Label = " 128 ( 128 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize256,  Label = " 256 ( 256 bits length)" });
            this.Add(new MMCSecurityKeySizeItem() { ID = KeySizeMode.KeySize384,  Label = " 384 ( 384 bits length)" });
        }
    }

    /// <summary>
    /// MMCSecurityKeyGeneratorItem class implémentation
    /// </summary>
    public class  MMCSecurityKeyGeneratorItem
    {
        public KeyGeneratorMode ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCPreferredMethodList class implémentation
    /// </summary>
    public class MMCSecurityKeyGeneratorList : BindingList<MMCSecurityKeyGeneratorItem>
    {
        public MMCSecurityKeyGeneratorList()
        {
            this.Add(new MMCSecurityKeyGeneratorItem() { ID = KeyGeneratorMode.Guid, Label = "Guid" });
            this.Add(new MMCSecurityKeyGeneratorItem() { ID = KeyGeneratorMode.ClientSecret128, Label = "128 bits" });
            this.Add(new MMCSecurityKeyGeneratorItem() { ID = KeyGeneratorMode.ClientSecret256, Label = "256 bits" });
            this.Add(new MMCSecurityKeyGeneratorItem() { ID = KeyGeneratorMode.ClientSecret384, Label = "384 bits" });
            this.Add(new MMCSecurityKeyGeneratorItem() { ID = KeyGeneratorMode.ClientSecret512, Label = "512 bits" });
        }
    }

}