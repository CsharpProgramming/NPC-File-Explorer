namespace NPC_File_Browser
{
    partial class SideBarDriveControl
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
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.Icon = new FontAwesome.Sharp.IconPictureBox();
            this.LabelSize = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoSize = true;
            this.FileNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileNameLabel.ForeColor = System.Drawing.Color.White;
            this.FileNameLabel.Location = new System.Drawing.Point(35, 4);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(57, 17);
            this.FileNameLabel.TabIndex = 5;
            this.FileNameLabel.Text = "Drive C:";
            this.FileNameLabel.DoubleClick += new System.EventHandler(this.FileNameLabel_DoubleClick);
            // 
            // Icon
            // 
            this.Icon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Icon.BackColor = System.Drawing.Color.Transparent;
            this.Icon.IconChar = FontAwesome.Sharp.IconChar.Hdd;
            this.Icon.IconColor = System.Drawing.Color.White;
            this.Icon.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Icon.IconSize = 23;
            this.Icon.Location = new System.Drawing.Point(0, 1);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(30, 23);
            this.Icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Icon.TabIndex = 4;
            this.Icon.TabStop = false;
            // 
            // LabelSize
            // 
            this.LabelSize.AutoSize = true;
            this.LabelSize.Dock = System.Windows.Forms.DockStyle.Right;
            this.LabelSize.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelSize.ForeColor = System.Drawing.Color.White;
            this.LabelSize.Location = new System.Drawing.Point(175, 0);
            this.LabelSize.Name = "LabelSize";
            this.LabelSize.Padding = new System.Windows.Forms.Padding(0, 4, 4, 0);
            this.LabelSize.Size = new System.Drawing.Size(61, 21);
            this.LabelSize.TabIndex = 7;
            this.LabelSize.Text = "Drive C:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(3, 30);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(230, 17);
            this.pictureBox1.TabIndex = 8;
            this.pictureBox1.TabStop = false;
            // 
            // SideBarDriveControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.LabelSize);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this.Icon);
            this.Name = "SideBarDriveControl";
            this.Size = new System.Drawing.Size(236, 50);
            this.DoubleClick += new System.EventHandler(this.SideBarDriveControl_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FileNameLabel;
        private FontAwesome.Sharp.IconPictureBox Icon;
        private System.Windows.Forms.Label LabelSize;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
