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
    public partial class InitializationWizard : Form
    {
        public InitializationWizard()
        {
            InitializeComponent();
            this.SelectedSnapInName = "Unknown";
        }


        /// <summary>
        /// Gets the entered name.
        /// </summary>
        public string SelectedSnapInName
        {
            get
            {
                return SnapInName.Text;
            }
            set
            {
                SnapInName.Text = value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
