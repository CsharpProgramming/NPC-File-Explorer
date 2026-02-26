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
using System.Security.AccessControl;
using System.Security.Principal;

namespace NPC_File_Browser
{
    public partial class FileControl : UserControl
    {
        public event EventHandler<string> FileClicked;
        public event EventHandler<string> FileDoubleClicked;
        public event EventHandler<string> DisplayMessage;
        public event EventHandler<string> FileRenamed;
        public string FolderPath { get; set; }
        public bool IsSelected { get; private set; } = false;

        string FileName;

        public FileControl(bool isFile, string fileName, string fileSize, string fileExtension)
        {
            InitializeComponent();
            if (isFile)
            {
                Icon.IconChar = FontAwesome.Sharp.IconChar.File;
            }

            else
            {
                Icon.IconChar = FontAwesome.Sharp.IconChar.Folder;
            }

            FileNameLabel.Text = Helper.Helper.TruncateFilename(fileName);
            RenameTextbox.Text = Helper.Helper.TruncateFilename(fileName);
            FileExtensionLabel.Text = fileExtension;
            FileSizeLabel.Text = fileSize;
            RenameTextbox.Location = new Point(41, 4);
            FileName = fileName;
        }

        public void UpdateSize(string newSize)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateSize), newSize);
                return;
            }

            FileSizeLabel.Text = newSize;
        }


        public void Select()
        {
            IsSelected = true;
            this.BackColor = Color.FromArgb(35, 35, 35);
        }

        public void Deselect()
        {
            IsSelected = false;
            this.BackColor = Color.Transparent;
            RenameTextbox.Visible = false;
            FileNameLabel.Visible = true;
        }

        private void FileControl_DoubleClick(object sender, EventArgs e)
        {
            FileDoubleClicked?.Invoke(this, FolderPath);
        }

        private void FileControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Clipboard.SetText(FolderPath);
                DisplayMessage?.Invoke(this, FolderPath);
            }

            else if (e.Button == MouseButtons.Left)
            {
                Select();
                FileClicked?.Invoke(this, FolderPath);
            }
        }

        private void RenameTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.WriteLine("Key pressed: " + e.KeyChar);
            if (e.KeyChar == 13)
            {
                Console.WriteLine("Inside if statement: " + e.KeyChar);
                //if (RenameTextbox.Text == "" || RenameTextbox.Text == Path.GetFileName(FileName)) { return; } //If new name is empty or the same as previously, do nothing
                if (RenameTextbox.Text == "") { return; } //If new name is empty or the same as previously, do nothing
                FileRenamed?.Invoke(this, RenameTextbox.Text);
                RenameTextbox.Visible = false;
                FileNameLabel.Visible = true;
            }
        }

        private void FileControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                ToggleRename();
            }
        }

        public void ToggleRename()
        {
            RenameTextbox.Visible = true;
            FileNameLabel.Visible = false;
        }
    }
}