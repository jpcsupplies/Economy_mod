using Economy.scripts;
using Economy.scripts.EconStructures;
using EconomyConfigurationEditor.Controls;
using EconomyConfigurationEditor.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EconomyConfigurationEditor
{
    public partial class ConfigurationEditorForm : Form
    {
        SandboxManager sandboxManager = new SandboxManager();
        EconomyManagerAlt economyManager = new EconomyManagerAlt();

        public ConfigurationEditorForm()
        {
            InitializeComponent();
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
            SettingsForm form = new SettingsForm();
            form.SEBinPath = GlobalSettings.Default.SEBinPath;

            if (form.ShowDialog() == DialogResult.OK)
            {
                GlobalSettings.Default.SEBinPath = form.SEBinPath;
                GlobalSettings.Default.Save();
            }
        }

        private void ConfigurationEditorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalSettings.Default.Save();
        }

        private void mnuOpenSave_Click(object sender, EventArgs e)
        {
            string userDataPath = Path.Combine(Environment.GetEnvironmentVariable("Appdata"), SpaceEngineersConsts.BasePathName);

            var dialog = new OpenFileDialog
            {
                AddExtension = false,
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = null,
                FileName = SpaceEngineersConsts.SandBoxCheckpointFilename,
                Filter = $"Save Games ({SpaceEngineersConsts.SandBoxCheckpointFilename})|{SpaceEngineersConsts.SandBoxCheckpointFilename}",
                InitialDirectory = Path.Combine(userDataPath, SpaceEngineersConsts.SavesFolder),
                Multiselect = false,
                Title = "Select save game",
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                tabControl.TabPages.Clear();

                string savePath = Path.GetDirectoryName(dialog.FileName);

                bool ret = sandboxManager.LoadGame(GlobalSettings.Default.SEBinPath, userDataPath, savePath);

                this.Cursor = null;
                if (!ret)
                {
                    MessageBox.Show(this, "Could not load save game.");
                    return;
                }

                if (!economyManager.LoadEconomy(savePath))
                {
                    MessageBox.Show(this, "Could not load economy data.");
                    return;
                }

                TabPage tab = new TabPage();
                MarketItemsControl child = new MarketItemsControl();
                child.LoadPrices(EconomyScript.Instance.ServerConfig);
                tab.Controls.Add(child);
                child.Dock = DockStyle.Fill;
                tab.Text = "Default Prices";
                tabControl.TabPages.Add(tab);

                foreach (MarketStruct market in EconomyScript.Instance.Data.Markets)
                {
                    tab = new TabPage();
                    child = new MarketItemsControl();
                    child.LoadPrices(market);
                    tab.Controls.Add(child);
                    child.Dock = DockStyle.Fill;
                    tab.Text = market.DisplayName;
                    tabControl.TabPages.Add(tab);
                }

                // TODO: display any other data?
            }
        }
    }
}
