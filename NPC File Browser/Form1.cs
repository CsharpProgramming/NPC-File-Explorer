using NPC_File_Browser.Properties;
using NPC_File_Explorer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPC_File_Browser
{
    public partial class Form1 : Form
    {
        enum DwmWindowAttribute : uint { DWMWA_USE_IMMERSIVE_DARK_MODE = 20, DWMWA_MICA_EFFECT = 38 }

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

        [DllImport("dwmapi.dll")]
        static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute attr, ref int attrValue, int attrSize);

        List<string> PathsClicked = new List<string>();
        string CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads";
        string LastPathClicked;
        string PinnedFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NPC_File_Browser", "pinned_folders.txt");
        int itemCount = 0;

        private CancellationTokenSource _loadCancellationTokenSource;
        private readonly Dictionary<string, FileControl> _fileControls = new Dictionary<string, FileControl>();

        private FileSearch _fileSearch = new FileSearch();
        private bool _indexLoaded = false;

        public Form1()
        {
            InitializeComponent();

            if (Settings.Default.FormWidth != 0 && Settings.Default.FormHeight != 0)
            {
                this.Size = new Size(Settings.Default.FormWidth, Settings.Default.FormHeight);
            }
            this.Location = new Point(Settings.Default.FormX, Settings.Default.FormY);
        }

        private void EnableControlDarkMode(Control control)
        {
            int trueValue = 1;
            SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            DwmSetWindowAttribute(control.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
            DwmSetWindowAttribute(control.Handle, DwmWindowAttribute.DWMWA_MICA_EFFECT, ref trueValue, Marshal.SizeOf(typeof(int)));
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            ContentPanel.Size = new Size(this.Size.Width - 267, this.Size.Height - 135);
            ContentPanel.Location = new Point(250, 100);
            EnableControlDarkMode(ContentPanel);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadItemsAsync(CurrentPath);
            PathTextbox.TextBoxText = CurrentPath;
            LoadSidebar();
            EnableControlDarkMode(ContentPanel);
            EnableControlDarkMode(this);

            // Load existing index
            _fileSearch.LoadIndex();

            if (!_fileSearch.HasIndex())
            {
                var result = MessageBox.Show(
                    "No search index found. Would you like to build one now?\n\n" +
                    "This will scan your user profile folder and may take a few minutes.\n" +
                    "You can continue using the file explorer while indexing runs in the background.",
                    "Build Search Index",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await BuildIndexWithProgress();
                }
            }
            else
            {
                var metadata = _fileSearch.GetMetadata();
                var daysSinceUpdate = (DateTime.Now - metadata.LastFullIndex).TotalDays;

                Debug.WriteLine($"Index loaded: {metadata.FileCount:N0} files, last updated {daysSinceUpdate:F1} days ago");

                // Prompt for update if index is old
                if (daysSinceUpdate > 7)
                {
                    var result = MessageBox.Show(
                        $"Your search index was last updated {daysSinceUpdate:F0} days ago.\n\n" +
                        "Would you like to update it now to ensure accurate search results?",
                        "Update Search Index",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        await UpdateIndexWithProgress();
                    }
                }

                // Start file watcher for real-time updates
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                _fileSearch.SetupFileWatcher(rootPath);
            }

            _indexLoaded = true;
            Debug.WriteLine("File search system ready!");
        }

        private void DisplaySearchResults(List<FileSearch.FileEntry> results)
        {
            ContentPanel.Controls.Clear();
            _fileControls.Clear();
            itemCount = 0;

            foreach (var result in results)
            {
                AddItem(true, result.Name, Helper.Helper.ConvertedSize(result.Size, false), "File", result.FullPath);
                itemCount++;
            }

            ItemCountLabel.Text = results.Count + " Results Found";
        }

        private async Task BuildIndexWithProgress()
        {
            var progressForm = new Form
            {
                Text = "Building Search Index",
                Size = new Size(400, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Scanning files...",
                Location = new Point(20, 20),
                Size = new Size(360, 20)
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(20, 50),
                Size = new Size(340, 23),
                Style = ProgressBarStyle.Marquee
            };

            var cancelButton = new Button
            {
                Text = "Run in Background",
                Location = new Point(140, 85),
                Size = new Size(120, 30)
            };

            bool cancelled = false;
            cancelButton.Click += (s, e) =>
            {
                cancelled = true;
                progressForm.Close();
            };

            progressForm.Controls.Add(label);
            progressForm.Controls.Add(progressBar);
            progressForm.Controls.Add(cancelButton);

            var progress = new Progress<int>(count =>
            {
                if (!progressForm.IsDisposed)
                {
                    label.Text = $"Indexed {count:N0} files...";
                }
            });

            progressForm.Show();

            string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            await _fileSearch.BuildIndexAsync(rootPath, progress);

            if (!cancelled && !progressForm.IsDisposed)
            {
                var metadata = _fileSearch.GetMetadata();
                MessageBox.Show(
                    $"Search index built successfully!\n\n" +
                    $"Files indexed: {metadata.FileCount:N0}\n" +
                    $"Total size: {Helper.Helper.ConvertedSize(metadata.TotalSize, false)}\n\n" +
                    "Real-time updates are now enabled.",
                    "Index Ready",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                progressForm.Close();
            }
        }

        // Add this new method for updating index with progress
        private async Task UpdateIndexWithProgress()
        {
            var progressForm = new Form
            {
                Text = "Updating Search Index",
                Size = new Size(400, 150),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label
            {
                Text = "Checking for changes...",
                Location = new Point(20, 20),
                Size = new Size(360, 20)
            };

            var progressBar = new ProgressBar
            {
                Location = new Point(20, 50),
                Size = new Size(340, 23),
                Style = ProgressBarStyle.Marquee
            };

            var cancelButton = new Button
            {
                Text = "Run in Background",
                Location = new Point(140, 85),
                Size = new Size(120, 30)
            };

            bool cancelled = false;
            cancelButton.Click += (s, e) =>
            {
                cancelled = true;
                progressForm.Close();
            };

            progressForm.Controls.Add(label);
            progressForm.Controls.Add(progressBar);
            progressForm.Controls.Add(cancelButton);

            var progress = new Progress<int>(count =>
            {
                if (!progressForm.IsDisposed)
                {
                    label.Text = $"Processed {count:N0} files...";
                }
            });

            progressForm.Show();

            string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            await _fileSearch.UpdateIndexAsync(rootPath, progress);

            if (!cancelled && !progressForm.IsDisposed)
            {
                MessageBox.Show(
                    "Search index updated successfully!",
                    "Update Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                progressForm.Close();
            }
        }

        // Update the RunSearchAsync method for better search experience:
        private async Task RunSearchAsync(string query)
        {
            if (!_indexLoaded || string.IsNullOrWhiteSpace(query))
            {
                // If empty query, reload current directory
                if (_indexLoaded && string.IsNullOrWhiteSpace(query))
                {
                    await LoadItemsAsync(CurrentPath);
                }
                return;
            }

            // Show searching indicator
            ItemCountLabel.Text = "Searching...";

            var results = await Task.Run(() => _fileSearch.Search(query, 500));

            if (results.Count == 0)
            {
                ContentPanel.Controls.Clear();
                _fileControls.Clear();

                var noResults = new Label
                {
                    Text = $"No results found for \"{query}\"",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 12),
                    AutoSize = true,
                    Location = new Point(20, 20)
                };
                ContentPanel.Controls.Add(noResults);
                ItemCountLabel.Text = "0 Results";
            }
            else
            {
                DisplaySearchResults(results);
            }
        }


        private async Task LoadItemsAsync(string directory)
        {
            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _loadCancellationTokenSource.Token;

            CurrentPath = directory;
            PathTextbox.TextBoxText = CurrentPath;
            ContentPanel.Controls.Clear();
            _fileControls.Clear();
            itemCount = 0;

            try
            {
                string[] folders = Directory.GetDirectories(directory);
                string[] files = Directory.GetFiles(directory);

                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    FileInfo info = new FileInfo(file);
                    string extension = string.IsNullOrEmpty(info.Extension) ? "File" : info.Extension.Substring(1).ToUpper() + " File";

                    AddItem(true, info.Name, Helper.Helper.ConvertedSize(info.Length, false), extension, info.FullName);
                    itemCount++;
                }

                foreach (var folder in folders)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    DirectoryInfo info = new DirectoryInfo(folder);
                    var fileControl = AddItem(false, info.Name, "Calculating...", "Folder", info.FullName);

                    await Task.Run(async () => await CalculateFolderSizeAsync(info, fileControl, cancellationToken));
                    itemCount++;
                }
            }
            catch { }

            ItemCountLabel.Text = itemCount + " Items";
        }

        private async Task CalculateFolderSizeAsync(DirectoryInfo info, FileControl fileControl, CancellationToken cancellationToken)
        {
            try
            {
                long size = await Helper.Helper.GetFolderSizeAsync(info, cancellationToken);

                if (!cancellationToken.IsCancellationRequested && !fileControl.IsDisposed)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => fileControl.UpdateSize(Helper.Helper.ConvertedSize(size, false))));
                    }
                    else
                    {
                        fileControl.UpdateSize(Helper.Helper.ConvertedSize(size, false));
                    }
                }
            }

            catch (Exception)
            {
                if (!cancellationToken.IsCancellationRequested && !fileControl.IsDisposed)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => fileControl.UpdateSize("Unknown")));
                    }
                    else
                    {
                        fileControl.UpdateSize("Unknown");
                    }
                }
            }
        }

        private FileControl AddItem(bool isFile, string fileName, string fileSize, string fileExtension, string fullPath)
        {
            FileControl FC = new FileControl(isFile, fileName, fileSize, fileExtension);
            FC.FolderPath = fullPath;
            FC.FileClicked += UpdateItems_FileClicked;
            FC.FileDoubleClicked += UpdateItems_FileDoubleClicked;
            ContentPanel.Controls.Add(FC);
            _fileControls[fullPath] = FC;
            return FC;
        }

        private void UpdateItems_FileClicked(object sender, string directory)
        {
            if (PathsClicked.Contains(directory) == false)
            {
                PathsClicked.Add(directory);
                ItemCountLabel.Text = itemCount + " Items | " + PathsClicked.Count + " Selected";
                LastPathClicked = directory;

                UpdateStarButton();
                EnableUI();
            }
        }
        
        private void UpdateStarButton()
        {
            if (IsPathPinned(LastPathClicked))
            {
                ButtonStar.IconFont = FontAwesome.Sharp.IconFont.Solid;
            }

            else
            {
                ButtonStar.IconFont = FontAwesome.Sharp.IconFont.Regular;
            }
        }

        private async void UpdateItems_FileDoubleClicked(object sender, string directory)
        {
            if (Directory.Exists(directory))
            {
                await LoadItemsAsync(directory);
                DisableUI();
                PathsClicked.Clear();
            }

            else if (File.Exists(directory)) 
            {
                //stackoverflow.com/questions/11365984/c-sharp-open-file-with-default-application-and-parameters
                Process fileopener = new Process();

                fileopener.StartInfo.FileName = "explorer";
                fileopener.StartInfo.Arguments = "\"" + directory + "\"";
                fileopener.Start();
            }
        }

        private async void ButtonReturn_Click(object sender, EventArgs e)
        {
            if (CurrentPath.Length > 3)
            {
                await LoadItemsAsync(Directory.GetParent(CurrentPath).FullName);
            }

            else
            {
                System.Media.SystemSounds.Beep.Play();
            }
        }

        private async void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                foreach (Control control in ContentPanel.Controls)
                {
                    if (control is FileControl fileControl && fileControl.IsSelected)
                    {
                        fileControl.Deselect();
                    }
                }

                PathsClicked.Clear();
                ButtonStar.IconFont = FontAwesome.Sharp.IconFont.Regular;
                ItemCountLabel.Text = itemCount + " Items";
                DisableUI(); 
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                string input = PathTextbox.TextBoxText.Trim();

                if (_indexLoaded)
                {
                    await RunSearchAsync(SearchTextbox.TextBoxText);
                    return;
                }

                if (string.IsNullOrWhiteSpace(input))
                {
                    PathTextbox.TextBoxText = CurrentPath;
                    await LoadItemsAsync(CurrentPath);
                    return;
                }

                MessageBox.Show("Search index not loaded yet! Please build or load it first.", "Index Missing", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if ((ModifierKeys & Keys.Control) == Keys.Control &&
            (ModifierKeys & Keys.Shift) == Keys.Shift &&
            e.KeyCode == Keys.R)
            {
                e.SuppressKeyPress = true;

                var result = MessageBox.Show(
                    "Rebuild the entire search index?\n\n" +
                    "This will rescan all files and may take several minutes.",
                    "Rebuild Index",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    await BuildIndexWithProgress();
                }
            }

            // Add keyboard shortcut for updating index (Ctrl+R)
            if ((ModifierKeys & Keys.Control) == Keys.Control &&
                e.KeyCode == Keys.R &&
                (ModifierKeys & Keys.Shift) != Keys.Shift)
            {
                e.SuppressKeyPress = true;
                await UpdateIndexWithProgress();
            }

            if ((ModifierKeys & Keys.Control) == Keys.Control && e.KeyCode == Keys.A)
            {
                PathsClicked.Clear();

                foreach (string path in Directory.GetDirectories(CurrentPath))
                {
                    PathsClicked.Add(path);
                }

                foreach (string file in Directory.GetFiles(CurrentPath))
                {
                    PathsClicked.Add(file);
                }

                foreach (Control control in ContentPanel.Controls)
                {
                    if (control is FileControl fileControl)
                    {
                        fileControl.Select();
                    }
                }

                EnableUI();
                ItemCountLabel.Text = itemCount + " Items | " + PathsClicked.Count + " Selected";
            }
        }

        private void CopyDirectories(List<string> directories)
        {
            if (directories == null || directories.Count == 0) { return; }

            StringCollection paths = new StringCollection();
            paths.AddRange(directories.ToArray());

            Clipboard.SetFileDropList(paths);
            ButtonPaste.Enabled = true;
            ButtonPaste.IconColor = Color.White;
        }

        private void PasteDirectories(string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            if (Clipboard.ContainsFileDropList())
            {
                StringCollection paths = Clipboard.GetFileDropList();

                foreach (string path in paths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            File.Copy(path, System.IO.Path.Combine(directory, System.IO.Path.GetFileName(path)), overwrite: true);
                        }

                        else if (Directory.Exists(path))
                        {
                            Helper.Helper.CopyDirectory(path, System.IO.Path.Combine(directory, System.IO.Path.GetFileName(path)));
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Error pasting: " + ex.Message);
                    }
                }
            }
        }

        private void DeleteDirectories(List<string> directories)
        {
            foreach (string directory in directories)
            {
                if (File.Exists(directory))
                {
                    File.Delete(directory);
                }
                
                else if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
        }

        private void ButtonCopy_Click(object sender, EventArgs e)
        {
            CopyDirectories(PathsClicked);
        }

        private async void ButtonPaste_Click(object sender, EventArgs e)
        {
            PasteDirectories(CurrentPath);

            foreach (Control control in ContentPanel.Controls)
            {
                if (control is FileControl fileControl && fileControl.IsSelected)
                {
                    fileControl.Deselect();
                }
            }

            PathsClicked.Clear();
            DisableUI();
            await LoadItemsAsync(CurrentPath);
        }

        private async void ButtonDelete_Click(object sender, EventArgs e)
        {
            DeleteDirectories(PathsClicked);
            PathsClicked.Clear();
            DisableUI();
            await LoadItemsAsync(CurrentPath);
        }

        private void ButtonCut_Click(object sender, EventArgs e)
        {
            //Coming in 1.3.0!
        }

        private void EnableUI()
        {
            ButtonCopy.Enabled = true;
            ButtonCopy.IconColor = Color.White;
            ButtonDelete.Enabled = true;
            ButtonDelete.IconColor = Color.White;
        }

        private void DisableUI()
        {
            ButtonCopy.Enabled = false;
            ButtonCopy.IconColor = Color.Gray;
            ButtonDelete.Enabled = false;
            ButtonDelete.IconColor = Color.Gray;
        }

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            PathsClicked.Clear();
            DisableUI();
            Helper.Helper.ClearSizeCache();
            await LoadItemsAsync(CurrentPath);
            PathTextbox.TextBoxText = CurrentPath;
        }

        private void LoadSidebar()
        {
            SidebarPanel.Controls.Clear();

            AddSidebarFile("Desktop", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), FontAwesome.Sharp.IconChar.Desktop);
            AddSidebarFile("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads", FontAwesome.Sharp.IconChar.Download);
            AddSidebarFile("Documents", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FontAwesome.Sharp.IconChar.FilePdf);
            AddSidebarFile("Videos", Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), FontAwesome.Sharp.IconChar.FileVideo);

            AddSideBarSeperator();

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    AddSidebarDrive(drive.Name.ToString().Remove(2, 1));
                }
            }
            catch { }

            AddSideBarSeperator();

            if (File.Exists(PinnedFilePath))
            {
                foreach (string path in File.ReadAllLines(PinnedFilePath))
                {
                    if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                    {
                        string folderName = System.IO.Path.GetFileName(path);
                        if (string.IsNullOrEmpty(folderName))
                        {
                            folderName = path;
                        }

                        AddSidebarFile(folderName, path, FontAwesome.Sharp.IconChar.Folder);
                    }
                }
            }
        }

        private void AddSidebarFile(string folderName, string folderPath, FontAwesome.Sharp.IconChar icon)
        {
            SidebarFileControl SFC = new SidebarFileControl(folderName, icon);
            SFC.FolderPath = folderPath;
            SFC.FileDoubleClicked += async (sender, path) => { await LoadItemsAsync(folderPath); };
            SidebarPanel.Controls.Add(SFC);
        }

        private void AddSidebarDrive(string drive)
        {
            SideBarDriveControl SDC = new SideBarDriveControl(drive);
            SDC.FileDoubleClicked += async (sender, path) => { await LoadItemsAsync(drive + @"\"); };
            SidebarPanel.Controls.Add(SDC);
        }

        private void AddSideBarSeperator()
        {
            Panel panel1 = new Panel();
            panel1.Size = new Size(236, 5);
            panel1.BackColor = Color.Transparent;
            Panel panel2 = new Panel();
            panel2.Size = new Size(236, 2);
            panel2.BackColor = Color.DimGray;
            Panel panel3 = new Panel();
            panel3.Size = new Size(236, 5);
            panel3.BackColor = Color.Transparent;

            SidebarPanel.Controls.Add(panel1);
            SidebarPanel.Controls.Add(panel2);
            SidebarPanel.Controls.Add(panel3);
        }

        private void AddPinnedFolder(string folderPath)
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(PinnedFilePath));

            if (File.Exists(PinnedFilePath))
            {
                if (File.ReadAllLines(PinnedFilePath).Any(path => path.Equals(folderPath, StringComparison.OrdinalIgnoreCase))) { return; }
            }

            File.AppendAllText(PinnedFilePath, folderPath + Environment.NewLine);
        }

        private bool IsPathPinned(string folderPath)
        {
            if (File.Exists(PinnedFilePath))
            {
                return File.ReadAllLines(PinnedFilePath).Any(path => path.Equals(folderPath, StringComparison.OrdinalIgnoreCase));
            }

            return false;
        }

        private void RemovePinnedFolder(string folderPath)
        {
            if (File.Exists(PinnedFilePath))
            {
                File.WriteAllLines(PinnedFilePath, File.ReadAllLines(PinnedFilePath).Where(path => !path.Equals(folderPath, StringComparison.OrdinalIgnoreCase)).ToArray());
            }
        }

        private void ButtonStar_Click(object sender, EventArgs e)
        {
            if (IsPathPinned(LastPathClicked))
            {
                RemovePinnedFolder(LastPathClicked);
            }

            else
            {
                AddPinnedFolder(LastPathClicked);
            }

            UpdateStarButton();
            LoadSidebar();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource?.Dispose();
            _fileSearch?.Dispose();
            base.OnFormClosed(e);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.Default.FormWidth = this.Width;
            Settings.Default.FormHeight = this.Height;
            Settings.Default.FormX = Location.X;
            Settings.Default.FormY = Location.Y;
            Settings.Default.Save();
        }
    }
}