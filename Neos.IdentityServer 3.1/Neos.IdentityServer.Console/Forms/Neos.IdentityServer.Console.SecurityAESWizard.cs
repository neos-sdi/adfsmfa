using Neos.IdentityServer.MultiFactor.Administration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neos.IdentityServer.Console
{
    public partial class ECHDWizard : Form
    {
        private int _choice = 0;
        private bool _keyexists = false;

        public int Choice { get => _choice; set => _choice = value; }
        public bool KeyExists { get => _keyexists; set => _keyexists = value; }

        /// <summary>
        /// ECHDWizard constructor
        /// </summary>
        public ECHDWizard()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Radio buttons CheckedChanged implementation
        /// </summary>
        private void ECDHKeys_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == this.rdioAddNewECDHKeys)
                _choice = 1;
            else if (sender == this.rdioDeployECDHKeys)
                _choice = 2;
            else if (sender == this.rdioDeleteECDHKeys)
                _choice = 3;
            else
                _choice = 1;
        }

        /// <summary>
        /// ECHDWizard_Load method implmentation
        /// </summary>
        private void ECHDWizard_Load(object sender, EventArgs e)
        {
            if (!KeyExists)
            {
                this.rdioAddNewECDHKeys.Enabled = true;
                this.rdioDeployECDHKeys.Enabled = false;
                this.rdioDeleteECDHKeys.Enabled = false;
            }
            else
            {
                this.rdioAddNewECDHKeys.Enabled = true;
                this.rdioDeployECDHKeys.Enabled = true;
                this.rdioDeleteECDHKeys.Enabled = true;
            }
        }
    }
}
