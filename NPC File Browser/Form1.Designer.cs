﻿namespace NPC_File_Browser
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.SidebarPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.PathTextbox = new ModernUI.Controls.ModernTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel7 = new System.Windows.Forms.Panel();
            this.panel8 = new System.Windows.Forms.Panel();
            this.iconPictureBox2 = new FontAwesome.Sharp.IconPictureBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.iconPictureBox1 = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonRefresh = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonRename = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonStar = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonCut = new FontAwesome.Sharp.IconPictureBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.ButtonDelete = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonPaste = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonCopy = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonForward = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonBack = new FontAwesome.Sharp.IconPictureBox();
            this.ButtonReturn = new FontAwesome.Sharp.IconPictureBox();
            this.ContentPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox2)).BeginInit();
            this.panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonRefresh)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonRename)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonStar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonDelete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonPaste)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCopy)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonForward)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonBack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonReturn)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panel1.Controls.Add(this.SidebarPanel);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(250, 652);
            this.panel1.TabIndex = 0;
            // 
            // SidebarPanel
            // 
            this.SidebarPanel.Location = new System.Drawing.Point(7, 29);
            this.SidebarPanel.Name = "SidebarPanel";
            this.SidebarPanel.Size = new System.Drawing.Size(236, 611);
            this.SidebarPanel.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.panel2.Controls.Add(this.PathTextbox);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1234, 60);
            this.panel2.TabIndex = 1;
            // 
            // PathTextbox
            // 
            this.PathTextbox.BackColor = System.Drawing.Color.Transparent;
            this.PathTextbox.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.PathTextbox.BorderColor = System.Drawing.Color.Transparent;
            this.PathTextbox.BorderRadius = 10;
            this.PathTextbox.Location = new System.Drawing.Point(260, 12);
            this.PathTextbox.Multiline = false;
            this.PathTextbox.Name = "PathTextbox";
            this.PathTextbox.Padding = new System.Windows.Forms.Padding(10);
            this.PathTextbox.PlaceholderColor = System.Drawing.Color.DimGray;
            this.PathTextbox.PlaceholderText = "Path";
            this.PathTextbox.Size = new System.Drawing.Size(729, 36);
            this.PathTextbox.TabIndex = 0;
            this.PathTextbox.TextBoxText = "";
            this.PathTextbox.TextColor = System.Drawing.Color.White;
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.panel3.Controls.Add(this.panel7);
            this.panel3.Controls.Add(this.panel5);
            this.panel3.Controls.Add(this.ButtonRefresh);
            this.panel3.Controls.Add(this.ButtonRename);
            this.panel3.Controls.Add(this.ButtonStar);
            this.panel3.Controls.Add(this.ButtonCut);
            this.panel3.Controls.Add(this.panel4);
            this.panel3.Controls.Add(this.ButtonDelete);
            this.panel3.Controls.Add(this.ButtonPaste);
            this.panel3.Controls.Add(this.ButtonCopy);
            this.panel3.Controls.Add(this.ButtonForward);
            this.panel3.Controls.Add(this.ButtonBack);
            this.panel3.Controls.Add(this.ButtonReturn);
            this.panel3.Location = new System.Drawing.Point(250, 60);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(984, 40);
            this.panel3.TabIndex = 2;
            // 
            // panel7
            // 
            this.panel7.BackColor = System.Drawing.Color.White;
            this.panel7.Controls.Add(this.panel8);
            this.panel7.Controls.Add(this.iconPictureBox2);
            this.panel7.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel7.Location = new System.Drawing.Point(87, 6);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(1, 28);
            this.panel7.TabIndex = 16;
            // 
            // panel8
            // 
            this.panel8.BackColor = System.Drawing.Color.White;
            this.panel8.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel8.Location = new System.Drawing.Point(63, 0);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(1, 28);
            this.panel8.TabIndex = 13;
            // 
            // iconPictureBox2
            // 
            this.iconPictureBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.iconPictureBox2.IconChar = FontAwesome.Sharp.IconChar.Star;
            this.iconPictureBox2.IconColor = System.Drawing.Color.White;
            this.iconPictureBox2.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconPictureBox2.IconSize = 23;
            this.iconPictureBox2.Location = new System.Drawing.Point(73, 0);
            this.iconPictureBox2.Name = "iconPictureBox2";
            this.iconPictureBox2.Size = new System.Drawing.Size(23, 28);
            this.iconPictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconPictureBox2.TabIndex = 12;
            this.iconPictureBox2.TabStop = false;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.White;
            this.panel5.Controls.Add(this.panel6);
            this.panel5.Controls.Add(this.iconPictureBox1);
            this.panel5.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel5.Location = new System.Drawing.Point(241, 6);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1, 28);
            this.panel5.TabIndex = 11;
            // 
            // panel6
            // 
            this.panel6.BackColor = System.Drawing.Color.White;
            this.panel6.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel6.Location = new System.Drawing.Point(63, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(1, 28);
            this.panel6.TabIndex = 13;
            // 
            // iconPictureBox1
            // 
            this.iconPictureBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.iconPictureBox1.IconChar = FontAwesome.Sharp.IconChar.Star;
            this.iconPictureBox1.IconColor = System.Drawing.Color.White;
            this.iconPictureBox1.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.iconPictureBox1.IconSize = 23;
            this.iconPictureBox1.Location = new System.Drawing.Point(73, 0);
            this.iconPictureBox1.Name = "iconPictureBox1";
            this.iconPictureBox1.Size = new System.Drawing.Size(23, 28);
            this.iconPictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.iconPictureBox1.TabIndex = 12;
            this.iconPictureBox1.TabStop = false;
            // 
            // ButtonRefresh
            // 
            this.ButtonRefresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonRefresh.IconChar = FontAwesome.Sharp.IconChar.Repeat;
            this.ButtonRefresh.IconColor = System.Drawing.Color.White;
            this.ButtonRefresh.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonRefresh.IconSize = 23;
            this.ButtonRefresh.Location = new System.Drawing.Point(96, 6);
            this.ButtonRefresh.Name = "ButtonRefresh";
            this.ButtonRefresh.Size = new System.Drawing.Size(23, 28);
            this.ButtonRefresh.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonRefresh.TabIndex = 14;
            this.ButtonRefresh.TabStop = false;
            this.ButtonRefresh.Click += new System.EventHandler(this.ButtonRefresh_Click);
            // 
            // ButtonRename
            // 
            this.ButtonRename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonRename.IconChar = FontAwesome.Sharp.IconChar.Keyboard;
            this.ButtonRename.IconColor = System.Drawing.Color.White;
            this.ButtonRename.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonRename.IconSize = 23;
            this.ButtonRename.Location = new System.Drawing.Point(273, 6);
            this.ButtonRename.Name = "ButtonRename";
            this.ButtonRename.Size = new System.Drawing.Size(23, 28);
            this.ButtonRename.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonRename.TabIndex = 9;
            this.ButtonRename.TabStop = false;
            // 
            // ButtonStar
            // 
            this.ButtonStar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonStar.IconChar = FontAwesome.Sharp.IconChar.Star;
            this.ButtonStar.IconColor = System.Drawing.Color.White;
            this.ButtonStar.IconFont = FontAwesome.Sharp.IconFont.Regular;
            this.ButtonStar.IconSize = 23;
            this.ButtonStar.Location = new System.Drawing.Point(250, 6);
            this.ButtonStar.Name = "ButtonStar";
            this.ButtonStar.Size = new System.Drawing.Size(23, 28);
            this.ButtonStar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonStar.TabIndex = 8;
            this.ButtonStar.TabStop = false;
            this.ButtonStar.Click += new System.EventHandler(this.ButtonStar_Click);
            // 
            // ButtonCut
            // 
            this.ButtonCut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonCut.ForeColor = System.Drawing.Color.Gray;
            this.ButtonCut.IconChar = FontAwesome.Sharp.IconChar.Cut;
            this.ButtonCut.IconColor = System.Drawing.Color.Gray;
            this.ButtonCut.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonCut.IconSize = 23;
            this.ButtonCut.Location = new System.Drawing.Point(209, 6);
            this.ButtonCut.Name = "ButtonCut";
            this.ButtonCut.Size = new System.Drawing.Size(23, 28);
            this.ButtonCut.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonCut.TabIndex = 7;
            this.ButtonCut.TabStop = false;
            this.ButtonCut.Click += new System.EventHandler(this.ButtonCut_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel4.Location = new System.Drawing.Point(127, 6);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1, 28);
            this.panel4.TabIndex = 6;
            // 
            // ButtonDelete
            // 
            this.ButtonDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonDelete.ForeColor = System.Drawing.Color.Gray;
            this.ButtonDelete.IconChar = FontAwesome.Sharp.IconChar.TrashAlt;
            this.ButtonDelete.IconColor = System.Drawing.Color.Gray;
            this.ButtonDelete.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonDelete.IconSize = 23;
            this.ButtonDelete.Location = new System.Drawing.Point(185, 6);
            this.ButtonDelete.Name = "ButtonDelete";
            this.ButtonDelete.Size = new System.Drawing.Size(23, 28);
            this.ButtonDelete.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonDelete.TabIndex = 5;
            this.ButtonDelete.TabStop = false;
            this.ButtonDelete.Click += new System.EventHandler(this.ButtonDelete_Click);
            // 
            // ButtonPaste
            // 
            this.ButtonPaste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonPaste.ForeColor = System.Drawing.Color.Gray;
            this.ButtonPaste.IconChar = FontAwesome.Sharp.IconChar.Paste;
            this.ButtonPaste.IconColor = System.Drawing.Color.Gray;
            this.ButtonPaste.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonPaste.IconSize = 23;
            this.ButtonPaste.Location = new System.Drawing.Point(160, 6);
            this.ButtonPaste.Name = "ButtonPaste";
            this.ButtonPaste.Size = new System.Drawing.Size(23, 28);
            this.ButtonPaste.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonPaste.TabIndex = 4;
            this.ButtonPaste.TabStop = false;
            this.ButtonPaste.Click += new System.EventHandler(this.ButtonPaste_Click);
            // 
            // ButtonCopy
            // 
            this.ButtonCopy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonCopy.Enabled = false;
            this.ButtonCopy.ForeColor = System.Drawing.Color.Gray;
            this.ButtonCopy.IconChar = FontAwesome.Sharp.IconChar.Clipboard;
            this.ButtonCopy.IconColor = System.Drawing.Color.Gray;
            this.ButtonCopy.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonCopy.IconSize = 23;
            this.ButtonCopy.Location = new System.Drawing.Point(137, 6);
            this.ButtonCopy.Name = "ButtonCopy";
            this.ButtonCopy.Size = new System.Drawing.Size(23, 28);
            this.ButtonCopy.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonCopy.TabIndex = 3;
            this.ButtonCopy.TabStop = false;
            this.ButtonCopy.Click += new System.EventHandler(this.ButtonCopy_Click);
            // 
            // ButtonForward
            // 
            this.ButtonForward.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonForward.IconChar = FontAwesome.Sharp.IconChar.ArrowRight;
            this.ButtonForward.IconColor = System.Drawing.Color.White;
            this.ButtonForward.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonForward.IconSize = 23;
            this.ButtonForward.Location = new System.Drawing.Point(56, 6);
            this.ButtonForward.Name = "ButtonForward";
            this.ButtonForward.Size = new System.Drawing.Size(23, 28);
            this.ButtonForward.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonForward.TabIndex = 2;
            this.ButtonForward.TabStop = false;
            // 
            // ButtonBack
            // 
            this.ButtonBack.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonBack.IconChar = FontAwesome.Sharp.IconChar.ArrowLeft;
            this.ButtonBack.IconColor = System.Drawing.Color.White;
            this.ButtonBack.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonBack.IconSize = 23;
            this.ButtonBack.Location = new System.Drawing.Point(33, 6);
            this.ButtonBack.Name = "ButtonBack";
            this.ButtonBack.Size = new System.Drawing.Size(23, 28);
            this.ButtonBack.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonBack.TabIndex = 1;
            this.ButtonBack.TabStop = false;
            // 
            // ButtonReturn
            // 
            this.ButtonReturn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ButtonReturn.IconChar = FontAwesome.Sharp.IconChar.Backward;
            this.ButtonReturn.IconColor = System.Drawing.Color.White;
            this.ButtonReturn.IconFont = FontAwesome.Sharp.IconFont.Auto;
            this.ButtonReturn.IconSize = 23;
            this.ButtonReturn.Location = new System.Drawing.Point(5, 6);
            this.ButtonReturn.Name = "ButtonReturn";
            this.ButtonReturn.Size = new System.Drawing.Size(23, 28);
            this.ButtonReturn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.ButtonReturn.TabIndex = 0;
            this.ButtonReturn.TabStop = false;
            this.ButtonReturn.Click += new System.EventHandler(this.ButtonReturn_Click);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ContentPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(27)))));
            this.ContentPanel.Location = new System.Drawing.Point(250, 100);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(983, 550);
            this.ContentPanel.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.ClientSize = new System.Drawing.Size(1234, 652);
            this.Controls.Add(this.ContentPanel);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.KeyPreview = true;
            this.MaximumSize = new System.Drawing.Size(1920, 1080);
            this.MinimumSize = new System.Drawing.Size(1050, 191);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NPC File Explorer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel7.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox2)).EndInit();
            this.panel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.iconPictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonRefresh)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonRename)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonStar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonDelete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonPaste)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonCopy)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonForward)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonBack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ButtonReturn)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.FlowLayoutPanel ContentPanel;
        private FontAwesome.Sharp.IconPictureBox ButtonReturn;
        private FontAwesome.Sharp.IconPictureBox ButtonForward;
        private FontAwesome.Sharp.IconPictureBox ButtonBack;
        private FontAwesome.Sharp.IconPictureBox ButtonDelete;
        private FontAwesome.Sharp.IconPictureBox ButtonPaste;
        private FontAwesome.Sharp.IconPictureBox ButtonCopy;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
        private FontAwesome.Sharp.IconPictureBox ButtonRename;
        private FontAwesome.Sharp.IconPictureBox ButtonStar;
        private FontAwesome.Sharp.IconPictureBox ButtonCut;
        private System.Windows.Forms.Panel panel7;
        private System.Windows.Forms.Panel panel8;
        private FontAwesome.Sharp.IconPictureBox iconPictureBox2;
        private System.Windows.Forms.Panel panel6;
        private FontAwesome.Sharp.IconPictureBox iconPictureBox1;
        private FontAwesome.Sharp.IconPictureBox ButtonRefresh;
        private System.Windows.Forms.FlowLayoutPanel SidebarPanel;
        private ModernUI.Controls.ModernTextBox PathTextbox;
    }
}

