using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPC_File_Browser
{
    public partial class FileControl : UserControl
    {
        public event EventHandler<string> FileClicked;
        public event EventHandler<string> FileDoubleClicked;
        public event EventHandler<string> DisplayMessage;
        public string FolderPath { get; set; }
        public bool IsSelected { get; private set; } = false;

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
            FileExtensionLabel.Text = fileExtension;
            FileSizeLabel.Text = fileSize;
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
    }
}