using System;
using System.Windows.Forms;

namespace NPC_File_Browser
{
    public partial class SidebarFileControl : UserControl
    {
        public event EventHandler<string> FileDoubleClicked;
        public string FolderPath { get; set; }
        public bool IsSelected { get; private set; } = false;

        public SidebarFileControl(string fileName, FontAwesome.Sharp.IconChar icon)
        {
            InitializeComponent();
            FileNameLabel.Text = Helper.Helper.TruncateFilename(fileName);
            this.DoubleClick += SidebarFileControl_DoubleClick;
            Icon.IconChar = icon;
        }

        private void SidebarFileControl_DoubleClick(object sender, EventArgs e)
        {
            FileDoubleClicked?.Invoke(this, FolderPath);
        }
    }
}