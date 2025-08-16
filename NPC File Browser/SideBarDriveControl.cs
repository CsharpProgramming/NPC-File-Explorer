using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace NPC_File_Browser
{
    public partial class SideBarDriveControl : UserControl
    {
        double pbUnit;
        int pbWIDTH, pbHEIGHT, pbComplete;
        Bitmap bmp;
        Graphics g;

        public event EventHandler<string> FileDoubleClicked;

        public SideBarDriveControl(string drive)
        {
            InitializeComponent();
            FileNameLabel.Text = "Drive " + drive;

            UpdateDiskSpace(drive); //New progress bar adapted from: dyclassroom.com/csharp-project/how-to-create-a-custom-progress-bar-in-csharp-using-visual-studio

            DriveInfo info = new DriveInfo(drive);
            if (info.DriveType == DriveType.Removable)
            {
                Icon.IconChar = FontAwesome.Sharp.IconChar.Usb;
            }

            else
            {
                Icon.IconChar = FontAwesome.Sharp.IconChar.Hdd;
            }
        }

        private void FileNameLabel_DoubleClick(object sender, EventArgs e)
        {
            FileDoubleClicked?.Invoke(this, FileNameLabel.Text.Replace("Drive", ""));
        }

        private void SideBarDriveControl_DoubleClick(object sender, EventArgs e)
        {
            FileDoubleClicked?.Invoke(this, FileNameLabel.Text.Replace("Drive", ""));
        }

        private void UpdateDiskSpace(string drive)
        {
            try
            {
                pbWIDTH = pictureBox1.Width;
                pbHEIGHT = pictureBox1.Height;

                if (pbWIDTH <= 0 || pbHEIGHT <= 0)
                {
                    return;
                }

                pbUnit = pbWIDTH / 100.0;
                pbComplete = 0;

                DriveInfo cDrive = new DriveInfo(drive);
                if (cDrive.IsReady)
                {
                    long totalSize = cDrive.TotalSize;
                    long usedSpace = totalSize - cDrive.TotalFreeSpace;
                    UpdateProgressBar((int)Math.Round((double)usedSpace / totalSize * 100));
                    LabelSize.Text = $"{Helper.Helper.ConvertedSize(usedSpace, true)} / {Helper.Helper.ConvertedSize(totalSize, true)}";
                }
            }
            catch { }
        }
        private void UpdateProgressBar(int percentage)
        {
            if (bmp != null) bmp.Dispose();

            bmp = new Bitmap(pbWIDTH, pbHEIGHT);

            if (percentage >= 85)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(40, 40, 40));
                    g.FillRectangle(Brushes.IndianRed, new Rectangle(0, 0, (int)(percentage * pbUnit), pbHEIGHT));
                }
            }

            else
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.FromArgb(40, 40, 40));
                    g.FillRectangle(Brushes.DimGray, new Rectangle(0, 0, (int)(percentage * pbUnit), pbHEIGHT));
                }
            }

            pictureBox1.Image = bmp;
        }
    }
}