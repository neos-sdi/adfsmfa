using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console.Forms
{
    public partial class AdminPinWizard : Form
    {
        private ComponentResourceManager resources;
        public string AdminPin { get; set; }
        public string ErrorMessage { get; set; }

        public AdminPinWizard()
        {
            resources = new ComponentResourceManager(typeof(AdminPinWizard));
            InitializeComponent();            
        }

        /// <summary>
        /// AdminPinWizard_Validating method implementation
        /// </summary>
        private void AdminPinWizard_Validating(object sender, CancelEventArgs e)
        {
            if ((string.IsNullOrEmpty(textBoxPin.Text)) || (string.IsNullOrWhiteSpace(textBoxPin.Text)))
            {
                e.Cancel = true;
                errors.SetError(textBoxPin, resources.GetString("PINWWIZPASSREQUIRED"));
            }
        }

        /// <summary>
        /// AdminPinWizard_Validated method implementation
        /// </summary>
        private void AdminPinWizard_Validated(object sender, EventArgs e)
        {
            errors.SetError(textBoxPin, "");
        }

        /// <summary>
        /// buttonOK_Click method implmentation
        /// </summary>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (!this.ValidateChildren())
                this.DialogResult = DialogResult.None;
            else
            {
                if (Convert.ToInt32(this.AdminPin) == Convert.ToInt32(this.textBoxPin.Text))
                    this.DialogResult = DialogResult.OK;
                else
                {
                    ErrorMessage = resources.GetString("PINWWIZBADPASS");
                    this.DialogResult = DialogResult.Abort;
                }
            }
        }
    }
}
