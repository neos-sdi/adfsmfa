using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace Neos.IdentityServer.MultiFactor.Common
{
    public abstract class ResourceLocaleBase
    {
        /// <summary>
        /// ResourceManager constructor
        /// </summary>
        public ResourceLocaleBase(int lcid)
        {
            try
            {
                Culture = new CultureInfo(lcid);
            }
            catch (CultureNotFoundException)
            {
                Culture = new CultureInfo("en");
            }
            catch (Exception)
            {
                Culture = new CultureInfo("en");
            }
            LoadResources();
        }

        /// <summary>
        /// Culture property implmentation
        /// </summary>
        public CultureInfo Culture { get; private set; }

        /// <summary>
        /// Culture property implmentation
        /// </summary>
        public Dictionary<ResourcesLocaleKind, ResourceManager> ResourcesList { get; } = new Dictionary<ResourcesLocaleKind, ResourceManager>();

        /// <summary>
        /// GetResourceManager method implmentation
        /// </summary>
        protected ResourceManager GetResourceManager(Assembly assenbly, string resourcename)
        {
            char sep = Path.DirectorySeparatorChar;
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + "." + Culture.Name + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + "." + Culture.TwoLetterISOLanguageName + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".en-us.resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".en.resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet" + sep + resourcename + ".resources"))
                return ResourceManager.CreateFileBasedResourceManager(resourcename, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + sep + "MFA" + sep + "ResourceSet", null);
            else
                return new ResourceManager(resourcename, assenbly);
        }

        /// <summary>
        /// LoadResources methoid declaration
        /// </summary>
        public abstract void LoadResources();

        /// <summary>
        /// GetString method implementation
        /// </summary>
        public virtual string GetString(ResourcesLocaleKind kind, string name)
        {
            try
            {
                ResourceManager mgr = this.ResourcesList[kind];
                if (mgr == null)
                    return string.Empty;
                return mgr.GetString(name, this.Culture);
            }
            catch (Exception ex)
            {
                Log.WriteEntry(string.Format("Resource {0} not found ! \r\n {1}", kind.ToString(), ex.Message), EventLogEntryType.Error, 5000);
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// ResourcesLocaleKind enum implementation
    /// </summary>
    public enum ResourcesLocaleKind
    {
        UITitles = 0,
        UIInformations = 1,
        UIErrors = 2,
        UIHtml = 3,
        UIValidation = 4,

        CommonHtml = 10,
        CommonErrors = 11,
        CommonMail = 12,

        AzureHtml = 20,

        SamplesHtml = 30,
        SamplesErrors = 31,
        SamplesMail = 32,

        SMSHtml = 40,
        SMSAzure = 41,

        FIDOHtml = 51,
        FIDOErrors = 52,

        CustomUITitles = 100,
        CustomUIInformations = 101,
        CustomUIErrors = 102,
        CustomUIHtml = 103,
        CustomUIValidation = 104,
        CustomUI = 110
    }
}

