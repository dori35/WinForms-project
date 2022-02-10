
namespace WarehouseSimulation
{
    partial class SimulationView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.menuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.startItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadItem = new System.Windows.Forms.ToolStripMenuItem();
            this.speedMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.slowItem = new System.Windows.Forms.ToolStripMenuItem();
            this.normalItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fastItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(48, 48);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem,
            this.speedMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 28);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // menuItem
            // 
            this.menuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.startItem,
            this.stopItem,
            this.loadItem});
            this.menuItem.Name = "menuItem";
            this.menuItem.Size = new System.Drawing.Size(60, 24);
            this.menuItem.Text = "Menu";
            // 
            // startItem
            // 
            this.startItem.Name = "startItem";
            this.startItem.Size = new System.Drawing.Size(125, 26);
            this.startItem.Text = "Start";
            // 
            // loadItem
            // 
            this.loadItem.Name = "loadItem";
            this.loadItem.Size = new System.Drawing.Size(125, 26);
            this.loadItem.Text = "Load";
            this.loadItem.Click += new System.EventHandler(this.loadFromFile);
            // 
            // speedMenuItem
            // 
            this.speedMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.slowItem,
            this.normalItem,
            this.fastItem});
            this.speedMenuItem.Name = "speedMenuItem";
            this.speedMenuItem.Size = new System.Drawing.Size(65, 24);
            this.speedMenuItem.Text = "Speed";
            // 
            // slowItem
            // 
            this.slowItem.Name = "slowItem";
            this.slowItem.Size = new System.Drawing.Size(142, 26);
            this.slowItem.Text = "Slow";
            this.slowItem.Click += new System.EventHandler(this.slowSpeed);
            // 
            // normalItem
            // 
            this.normalItem.Name = "normalItem";
            this.normalItem.Size = new System.Drawing.Size(142, 26);
            this.normalItem.Text = "Normal";
            this.normalItem.Click += new System.EventHandler(this.normalSpeed);
            // 
            // fastItem
            // 
            this.fastItem.Name = "fastItem";
            this.fastItem.Size = new System.Drawing.Size(142, 26);
            this.fastItem.Text = "Fast";
            this.fastItem.Click += new System.EventHandler(this.fastSpeed);
            // 
            // stopItem
            // 
            this.stopItem.Name = "stopItem";
            this.stopItem.Size = new System.Drawing.Size(125, 26);
            this.stopItem.Text = "Stop";
            // 
            // SimulationView
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SimulationView";
            this.Text = "WarehouseSimulation";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem Menu;
        private System.Windows.Forms.ToolStripMenuItem startItem;
        private System.Windows.Forms.ToolStripMenuItem loadItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem speedMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem;
        private System.Windows.Forms.ToolStripMenuItem slowItem;
        private System.Windows.Forms.ToolStripMenuItem normalItem;
        private System.Windows.Forms.ToolStripMenuItem fastItem;
        private System.Windows.Forms.ToolStripMenuItem stopItem;
    }
}

