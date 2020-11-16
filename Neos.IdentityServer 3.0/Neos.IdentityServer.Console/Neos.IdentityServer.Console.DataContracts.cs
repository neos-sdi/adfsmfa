
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
            this.Add(new MMCPreferredMethodItem() { ID = PreferredMethod.Biometrics, Label = res.MMCMETHBIO });
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
    /// MMCReplayModeItem class
    /// </summary>
    public class MMCReplayModeItem
    {
        public ReplayLevel ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ReplayLevel paramobj = ReplayLevel.Disabled;
            if (obj is UserTemplateMode)
                paramobj = (ReplayLevel)obj;
            else if (obj is MMCReplayModeItem)
                paramobj = ((MMCReplayModeItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// MMCReplayModeList class implémentation
    /// </summary>
    public class MMCReplayModeList : BindingList<MMCReplayModeItem>
    {
        public MMCReplayModeList()
        {
            this.Add(new MMCReplayModeItem() { ID = ReplayLevel.Disabled, Label = "Disabled" });
            this.Add(new MMCReplayModeItem() { ID = ReplayLevel.Intermediate, Label = "Intermediate" });
            this.Add(new MMCReplayModeItem() { ID = ReplayLevel.Full, Label = "Full" });
        }
    }

    /// <summary>
    /// MMCChallengeSizeItem class
    /// </summary>
    public class MMCChallengeSizeItem
    {
        public int ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            int paramobj = 16;
            if (obj is MMCChallengeSizeItem)
                paramobj = ((MMCChallengeSizeItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }


    /// <summary>
    /// MMCReplayModeList class implémentation
    /// </summary>
    public class MMCChallengeSizeList : BindingList<MMCChallengeSizeItem>
    {
        public MMCChallengeSizeList()
        {
            this.Add(new MMCChallengeSizeItem() { ID = 16, Label = "128 bits (default)" });
            this.Add(new MMCChallengeSizeItem() { ID = 32, Label = "256 bits" });
            this.Add(new MMCChallengeSizeItem() { ID = 48, Label = "384 bits" });
            this.Add(new MMCChallengeSizeItem() { ID = 64, Label = "512 bits" });
        }
    }


    /// <summary>
    /// MMCTextualItem class
    /// </summary>
    public class MMCTextualItem
    {
        public string ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            string paramobj = string.Empty;
            if (obj is MMCTextualItem)
                paramobj = ((MMCTextualItem)obj).ID;
            else
                return false;
            if (paramobj.Equals(this.ID))
                return true;
            else
                return false;
        }
    }


    /// <summary>
    /// MMCAttachementList class implémentation
    /// </summary>
    public class MMCAttachementList : BindingList<MMCTextualItem>
    {
        public MMCAttachementList()
        {
            this.Add(new MMCTextualItem() { ID = string.Empty, Label = "Empty" });
            this.Add(new MMCTextualItem() { ID = "platform", Label = "Platform" });
            this.Add(new MMCTextualItem() { ID = "cross-platform", Label = "Cross-Platform" });
        }
    }

    /// <summary>
    /// MMCConveyancePreferenceList class implémentation
    /// </summary>
    public class MMCConveyancePreferenceList : BindingList<MMCTextualItem>
    {
        public MMCConveyancePreferenceList()
        {
            this.Add(new MMCTextualItem() { ID = "none", Label = "None" });
            this.Add(new MMCTextualItem() { ID = "direct", Label = "Direct" });
            this.Add(new MMCTextualItem() { ID = "indirect", Label = "Indirect" });
        }
    }

    /// <summary>
    /// MMCConveyancePreferenceList class implémentation
    /// </summary>
    public class MMCUserVerificationRequirementList : BindingList<MMCTextualItem>
    {
        public MMCUserVerificationRequirementList()
        {
            this.Add(new MMCTextualItem() { ID = "preferred", Label = "Preferred" });
            this.Add(new MMCTextualItem() { ID = "required", Label = "Required" });
            this.Add(new MMCTextualItem() { ID = "discouraged", Label = "Discouraged" });
        }
    }

    /// <summary>
    /// MMCLibVersionItem class
    /// </summary>
    public class MMCLibVersionItem
    {
        public SecretKeyVersion ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            SecretKeyVersion paramobj = SecretKeyVersion.V2;
            if (obj is MMCLibVersionItem)
                paramobj = ((MMCLibVersionItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }


    /// <summary>
    /// MMCReplayModeList class implémentation
    /// </summary>
    public class MMCLibVersionList : BindingList<MMCLibVersionItem>
    {
        public MMCLibVersionList()
        {
            this.Add(new MMCLibVersionItem() { ID = SecretKeyVersion.V1, Label = "V1" });
            this.Add(new MMCLibVersionItem() { ID = SecretKeyVersion.V2, Label = "V2" });
            // this.Add(new MMCLibVersionItem() { ID = SecretKeyVersion.V3, Label = "V3" });  // For No Future
        }
    }

    /// <summary>
    /// MMCProviderItem class
    /// </summary>
    public class MMCProviderItem
    {
        public PreferredMethod ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            PreferredMethod paramobj = PreferredMethod.Choose;
            if (obj is PreferredMethod)
                paramobj = (PreferredMethod)obj;
            else if (obj is MMCProviderItem)
                paramobj = ((MMCProviderItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// MMCProviersList class implémentation
    /// </summary>
    public class MMCProviersList : BindingList<MMCProviderItem>
    {
        public MMCProviersList()
        {
            this.Add(new MMCProviderItem() { ID = PreferredMethod.Choose, Label = "Choose" });
            this.Add(new MMCProviderItem() { ID = PreferredMethod.Code, Label = "Code" });
            this.Add(new MMCProviderItem() { ID = PreferredMethod.Email, Label = "Email" });
            this.Add(new MMCProviderItem() { ID = PreferredMethod.External, Label = "External" });
            this.Add(new MMCProviderItem() { ID = PreferredMethod.Azure, Label = "Azure" });
            this.Add(new MMCProviderItem() { ID = PreferredMethod.Biometrics, Label = "Biometrics" });
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
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.RNG, Label = "Encoded Keys RNG" });
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.RSA, Label = "Asymmetric Keys RSA (2048 bits)" });
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.AES, Label = "Symmetric Keys AES (1024 bits)" });
            this.Add(new MMCSecurityFormatItem() { ID = SecretKeyFormat.CUSTOM, Label = "Custom Keys" });
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
    /// MMCSecurityKeySizeList class implémentation
    /// </summary>
    public class MMCSecurityKeySizeList : BindingList<MMCSecurityKeySizeItem>
    {
        public MMCSecurityKeySizeList()
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
    /// MMCSecurityKeyGeneratorList class implémentation
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

    /// <summary>
    /// MMCAESSecurityKeyGeneratorItem class implémentation
    /// </summary>
    public class MMCAESSecurityKeyGeneratorItem
    {
        public AESKeyGeneratorMode ID { get; set; }
        public String Label { get; set; }
    }

    /// <summary>
    /// MMCSecurityKeyGeneratorList class implémentation
    /// </summary>
    public class MMCAESSecurityKeyGeneratorList : BindingList<MMCAESSecurityKeyGeneratorItem>
    {
        public MMCAESSecurityKeyGeneratorList()
        {
            this.Add(new MMCAESSecurityKeyGeneratorItem() { ID = AESKeyGeneratorMode.AESSecret512, Label = "512 bits" });
            this.Add(new MMCAESSecurityKeyGeneratorItem() { ID = AESKeyGeneratorMode.AESSecret1024, Label = "1024 bits" });
        }
    }

    /// <summary>
    /// MMCTOTPDurationItem class
    /// </summary>
    public class MMCTOTPDurationItem
    {
        public int ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            int paramobj = 30;
            if (obj is MMCTOTPDurationItem)
                paramobj = ((MMCTOTPDurationItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// MMCTOTPDurationList class implémentation
    /// </summary>
    public class MMCTOTPDurationList : BindingList<MMCTOTPDurationItem>
    {
        public MMCTOTPDurationList()
        {
            this.Add(new MMCTOTPDurationItem() { ID =  30, Label = "30 seconds (default)" });
            this.Add(new MMCTOTPDurationItem() { ID =  60, Label = "1 minute" });
            this.Add(new MMCTOTPDurationItem() { ID =  90, Label = "1 minute 30 seconds" });
            this.Add(new MMCTOTPDurationItem() { ID = 120, Label = "2 minutes" });
            this.Add(new MMCTOTPDurationItem() { ID = 150, Label = "2 minutes 30 seconds" });
            this.Add(new MMCTOTPDurationItem() { ID = 180, Label = "3 minutes" });
        }
    }

    /// <summary>
    /// MMCTOTPDurationItem class
    /// </summary>
    public class MMCTOTPDigitsItem
    {
        public int ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            int paramobj = 30;
            if (obj is MMCTOTPDigitsItem)
                paramobj = ((MMCTOTPDigitsItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// MMCTOTPDurationList class implémentation
    /// </summary>
    public class MMCTOTPDigitsList : BindingList<MMCTOTPDigitsItem>
    {
        public MMCTOTPDigitsList()
        {
            this.Add(new MMCTOTPDigitsItem() { ID = 4, Label = "4 digits" });
            this.Add(new MMCTOTPDigitsItem() { ID = 5, Label = "5 digits" });
            this.Add(new MMCTOTPDigitsItem() { ID = 6, Label = "6 digits (default)" });
            this.Add(new MMCTOTPDigitsItem() { ID = 7, Label = "7 digits" });
            this.Add(new MMCTOTPDigitsItem() { ID = 8, Label = "8 digits" });
        }
    }

    /// <summary>
    /// MMCTOTPHashModeList class implémentation
    /// </summary>
    public class MMCTOTPHashModeList : BindingList<MMCTOTPHashModeItem>
    {
        public MMCTOTPHashModeList()
        {
            this.Add(new MMCTOTPHashModeItem() { ID = HashMode.SHA1,   Label = " SHA1 " });
            this.Add(new MMCTOTPHashModeItem() { ID = HashMode.SHA256, Label = "SHA256" });
            this.Add(new MMCTOTPHashModeItem() { ID = HashMode.SHA384, Label = "SHA384" });
            this.Add(new MMCTOTPHashModeItem() { ID = HashMode.SHA512, Label = "SHA512" });
        }
    }

    /// <summary>
    /// MMCTOTPHashModeItem class
    /// </summary>
    public class MMCTOTPHashModeItem
    {
        public HashMode ID { get; set; }
        public String Label { get; set; }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            HashMode paramobj = HashMode.SHA1;
            if (obj is MMCTOTPHashModeItem)
                paramobj = ((MMCTOTPHashModeItem)obj).ID;
            else
                return false;
            if (paramobj == this.ID)
                return true;
            else
                return false;
        }
    }
}