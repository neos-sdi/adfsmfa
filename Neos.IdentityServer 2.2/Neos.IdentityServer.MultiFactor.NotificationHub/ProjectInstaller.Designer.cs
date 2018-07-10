namespace Neos.IdentityServer.MultiFactor
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.HubInstaller = new System.ServiceProcess.ServiceInstaller();
            this.HubProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            // 
            // HubInstaller
            // 
            this.HubInstaller.Description = "MFA Notification Hub, manage notifications between components";
            this.HubInstaller.DisplayName = "MFA Notification Hub";
            this.HubInstaller.ServiceName = "mfanotifhub";
            this.HubInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.HubInstaller.Committed += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_Committed);
            this.HubInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_AfterInstall);
            this.HubInstaller.AfterRollback += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_AfterRollback);
            this.HubInstaller.AfterUninstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_AfterUninstall);
            this.HubInstaller.Committing += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_Committing);
            this.HubInstaller.BeforeInstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_BeforeInstall);
            this.HubInstaller.BeforeRollback += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_BeforeRollback);
            this.HubInstaller.BeforeUninstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_BeforeUninstall);
            // 
            // HubProcessInstaller
            // 
            this.HubProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.HubProcessInstaller.Password = null;
            this.HubProcessInstaller.Username = null;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.HubInstaller,
            this.HubProcessInstaller});
            this.Committed += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_Committed);
            this.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_AfterInstall);
            this.AfterRollback += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_AfterRollback);
            this.AfterUninstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_AfterUninstall);
            this.Committing += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_Committing);
            this.BeforeInstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_BeforeInstall);
            this.BeforeRollback += new System.Configuration.Install.InstallEventHandler(this.HubInstaller_BeforeRollback);
            this.BeforeUninstall += new System.Configuration.Install.InstallEventHandler(this.ProjectInstaller_BeforeUninstall);

        }

        #endregion

        private System.ServiceProcess.ServiceInstaller HubInstaller;
        private System.ServiceProcess.ServiceProcessInstaller HubProcessInstaller;
    }
}