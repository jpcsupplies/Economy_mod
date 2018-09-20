using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Economy.scripts.EconStructures;

namespace EconomyConfigurationEditor.Controls
{
    public partial class MarketItemsControl : UserControl
    {
        public MarketStruct ThisMarket;
        public EconConfigStruct ThisConfig;

        public MarketItemsControl()
        {
            InitializeComponent();
        }

        public void LoadPrices(EconConfigStruct serverConfig)
        {
            ThisConfig = serverConfig;

            dataGridView1.DataSource = serverConfig.DefaultPrices;

            foreach (MarketItemStruct item in serverConfig.DefaultPrices)
            {
                //DataGridViewRow row = new DataGridViewRow();
                //dataGridView1.Rows.Add(row);
                // TODO:

            //<TypeId>MyObjectBuilder_AmmoMagazine</TypeId>
            //<SubtypeName>NATO_5p56x45mm</SubtypeName>
            //<Quantity>1000</Quantity>
            //<SellPrice>2.35</SellPrice>
            //<BuyPrice>2.09</BuyPrice>
            //<IsBlacklisted>false</IsBlacklisted>
            }
        }

        public void LoadPrices(MarketStruct market)
        {
            ThisMarket = market;

            dataGridView1.DataSource = market.MarketItems;

            foreach (MarketItemStruct item in market.MarketItems)
            {
                //DataGridViewRow row = new DataGridViewRow();
                //dataGridView1.Rows.Add(row);
                // TODO:
            }

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
