using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NPC_File_Explorer
{
    public class FileSearch
    {
        private readonly string _indexPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file_index.json");
        private readonly string _metadataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "index_metadata.json");

        private readonly object _indexLock = new object();
        private List<FileEntry> _files = new List<FileEntry>();
        private Dictionary<string, FileEntry> _filesByPath = new Dictionary<string, FileEntry>(StringComparer.OrdinalIgnoreCase);

        // Inverted index for faster searching
        private Dictionary<string, HashSet<string>> _invertedIndex = new Dictionary<string, HashSet<string>>();

        private FileSystemWatcher _watcher;
        private ConcurrentQueue<FileOperation> _operationQueue = new ConcurrentQueue<FileOperation>();
        private CancellationTokenSource _processingCts;
        private Task _processingTask;

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

        private class FileOperation
        {
            public enum OpType { Add, Remove, Rename }
            public OpType Type { get; set; }
            public string Path { get; set; }
            public string NewPath { get; set; }
            public DateTime Timestamp { get; set; }
        }

        public FileSearch()
        {
            StartBackgroundProcessor();
        }

        private void StartBackgroundProcessor()
        {
            _processingCts = new CancellationTokenSource();
            _processingTask = Task.Run(async () => await ProcessQueueAsync(_processingCts.Token));
        }

        public async Task BuildIndexAsync(string rootPath, IProgress<int> progress = null)
        {
            var files = new ConcurrentBag<FileEntry>();
            var dirs = new ConcurrentStack<string>();
            dirs.Push(rootPath);

            int processedCount = 0;
            long totalSize = 0;

            await Task.Run(() =>
            {
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
                };

                while (dirs.Count > 0)
                {
                    var batch = new List<string>();
                    while (dirs.TryPop(out string dir) && batch.Count < 100)
                    {
                        batch.Add(dir);
                    }

                    Parallel.ForEach(batch, options, currentDir =>
                    {
                        try
                        {
                            foreach (string dir in Directory.GetDirectories(currentDir))
                            {
                                dirs.Push(dir);
                            }

                            foreach (string file in Directory.GetFiles(currentDir))
                            {
                                try
                                {
                                    var info = new FileInfo(file);
                                    var entry = new FileEntry
                                    {
                                        Name = info.Name,
                                        FullPath = info.FullName,
                                        Size = info.Length,
                                        Modified = info.LastWriteTime,
                                        Extension = info.Extension.ToLowerInvariant()
                                    };

                                    files.Add(entry);
                                    Interlocked.Increment(ref processedCount);
                                    Interlocked.Add(ref totalSize, info.Length);

                                    if (processedCount % 1000 == 0)
                                    {
                                        progress?.Report(processedCount);
                                    }
                                }
                                catch { }
                            }
                        }
                        catch { }
                    });
                }

                lock (_indexLock)
                {
                    _files = files.ToList();
                    RebuildInternalStructures();
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

            var updatedFiles = new List<FileEntry>();
            var removedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            lock (_indexLock)
            {
                foreach (var existing in _files.ToList())
                {
                    if (!File.Exists(existing.FullPath))
                    {
                        removedPaths.Add(existing.FullPath);
                    }
                }
            }

            lock (_indexLock)
            {
                _files.RemoveAll(f => removedPaths.Contains(f.FullPath));
            }

            await Task.Run(() =>
            {
                var dirs = new Stack<string>();
                dirs.Push(rootPath);

                while (dirs.Count > 0)
                {
                    string currentDir = dirs.Pop();

                    try
                    {
                        foreach (string dir in Directory.GetDirectories(currentDir))
                            dirs.Push(dir);

                        foreach (string file in Directory.GetFiles(currentDir))
                        {
                            try
                            {
                                var info = new FileInfo(file);

                                lock (_indexLock)
                                {
                                    if (_filesByPath.TryGetValue(file, out var existing))
                                    {
                                        if (existing.Modified != info.LastWriteTime || existing.Size != info.Length)
                                        {
                                            existing.Modified = info.LastWriteTime;
                                            existing.Size = info.Length;
                                            existing.Name = info.Name;
                                            updatedFiles.Add(existing);
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
                                        updatedFiles.Add(entry);
                                    }
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                lock (_indexLock)
                {
                    RebuildInvertedIndex();
                }

                SaveIndex();
            });

            Debug.WriteLine($"Index updated: {updatedFiles.Count} files changed, {removedPaths.Count} files removed");
        }

        private void RebuildInternalStructures()
        {
            _filesByPath.Clear();
            foreach (var file in _files)
            {
                _filesByPath[file.FullPath] = file;
            }

            RebuildInvertedIndex();
        }

        private void RebuildInvertedIndex()
        {
            _invertedIndex.Clear();

            foreach (var file in _files)
            {
                IndexFileTerms(file);
            }
        }

        private void IndexFileTerms(FileEntry file)
        {
            // Split filename into searchable terms
            var terms = SplitIntoTerms(file.Name);

            foreach (var term in terms)
            {
                if (term.Length < 2) continue;

                if (!_invertedIndex.ContainsKey(term))
                {
                    _invertedIndex[term] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }
                _invertedIndex[term].Add(file.FullPath);
            }
        }

        private void RemoveFileFromInvertedIndex(string fullPath)
        {
            foreach (var kvp in _invertedIndex.ToList())
            {
                kvp.Value.Remove(fullPath);
                if (kvp.Value.Count == 0)
                {
                    _invertedIndex.Remove(kvp.Key);
                }
            }
        }

        private List<string> SplitIntoTerms(string filename)
        {
            var terms = new List<string>();
            var lower = filename.ToLowerInvariant();

            // Split by common delimiters
            var parts = lower.Split(new[] { ' ', '_', '-', '.', '(', ')', '[', ']' },
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                terms.Add(part);

                // Add prefixes for autocomplete-style search
                for (int i = 2; i <= Math.Min(part.Length, 5); i++)
                {
                    terms.Add(part.Substring(0, i));
                }
            }

            return terms;
        }

        private void SaveIndex()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_files, Formatting.None);
                File.WriteAllText(_indexPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving index: " + ex.Message);
            }
        }

        private void SaveMetadata()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_metadata, Formatting.Indented);
                File.WriteAllText(_metadataPath, json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error saving metadata: " + ex.Message);
            }
        }

        public void LoadIndex()
        {
            try
            {
                if (!File.Exists(_indexPath))
                    return;

                var json = File.ReadAllText(_indexPath);
                _files = JsonConvert.DeserializeObject<List<FileEntry>>(json) ?? new List<FileEntry>();

                RebuildInternalStructures();

                if (File.Exists(_metadataPath))
                {
                    var metaJson = File.ReadAllText(_metadataPath);
                    _metadata = JsonConvert.DeserializeObject<IndexMetadata>(metaJson) ?? new IndexMetadata();
                }

                Debug.WriteLine($"Loaded {_files.Count} files from index (last indexed: {_metadata.LastFullIndex})");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to load index: " + ex.Message);
                _files = new List<FileEntry>();
            }
        }

        public bool HasIndex() => _files != null && _files.Count > 0;

        public IndexMetadata GetMetadata() => _metadata;

        public List<FileEntry> Search(string query, int maxResults = 1000)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<FileEntry>();

            query = query.ToLowerInvariant();
            var terms = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            lock (_indexLock)
            {
                // Use inverted index for faster search
                HashSet<string> candidatePaths = null;

                foreach (var term in terms)
                {
                    if (_invertedIndex.TryGetValue(term, out var paths))
                    {
                        if (candidatePaths == null)
                        {
                            candidatePaths = new HashSet<string>(paths, StringComparer.OrdinalIgnoreCase);
                        }
                        else
                        {
                            candidatePaths.IntersectWith(paths);
                        }
                    }
                    else
                    {
                        // If any term has no matches, no results
                        candidatePaths = new HashSet<string>();
                        break;
                    }
                }

                if (candidatePaths == null || candidatePaths.Count == 0)
                {
                    // Fallback to linear search for partial matches
                    return _files
                        .Where(f => f.Name.ToLowerInvariant().Contains(query))
                        .OrderByDescending(f => CalculateRelevance(f.Name, query))
                        .Take(maxResults)
                        .ToList();
                }

                // Get files from candidate paths and rank them
                return candidatePaths
                    .Select(path => _filesByPath.TryGetValue(path, out var file) ? file : null)
                    .Where(f => f != null)
                    .OrderByDescending(f => CalculateRelevance(f.Name, query))
                    .Take(maxResults)
                    .ToList();
            }
        }

        private int CalculateRelevance(string filename, string query)
        {
            int score = 0;
            var lowerFilename = filename.ToLowerInvariant();
            var lowerQuery = query.ToLowerInvariant();

            // Exact match
            if (lowerFilename == lowerQuery)
                score += 1000;

            // Starts with query
            if (lowerFilename.StartsWith(lowerQuery))
                score += 500;

            // Word boundary match
            if (lowerFilename.StartsWith(lowerQuery + " ") ||
                lowerFilename.Contains(" " + lowerQuery))
                score += 300;

            // Contains query
            if (lowerFilename.Contains(lowerQuery))
                score += 100;

            // Shorter filenames rank higher (more specific)
            score -= filename.Length / 10;

            return score;
        }

        public void SetupFileWatcher(string folderPath)
        {
            try
            {
                _watcher?.Dispose();

                _watcher = new FileSystemWatcher(folderPath)
                {
                    IncludeSubdirectories = true,
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
                };

                _watcher.Created += (s, e) => QueueOperation(FileOperation.OpType.Add, e.FullPath);
                _watcher.Deleted += (s, e) => QueueOperation(FileOperation.OpType.Remove, e.FullPath);
                _watcher.Changed += (s, e) => QueueOperation(FileOperation.OpType.Add, e.FullPath);
                _watcher.Renamed += (s, e) => QueueOperation(FileOperation.OpType.Rename, e.OldFullPath, e.FullPath);

                Debug.WriteLine($"File watcher started on: {folderPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Watcher setup failed: " + ex.Message);
            }
        }

        private void QueueOperation(FileOperation.OpType type, string path, string newPath = null)
        {
            _operationQueue.Enqueue(new FileOperation
            {
                Type = type,
                Path = path,
                NewPath = newPath,
                Timestamp = DateTime.Now
            });
        }

        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            var batch = new List<FileOperation>();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(1000, cancellationToken);

                    batch.Clear();
                    while (_operationQueue.TryDequeue(out var op) && batch.Count < 100)
                    {
                        batch.Add(op);
                    }

                    if (batch.Count == 0)
                        continue;

                    await Task.Run(() => ProcessBatch(batch), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Queue processing error: {ex.Message}");
                }
            }
        }

        private void ProcessBatch(List<FileOperation> operations)
        {
            bool indexChanged = false;

            lock (_indexLock)
            {
                foreach (var op in operations)
                {
                    try
                    {
                        switch (op.Type)
                        {
                            case FileOperation.OpType.Add:
                                if (File.Exists(op.Path))
                                {
                                    var info = new FileInfo(op.Path);
                                    var entry = new FileEntry
                                    {
                                        Name = info.Name,
                                        FullPath = info.FullName,
                                        Size = info.Length,
                                        Modified = info.LastWriteTime,
                                        Extension = info.Extension.ToLowerInvariant()
                                    };

                                    if (_filesByPath.TryGetValue(op.Path, out var existing))
                                    {
                                        _files.Remove(existing);
                                        RemoveFileFromInvertedIndex(op.Path);
                                    }

                                    _files.Add(entry);
                                    _filesByPath[entry.FullPath] = entry;
                                    IndexFileTerms(entry);
                                    indexChanged = true;
                                }
                                break;

                            case FileOperation.OpType.Remove:
                                if (_filesByPath.TryGetValue(op.Path, out var toRemove))
                                {
                                    _files.Remove(toRemove);
                                    _filesByPath.Remove(op.Path);
                                    RemoveFileFromInvertedIndex(op.Path);
                                    indexChanged = true;
                                }
                                break;

                            case FileOperation.OpType.Rename:
                                if (_filesByPath.TryGetValue(op.Path, out var toRename))
                                {
                                    _files.Remove(toRename);
                                    _filesByPath.Remove(op.Path);
                                    RemoveFileFromInvertedIndex(op.Path);

                                    if (File.Exists(op.NewPath))
                                    {
                                        var info = new FileInfo(op.NewPath);
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
                                        IndexFileTerms(entry);
                                    }
                                    indexChanged = true;
                                }
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing operation: {ex.Message}");
                    }
                }
            }

            if (indexChanged)
            {
                SaveIndex();
                Debug.WriteLine($"Processed batch of {operations.Count} operations");
            }
        }
        public void Dispose()
        {
            _processingCts?.Cancel();
            _processingTask?.Wait(TimeSpan.FromSeconds(2));
            _watcher?.Dispose();
            _processingCts?.Dispose();
        }
    }
}