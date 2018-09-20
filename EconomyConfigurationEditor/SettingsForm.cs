using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EconomyConfigurationEditor
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public string SEBinPath { get; set; }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            txtSEBinPath.Text = SEBinPath;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select Keen Bin folder",
                SelectedPath = txtSEBinPath.Text,
                ShowNewFolderButton = false
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtSEBinPath.Text = dialog.SelectedPath;
            }

        }

        private void btnOkay_Click(object sender, EventArgs e)
        {
            SEBinPath = txtSEBinPath.Text;
        }

    }
}
