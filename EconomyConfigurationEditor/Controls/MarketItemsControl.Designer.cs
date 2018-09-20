namespace EconomyConfigurationEditor.Controls
{
    partial class MarketItemsControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.colTypeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSubtypeName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisplayName = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSellPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBuyPrice = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIsBlacklisted = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTypeId,
            this.colSubtypeName,
            this.colDisplayName,
            this.colQuantity,
            this.colSellPrice,
            this.colBuyPrice,
            this.colIsBlacklisted});
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 0);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(586, 436);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // colTypeId
            // 
            this.colTypeId.DataPropertyName = "TypeId";
            this.colTypeId.HeaderText = "Type Id";
            this.colTypeId.Name = "colTypeId";
            this.colTypeId.ReadOnly = true;
            // 
            // colSubtypeName
            // 
            this.colSubtypeName.DataPropertyName = "SubtypeName";
            this.colSubtypeName.HeaderText = "Subtype Name";
            this.colSubtypeName.Name = "colSubtypeName";
            this.colSubtypeName.ReadOnly = true;
            // 
            // colDisplayName
            // 
            this.colDisplayName.DataPropertyName = "DisplayName";
            this.colDisplayName.HeaderText = "Display Name";
            this.colDisplayName.Name = "colDisplayName";
            this.colDisplayName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDisplayName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // colQuantity
            // 
            this.colQuantity.DataPropertyName = "Quantity";
            this.colQuantity.HeaderText = "Quantity";
            this.colQuantity.Name = "colQuantity";
            // 
            // colSellPrice
            // 
            this.colSellPrice.DataPropertyName = "SellPrice";
            this.colSellPrice.HeaderText = "Sell Price";
            this.colSellPrice.Name = "colSellPrice";
            // 
            // colBuyPrice
            // 
            this.colBuyPrice.DataPropertyName = "BuyPrice";
            this.colBuyPrice.HeaderText = "Buy Price";
            this.colBuyPrice.Name = "colBuyPrice";
            // 
            // colIsBlacklisted
            // 
            this.colIsBlacklisted.DataPropertyName = "IsBlacklisted";
            this.colIsBlacklisted.HeaderText = "Is Blacklisted";
            this.colIsBlacklisted.Name = "colIsBlacklisted";
            this.colIsBlacklisted.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colIsBlacklisted.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // MarketItemsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.dataGridView1);
            this.Name = "MarketItemsControl";
            this.Size = new System.Drawing.Size(586, 436);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTypeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSubtypeName;
        private System.Windows.Forms.DataGridViewComboBoxColumn colDisplayName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSellPrice;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBuyPrice;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colIsBlacklisted;
    }
}
