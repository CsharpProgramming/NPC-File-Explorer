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
using System.Windows.Media.Animation;

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
        bool CutMode = false;

        private CancellationTokenSource _loadCancellationTokenSource;
        private readonly Dictionary<string, FileControl> _fileControls = new Dictionary<string, FileControl>();

        private FileSearch _fileSearch = new FileSearch();
        private bool _indexLoaded = false;

        private List<FileSearch.FileEntry> _pendingResults = new List<FileSearch.FileEntry>();
        private bool _isLoadingBatch = false;

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
            await _fileSearch.LoadIndex();

            if (!_fileSearch.HasIndex())
            {
                ItemCountLabel.Text = ItemCountLabel.Text + " | Building Search Index";
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                await _fileSearch.BuildIndexAsync(rootPath);
                ItemCountLabel.Text = ItemCountLabel.Text.Replace(" | Building Search Index", " | Index built");
            }
            
            else //Start file watcher for updating the index
            {
                SearchTextbox.RemovePlaceholder(sender, e);
                SearchTextbox.SetPlaceholderText("Search...");
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                _fileSearch.SetupFileWatcher(rootPath);
            }

            _indexLoaded = true;
        }

        private async void DisplaySearchResults(List<FileSearch.FileEntry> results)
        {
            ContentPanel.SuspendLayout();
            ContentPanel.Controls.Clear();
            _fileControls.Clear();
            itemCount = 0;
            ContentPanel.ResumeLayout();

            _pendingResults = results;

            // Load first 100 instantly
            await LoadBatch(0, Math.Min(100, results.Count));

            // Load rest in background
            if (results.Count > 100)
            {
                _ = LoadRemainingBatches(100);
            }
        }

        private async Task LoadBatch(int start, int end)
        {
            var controls = new Control[end - start];

            for (int i = start; i < end; i++)
            {
                var result = _pendingResults[i];
                string extension = string.IsNullOrEmpty(result.Extension)
                    ? "File"
                    : result.Extension.Substring(1).ToUpper() + " File";

                FileControl FC = new FileControl(true, result.Name,
                    Helper.Helper.ConvertedSize(result.Size, false),
                    extension);
                FC.FolderPath = result.FullPath;
                FC.FileClicked += UpdateItems_FileClicked;
                FC.FileDoubleClicked += UpdateItems_FileDoubleClicked;

                controls[i - start] = FC;
                _fileControls[result.FullPath] = FC;
            }

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() =>
                {
                    ContentPanel.SuspendLayout();
                    ContentPanel.Controls.AddRange(controls);
                    ContentPanel.ResumeLayout();

                    itemCount = ContentPanel.Controls.Count;
                    ItemCountLabel.Text = itemCount < _pendingResults.Count
                        ? $"Loaded {itemCount} of {_pendingResults.Count} Results..."
                        : $"{_pendingResults.Count} Results Found";
                }));
            }
            else
            {
                ContentPanel.SuspendLayout();
                ContentPanel.Controls.AddRange(controls);
                ContentPanel.ResumeLayout();

                itemCount = ContentPanel.Controls.Count;
                ItemCountLabel.Text = itemCount < _pendingResults.Count
                    ? $"Loaded {itemCount} of {_pendingResults.Count} Results..."
                    : $"{_pendingResults.Count} Results Found";
            }
        }

        private async Task LoadRemainingBatches(int startIndex)
        {
            if (_isLoadingBatch) return;
            _isLoadingBatch = true;

            await Task.Run(async () =>
            {
                for (int i = startIndex; i < _pendingResults.Count; i += 100)
                {
                    int batchEnd = Math.Min(i + 100, _pendingResults.Count);
                    await LoadBatch(i, batchEnd);
                    await Task.Delay(50);
                }
            });

            _isLoadingBatch = false;
        }

        private async Task RunSearchAsync(string query)
        {
            if (!_indexLoaded || string.IsNullOrWhiteSpace(query))
            {
                if (_indexLoaded && string.IsNullOrWhiteSpace(query))
                {
                    await LoadItemsAsync(CurrentPath);
                }
                return;
            }

            ItemCountLabel.Text = "Searching...";

            var results = await Task.Run(() => _fileSearch.Search(query, 500));

            if (results.Count == 0)
            {
                ContentPanel.Controls.Clear();
                _fileControls.Clear();

                var noResults = new Label
                {
                    Text = $"No results found for \"{query}\"",
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
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

            if ((ModifierKeys & Keys.Control) == Keys.Control && (ModifierKeys & Keys.Shift) == Keys.Shift && e.KeyCode == Keys.R) //Rebuild index: ctrl+shift+r
            {
                e.SuppressKeyPress = true;

                ItemCountLabel.Text = itemCount + " Items" + " | Rebuilding Search Index";
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                await _fileSearch.BuildIndexAsync(rootPath);
            }

            if ((ModifierKeys & Keys.Control) == Keys.Control && e.KeyCode == Keys.R && (ModifierKeys & Keys.Shift) != Keys.Shift) //Update index: ctr+r
            {
                e.SuppressKeyPress = true;
                string rootPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                await _fileSearch.UpdateIndexAsync(rootPath);
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

                        if (CutMode)
                        {
                            DeleteDirectories(new List<string> { path });
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Error pasting: " + ex.Message);
                    }
                }
            }
            CutMode = false;
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

        private async void ButtonCut_Click(object sender, EventArgs e)
        {
            CopyDirectories(PathsClicked);
            CutMode = true;
        }

        private void EnableUI()
        {
            ButtonCopy.Enabled = true;
            ButtonCopy.IconColor = Color.White;
            ButtonCut.Enabled = true;
            ButtonCut.IconColor = Color.White;
            ButtonRename.Enabled = true;
            ButtonRename.IconColor = Color.White;
            ButtonDelete.Enabled = true;
            ButtonDelete.IconColor = Color.White;
        }

        private void DisableUI()
        {
            ButtonCopy.Enabled = false;
            ButtonCopy.IconColor = Color.Gray;
            ButtonCut.Enabled = false;
            ButtonCut.IconColor = Color.Gray;
            ButtonRename.Enabled = false;
            ButtonRename.IconColor = Color.Gray;
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

        private async void ButtonRename_Click(object sender, EventArgs e)
        {
            string NewName = Microsoft.VisualBasic.Interaction.InputBox("Enter new file name", "Enter new name", Path.GetFileName(LastPathClicked), 0, 0);
            string NewPath = Path.Combine(Path.GetDirectoryName(LastPathClicked), NewName);

            if (NewName == null)
            {
                return;
            }

            if (Directory.Exists(LastPathClicked))
            {
                Directory.Move(LastPathClicked, NewPath);
            }

            else if (File.Exists(LastPathClicked))
            {
                File.Move(LastPathClicked, NewPath);
            }

            else
            {
                MessageBox.Show("File or folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            DisableUI();
            await LoadItemsAsync(CurrentPath);
        }
    }
}