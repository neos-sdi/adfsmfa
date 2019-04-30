using System.Diagnostics;
namespace Neos.IdentityServer.MultiFactor.NotificationHub
{
    partial class MFANOTIFHUB
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
                Trace.TraceInformation("Dispose");
                components.Dispose();
            }
            if (disposing)
            {
                Trace.TraceInformation("Disposing MailSlot Manager");
                _mailslotsmgr.Dispose();
            }
            Trace.TraceInformation("Base.Disposing");
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // MFANOTIFHUB
            // 
            this.AutoLog = false;
            this.ServiceName = "mfanotifhub";

        }

        #endregion
    }
}
