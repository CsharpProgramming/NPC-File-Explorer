using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NPC_File_Browser.Controls
{
    [ToolboxItem(true)]
    public class ModernTextBox : UserControl
    {
        private TextBox textBox;
        private bool isPlaceholderActive = true;

        public string PlaceholderText { get; set; } = "";
        public Color BorderColor { get; set; } = Color.Gray;
        public int BorderRadius { get; set; } = 10;
        public Color BackgroundColor { get; set; } = Color.FromArgb(45, 45, 48);
        public Color TextColor { get; set; } = Color.White;
        public Color PlaceholderColor { get; set; } = Color.DimGray;

        [Browsable(true)]
        public bool Multiline
        {
            get => textBox.Multiline;
            set
            {
                textBox.Multiline = value;
                AdjustTextBox();
            }
        }

        // Exposed events (forwarded from inner textBox)
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs when a key is pressed while focus is on the text box.")]
        public new event KeyPressEventHandler KeyPress;

        [Browsable(true)]
        [Category("Action")]
        public new event EventHandler TextChanged;

        public ModernTextBox()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Padding = new Padding(10);
            Height = 36;

            textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = PlaceholderColor,
                Font = new Font("Segoe UI", 10),
                Location = new Point(10, 8),
                Width = Width - 20,
                Text = PlaceholderText
            };

            // Wire internal handlers
            textBox.Enter += RemovePlaceholder;
            textBox.Leave += SetPlaceholder;
            textBox.KeyPress += TextBox_KeyPress;
            textBox.TextChanged += TextBox_TextChanged;

            Controls.Add(textBox);
            Resize += (s, e) => AdjustTextBox();
        }

        private void AdjustTextBox()
        {
            textBox.Width = Width - Padding.Left - Padding.Right;

            if (!textBox.Multiline)
            {
                textBox.Height = TextRenderer.MeasureText("Test", textBox.Font).Height + 2;
                textBox.Location = new Point(Padding.Left, (Height - textBox.Height) / 2);
            }
            else
            {
                textBox.Height = Height - Padding.Top - Padding.Bottom;
                textBox.Location = new Point(Padding.Left, Padding.Top);
            }
        }

        public void RemovePlaceholder(object sender, EventArgs e)
        {
            if (isPlaceholderActive)
            {
                textBox.Text = "";
                textBox.ForeColor = TextColor;
                isPlaceholderActive = false;
            }
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            isPlaceholderActive = string.IsNullOrEmpty(textBox.Text);
            TextChanged?.Invoke(this, e);
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter && !textBox.Multiline)
            {
                e.Handled = true; // prevent beep
            }

            Console.WriteLine("ModernTextBox internal KeyPress fired: " + e.KeyChar);
            KeyPress?.Invoke(this, e);
        }

        public void SetPlaceholder(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = PlaceholderText;
                textBox.ForeColor = PlaceholderColor;
                isPlaceholderActive = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using (var pen = new Pen(Color.DarkGray))
            using (var brush = new SolidBrush(BackgroundColor))
            {
                var bgPath = GetRoundedRect(rect, BorderRadius);
                g.FillPath(brush, bgPath);
                g.DrawPath(pen, bgPath);
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, diameter, diameter, 180, 90);
            path.AddArc(bounds.Right - diameter, bounds.Y, diameter, diameter, 270, 90);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        public string TextBoxText
        {
            get => isPlaceholderActive ? "" : textBox.Text;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    SetPlaceholder(this, EventArgs.Empty);
                }
                else
                {
                    isPlaceholderActive = false;
                    textBox.Text = value;
                    textBox.ForeColor = TextColor;
                }
            }
        }

        public void SetPlaceholderText(string placeholder)
        {
            PlaceholderText = placeholder;
            SetPlaceholder(this, EventArgs.Empty);
        }
    }
}