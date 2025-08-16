using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace NPC_File_Browser.Helper
{
    public static class Helper
    {
        private static readonly ConcurrentDictionary<string, (long size, DateTime lastModified)> _sizeCache = new ConcurrentDictionary<string, (long, DateTime)>();

        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);

        public static async Task<long> GetFolderSizeAsync(DirectoryInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                string path = info.FullName;
                DateTime lastWrite = info.LastWriteTime;

                if (_sizeCache.TryGetValue(path, out var cached))
                {
                    if (DateTime.Now - cached.lastModified < CacheExpiry &&
                        Math.Abs((lastWrite - cached.lastModified).TotalSeconds) < 1)
                    {
                        return cached.size;
                    }
                }

                long size = await Task.Run(() =>
                {
                    return CalculateFolderSizeRecursive(info, cancellationToken);
                }, cancellationToken);

                _sizeCache.AddOrUpdate(path, (size, lastWrite), (key, oldValue) => (size, lastWrite));

                return size;
            }

            catch (OperationCanceledException)
            {
                throw;
            }

            catch (UnauthorizedAccessException)
            {
                return 0;
            }

            catch (DirectoryNotFoundException)
            {
                return 0;
            }

            catch
            {
                return 0;
            }
        }

        private static long CalculateFolderSizeRecursive(DirectoryInfo directory, CancellationToken cancellationToken)
        {
            long totalSize = 0;

            try
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        totalSize += file.Length;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }
                    catch (IOException)
                    {
                        continue;
                    }
                }

                foreach (var subDir in directory.EnumerateDirectories())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    try
                    {
                        totalSize += CalculateFolderSizeRecursive(subDir, cancellationToken);
                    }

                    catch (UnauthorizedAccessException)
                    {
                        continue;
                    }

                    catch (DirectoryNotFoundException)
                    {
                        continue;
                    }
                }
            }

            catch (UnauthorizedAccessException)
            {
                return 0;
            }

            catch (DirectoryNotFoundException)
            {
                return 0;
            }

            return totalSize;
        }

        public static long GetFolderSize(DirectoryInfo info)
        {
            try
            {
                var size = info.EnumerateFiles().Sum(file => file.Length);
                size += info.EnumerateDirectories().Sum(dir => GetFolderSize(dir));
                return size;
            }
            catch
            {
                return 0;
            }
        }

        public static void ClearSizeCache()
        {
            _sizeCache.Clear();
        }

        public static string ConvertedSize(double bytes, bool rounded)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };

            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            if (rounded)
            {
                return $"{len:0} {sizes[order]}";
            }

            else
            {
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public static void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
