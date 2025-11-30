using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPC_File_Explorer
{
    public class FileSearch : IDisposable
    {
        private readonly string _indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_index.json");
        private readonly string _metadataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index_metadata.json");
        private readonly object _lock = new object();

        private List<FileEntry> _files = new List<FileEntry>();
        private Dictionary<string, FileEntry> _filesByPath = new Dictionary<string, FileEntry>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, HashSet<string>> _invertedIndex = new Dictionary<string, HashSet<string>>();

        private FileSystemWatcher _watcher;
        private ConcurrentQueue<FileOp> _queue = new ConcurrentQueue<FileOp>();
        private CancellationTokenSource _cts;
        private Task _task;
        private IndexMetadata _metadata = new IndexMetadata();

        public class FileEntry
        {
            public string Name { get; set; }
            public string FullPath { get; set; }
            public long Size { get; set; }
            public DateTime Modified { get; set; }
            public string Extension { get; set; }
        }

        public class IndexMetadata
        {
            public DateTime LastFullIndex { get; set; }
            public string RootPath { get; set; }
            public int FileCount { get; set; }
            public long TotalSize { get; set; }
        }

        private class FileOp
        {
            public int Type { get; set; }
            public string Path { get; set; }
            public string NewPath { get; set; }
        }

        public FileSearch()
        {
            _cts = new CancellationTokenSource();
            _task = Task.Run(async () => await ProcessQueue(_cts.Token));
        }

        public async Task BuildIndexAsync(string rootPath, IProgress<int> progress = null)
        {
            var files = new ConcurrentBag<FileEntry>();
            int count = 0;
            long totalSize = 0;

            await Task.Run(() =>
            {
                var dirs = new ConcurrentStack<string>();
                dirs.Push(rootPath);

                var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1) };

                while (dirs.Count > 0)
                {
                    var batch = new List<string>();
                    while (dirs.TryPop(out string dir) && batch.Count < 100)
                        batch.Add(dir);

                    Parallel.ForEach(batch, options, currentDir =>
                    {
                        try
                        {
                            foreach (var dir in Directory.GetDirectories(currentDir))
                                dirs.Push(dir);

                            foreach (var file in Directory.GetFiles(currentDir))
                            {
                                try
                                {
                                    var info = new FileInfo(file);
                                    files.Add(new FileEntry
                                    {
                                        Name = info.Name,
                                        FullPath = info.FullName,
                                        Size = info.Length,
                                        Modified = info.LastWriteTime,
                                        Extension = info.Extension.ToLowerInvariant()
                                    });

                                    var newCount = Interlocked.Increment(ref count);
                                    Interlocked.Add(ref totalSize, info.Length);

                                    if (newCount % 1000 == 0)
                                        progress?.Report(newCount);
                                }
                                catch { }
                            }
                        }
                        catch { }
                    });
                }

                lock (_lock)
                {
                    _files = files.ToList();
                    RebuildAll();
                }

                _metadata = new IndexMetadata
                {
                    LastFullIndex = DateTime.Now,
                    RootPath = rootPath,
                    FileCount = _files.Count,
                    TotalSize = totalSize
                };

                SaveIndex();
                SaveMetadata();
            });

            SetupFileWatcher(rootPath);
        }

        public async Task UpdateIndexAsync(string rootPath, IProgress<int> progress = null)
        {
            if (!HasIndex())
            {
                await BuildIndexAsync(rootPath, progress);
                return;
            }

            var removed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var updated = 0;

            lock (_lock)
            {
                foreach (var file in _files.ToList())
                {
                    if (!File.Exists(file.FullPath))
                        removed.Add(file.FullPath);
                }
                _files.RemoveAll(f => removed.Contains(f.FullPath));
            }

            await Task.Run(() =>
            {
                var dirs = new Stack<string>();
                dirs.Push(rootPath);

                while (dirs.Count > 0)
                {
                    var currentDir = dirs.Pop();
                    try
                    {
                        foreach (var dir in Directory.GetDirectories(currentDir))
                            dirs.Push(dir);

                        foreach (var file in Directory.GetFiles(currentDir))
                        {
                            try
                            {
                                var info = new FileInfo(file);
                                lock (_lock)
                                {
                                    if (_filesByPath.TryGetValue(file, out FileEntry existing))
                                    {
                                        if (existing.Modified != info.LastWriteTime || existing.Size != info.Length)
                                        {
                                            existing.Modified = info.LastWriteTime;
                                            existing.Size = info.Length;
                                            existing.Name = info.Name;
                                            updated++;
                                        }
                                    }
                                    else
                                    {
                                        var entry = new FileEntry
                                        {
                                            Name = info.Name,
                                            FullPath = info.FullName,
                                            Size = info.Length,
                                            Modified = info.LastWriteTime,
                                            Extension = info.Extension.ToLowerInvariant()
                                        };
                                        _files.Add(entry);
                                        _filesByPath[entry.FullPath] = entry;
                                        updated++;
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                lock (_lock)
                    RebuildIndex();

                SaveIndex();
            });

            Console.WriteLine($"Index updated: {updated} changed, {removed.Count} removed");
        }

        public async Task LoadIndex()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (!File.Exists(_indexPath)) return;

                    var json = File.ReadAllText(_indexPath);
                    _files = JsonConvert.DeserializeObject<List<FileEntry>>(json) ?? new List<FileEntry>();
                    RebuildAll();

                    if (File.Exists(_metadataPath))
                    {
                        var metaJson = File.ReadAllText(_metadataPath);
                        _metadata = JsonConvert.DeserializeObject<IndexMetadata>(metaJson) ?? new IndexMetadata();
                    }
                });

                Console.WriteLine($"Loaded {_files.Count} files (last indexed: {_metadata.LastFullIndex})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load index: {ex.Message}");
                _files = new List<FileEntry>();
            }
        }

        public List<FileEntry> Search(string query, int maxResults = 1000)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<FileEntry>();

            var lower = query.ToLowerInvariant();
            var terms = lower.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            lock (_lock)
            {
                HashSet<string> candidates = null;

                foreach (var term in terms)
                {
                    if (_invertedIndex.TryGetValue(term, out HashSet<string> paths))
                    {
                        if (candidates == null)
                        {
                            candidates = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            candidates.IntersectWith(paths);
                        }
                    }
                    else
                    {
                        candidates = new HashSet<string>();
                        break;
                    }
                }

                if (candidates == null || candidates.Count == 0)
                {
                    return _files
                        .Where(f => f.Name.ToLowerInvariant().Contains(lower))
                        .OrderByDescending(f => Score(f.Name, lower))
                        .Take(maxResults)
                        .ToList();
                }

                var results = new List<FileEntry>();
                foreach (var path in candidates)
                {
                    FileEntry file;
                    if (_filesByPath.TryGetValue(path, out file))
                        results.Add(file);
                }

                return results
                    .OrderByDescending(f => Score(f.Name, lower))
                    .Take(maxResults)
                    .ToList();
            }
        }

        public void SetupFileWatcher(string path)
        {
            try
            {
                if (_watcher != null)
                {
                    _watcher.Dispose();
                    _watcher = null;
                }

                _watcher = new FileSystemWatcher(path)
                {
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Created += (s, e) => Queue(0, e.FullPath, null);
                _watcher.Deleted += (s, e) => Queue(1, e.FullPath, null);
                _watcher.Changed += (s, e) => Queue(0, e.FullPath, null);
                _watcher.Renamed += (s, e) => Queue(2, e.OldFullPath, e.FullPath);

                Console.WriteLine($"File watcher started on: {path}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Watcher setup failed: {ex.Message}");
            }
        }

        public bool HasIndex()
        {
            return _files != null && _files.Count > 0;
        }

        public IndexMetadata GetMetadata()
        {
            return _metadata;
        }

        public void Dispose()
        {
            if (_cts != null)
                _cts.Cancel();

            if (_task != null)
                _task.Wait(TimeSpan.FromSeconds(2));

            if (_watcher != null)
                _watcher.Dispose();

            if (_cts != null)
                _cts.Dispose();
        }

        private void RebuildAll()
        {
            _filesByPath.Clear();
            foreach (var file in _files)
                _filesByPath[file.FullPath] = file;

            RebuildIndex();
        }

        private void RebuildIndex()
        {
            _invertedIndex.Clear();

            foreach (var file in _files)
                AddToIndex(file);
        }

        private void AddToIndex(FileEntry file)
        {
            var terms = GetTerms(file.Name);

            foreach (var term in terms)
            {
                if (term.Length < 2) continue;

                if (!_invertedIndex.ContainsKey(term))
                    _invertedIndex[term] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                _invertedIndex[term].Add(file.FullPath);
            }
        }

        private void RemoveFromIndex(string path)
        {
            foreach (var kvp in _invertedIndex.ToList())
            {
                kvp.Value.Remove(path);
                if (kvp.Value.Count == 0)
                    _invertedIndex.Remove(kvp.Key);
            }
        }

        private List<string> GetTerms(string filename)
        {
            var terms = new List<string>();
            var parts = filename.ToLowerInvariant()
                .Split(new[] { ' ', '_', '-', '.', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                terms.Add(part);

                for (int i = 2; i <= Math.Min(part.Length, 5); i++)
                    terms.Add(part.Substring(0, i));
            }

            return terms;
        }

        private int Score(string filename, string query)
        {
            var lower = filename.ToLowerInvariant();
            int score = 0;

            if (lower == query) score += 1000;
            if (lower.StartsWith(query)) score += 500;
            if (lower.StartsWith(query + " ") || lower.Contains(" " + query)) score += 300;
            if (lower.Contains(query)) score += 100;

            score -= filename.Length / 10;
            return score;
        }

        private void SaveIndex()
        {
            try
            {
                File.WriteAllText(_indexPath, JsonConvert.SerializeObject(_files, Formatting.None));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving index: {ex.Message}");
            }
        }

        private void SaveMetadata()
        {
            try
            {
                File.WriteAllText(_metadataPath, JsonConvert.SerializeObject(_metadata, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving metadata: {ex.Message}");
            }
        }

        private void Queue(int type, string path, string newPath)
        {
            _queue.Enqueue(new FileOp { Type = type, Path = path, NewPath = newPath });
        }

        private async Task ProcessQueue(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, token);

                    var batch = new List<FileOp>();
                    while (_queue.TryDequeue(out FileOp op) && batch.Count < 100)
                        batch.Add(op);

                    if (batch.Count > 0)
                        await Task.Run(() => ProcessBatch(batch), token);
                }
                catch (TaskCanceledException) { break; }
                catch (Exception ex) { Console.WriteLine($"Queue processing error: {ex.Message}"); }
            }
        }

        private void ProcessBatch(List<FileOp> ops)
        {
            bool changed = false;

            lock (_lock)
            {
                foreach (var op in ops)
                {
                    try
                    {
                        if (op.Type == 0)
                            changed |= ProcessAdd(op.Path);
                        else if (op.Type == 1)
                            changed |= ProcessRemove(op.Path);
                        else if (op.Type == 2)
                            changed |= ProcessRename(op.Path, op.NewPath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing operation: {ex.Message}");
                    }
                }
            }

            if (changed)
            {
                SaveIndex();
                Console.WriteLine($"Processed batch of {ops.Count} operations");
            }
        }

        private bool ProcessAdd(string path)
        {
            if (!File.Exists(path)) return false;

            var info = new FileInfo(path);
            var entry = new FileEntry
            {
                Name = info.Name,
                FullPath = info.FullName,
                Size = info.Length,
                Modified = info.LastWriteTime,
                Extension = info.Extension.ToLowerInvariant()
            };

            if (_filesByPath.TryGetValue(path, out FileEntry existing))
            {
                _files.Remove(existing);
                RemoveFromIndex(path);
            }

            _files.Add(entry);
            _filesByPath[entry.FullPath] = entry;
            AddToIndex(entry);
            return true;
        }

        private bool ProcessRemove(string path)
        {
            if (!_filesByPath.TryGetValue(path, out FileEntry file))
                return false;

            _files.Remove(file);
            _filesByPath.Remove(path);
            RemoveFromIndex(path);
            return true;
        }

        private bool ProcessRename(string oldPath, string newPath)
        {
            if (!_filesByPath.TryGetValue(oldPath, out FileEntry file))
                return false;

            _files.Remove(file);
            _filesByPath.Remove(oldPath);
            RemoveFromIndex(oldPath);

            if (File.Exists(newPath))
            {
                var info = new FileInfo(newPath);
                var entry = new FileEntry
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    Size = info.Length,
                    Modified = info.LastWriteTime,
                    Extension = info.Extension.ToLowerInvariant()
                };

                _files.Add(entry);
                _filesByPath[entry.FullPath] = entry;
                AddToIndex(entry);
            }

            return true;
        }
    }
}