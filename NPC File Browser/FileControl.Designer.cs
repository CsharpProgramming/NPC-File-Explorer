using System.Windows.Forms;

namespace NPC_File_Browser
{
    partial class FileControl
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
            this.Icon = new FontAwesome.Sharp.IconPictureBox();
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.FileExtensionLabel = new System.Windows.Forms.Label();
            this.FileSizeLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).BeginInit();
            this.SuspendLayout();
            // 
            // Icon
            // 
            this.Icon.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.Icon.BackColor = System.Drawing.Color.Transparent;
            this.Icon.IconChar = FontAwesome.Sharp.IconChar.File;
            this.Icon.IconColor = System.Drawing.Color.White;
            this.Icon.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.Icon.IconSize = 23;
            this.Icon.Location = new System.Drawing.Point(0, 1);
            this.Icon.Name = "Icon";
            this.Icon.Size = new System.Drawing.Size(30, 23);
            this.Icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.Icon.TabIndex = 0;
            this.Icon.TabStop = false;
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoSize = true;
            this.FileNameLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileNameLabel.ForeColor = System.Drawing.Color.White;
            this.FileNameLabel.Location = new System.Drawing.Point(35, 4);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(44, 17);
            this.FileNameLabel.TabIndex = 1;
            this.FileNameLabel.Text = "Name";
            // 
            // FileExtensionLabel
            // 
            this.FileExtensionLabel.AutoSize = true;
            this.FileExtensionLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileExtensionLabel.ForeColor = System.Drawing.Color.White;
            this.FileExtensionLabel.Location = new System.Drawing.Point(433, 4);
            this.FileExtensionLabel.Name = "FileExtensionLabel";
            this.FileExtensionLabel.Size = new System.Drawing.Size(68, 17);
            this.FileExtensionLabel.TabIndex = 2;
            this.FileExtensionLabel.Text = "Extension";
            // 
            // FileSizeLabel
            // 
            this.FileSizeLabel.AutoSize = true;
            this.FileSizeLabel.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FileSizeLabel.ForeColor = System.Drawing.Color.White;
            this.FileSizeLabel.Location = new System.Drawing.Point(530, 4);
            this.FileSizeLabel.Name = "FileSizeLabel";
            this.FileSizeLabel.Size = new System.Drawing.Size(32, 17);
            this.FileSizeLabel.TabIndex = 3;
            this.FileSizeLabel.Text = "Size";
            // 
            // FileControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.FileSizeLabel);
            this.Controls.Add(this.FileExtensionLabel);
            this.Controls.Add(this.FileNameLabel);
            this.Controls.Add(this.Icon);
            this.Name = "FileControl";
            this.Size = new System.Drawing.Size(609, 25);
            this.Click += new System.EventHandler(this.FileControl_Click);
            this.DoubleClick += new System.EventHandler(this.FileControl_DoubleClick);
            ((System.ComponentModel.ISupportInitialize)(this.Icon)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private FontAwesome.Sharp.IconPictureBox Icon;
        private System.Windows.Forms.Label FileNameLabel;
        private System.Windows.Forms.Label FileExtensionLabel;
        private System.Windows.Forms.Label FileSizeLabel;
    }
}
