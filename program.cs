using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AegisFIM
{
    class Program
    {
        private static readonly ConcurrentDictionary<string, string> fileHashes = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, DateTime> lastAlertTime = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        private static readonly BlockingCollection<FileSystemEventArgs> eventQueue = new BlockingCollection<FileSystemEventArgs>();
        
        private static int itemsTracked = 0;
        private static int alertsGenerated = 0;
        private static string targetDirectory = string.Empty;
        private static bool isMonitoring = true;
        private static readonly object logLock = new object();
        private static readonly Queue<string> recentLogs = new Queue<string>();

        static async Task Main(string[] args)
        {
            Console.Title = "AEGIS - Ultra File Integrity Monitor";
            Console.CursorVisible = false;
            DrawBanner();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[?] Enter directory to monitor: ");
            Console.ForegroundColor = ConsoleColor.White;
            targetDirectory = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[!] Invalid directory.");
                return;
            }

            Console.Clear();
            DrawBanner();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[*] Initializing baseline for: {targetDirectory}");

            await BuildBaselineAsync(targetDirectory);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[+] Baseline established. Tracking {itemsTracked} items.");
            Thread.Sleep(1500);
            
            StartMonitoring();
            
            Task.Run(() => ProcessEventQueue());
            Task uiTask = Task.Run(() => UpdateUI());

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                isMonitoring = false;
                eventQueue.CompleteAdding();
            };

            await uiTask;
            Console.ResetColor();
            Console.CursorVisible = true;
        }

        static void DrawBanner()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"
    ___   ___________ ____________ 
   /   | / ____/ ____/  _/ ___/
  / /| |/ __/ / / __ / / \__ \ 
 / ___ / /___/ /_/ // / ___/ / 
/_/  |_\____/\____/___//____/  
   Advanced Integrity Monitor
");
            Console.ResetColor();
        }

        static async Task BuildBaselineAsync(string path)
        {
            try
            {
                if (!Directory.Exists(path)) return;

                var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                var dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                
                foreach (var dir in dirs)
                {
                    if (fileHashes.TryAdd(dir, "<DIR>"))
                        Interlocked.Increment(ref itemsTracked);
                }

                var tasks = files.Select(file => Task.Run(() =>
                {
                    string hash = ComputeHashWithRetry(file);
                    if (hash != null)
                    {
                        if (fileHashes.TryAdd(file, hash))
                            Interlocked.Increment(ref itemsTracked);
                    }
                })).ToArray();

                if (tasks.Length > 0)
                    await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                LogAlert("ERROR", $"Baseline failed: {ex.Message}", ConsoleColor.Red);
            }
        }

        static string ComputeHashWithRetry(string filePath, int retries = 5)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    if (Directory.Exists(filePath)) return "<DIR>";

                    using (var sha256 = SHA256.Create())
                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        byte[] hash = sha256.ComputeHash(stream);
                        var sb = new StringBuilder();
                        foreach (byte b in hash) sb.Append(b.ToString("x2"));
                        return sb.ToString();
                    }
                }
                catch (UnauthorizedAccessException) { if (Directory.Exists(filePath)) return "<DIR>"; Thread.Sleep(100); }
                catch (IOException) { Thread.Sleep(100); }
                catch { return null; }
            }
            return null;
        }

        static void StartMonitoring()
        {
            FileSystemWatcher watcher = new FileSystemWatcher(targetDirectory)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                InternalBufferSize = 65536,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.Security | NotifyFilters.Attributes | NotifyFilters.CreationTime
            };

            watcher.Changed += (s, e) => eventQueue.Add(e);
            watcher.Created += (s, e) => eventQueue.Add(e);
            watcher.Deleted += (s, e) => eventQueue.Add(e);
            watcher.Renamed += (s, e) => eventQueue.Add(e);
        }

        static void ProcessEventQueue()
        {
            foreach (var e in eventQueue.GetConsumingEnumerable())
            {
                if (!isMonitoring) break;

                if (lastAlertTime.TryGetValue(e.FullPath, out DateTime last) && (DateTime.Now - last).TotalMilliseconds < 250)
                    continue;

                try
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            HandleCreated(e.FullPath);
                            break;
                        case WatcherChangeTypes.Deleted:
                            HandleDeleted(e.FullPath);
                            break;
                        case WatcherChangeTypes.Changed:
                            HandleChanged(e.FullPath);
                            break;
                        case WatcherChangeTypes.Renamed:
                            if (e is RenamedEventArgs re) HandleRenamed(re);
                            break;
                    }
                }
                catch { }
            }
        }

        static void HandleCreated(string path)
        {
            if (Directory.Exists(path))
            {
                if (fileHashes.TryAdd(path, "<DIR>"))
                {
                    Interlocked.Increment(ref itemsTracked);
                    LogAlert("CREATED", $"[DIR] {path}", ConsoleColor.Green);
                    _ = BuildBaselineAsync(path);
                }
            }
            else
            {
                string hash = ComputeHashWithRetry(path);
                if (hash != null && fileHashes.TryAdd(path, hash))
                {
                    Interlocked.Increment(ref itemsTracked);
                    LogAlert("CREATED", path, ConsoleColor.Green);
                }
            }
            lastAlertTime[path] = DateTime.Now;
        }

        static void HandleChanged(string path)
        {
            if (Directory.Exists(path)) return;

            string newHash = ComputeHashWithRetry(path);
            if (newHash == null) return;

            if (fileHashes.TryGetValue(path, out string oldHash))
            {
                if (oldHash != newHash && oldHash != "<DIR>")
                {
                    fileHashes[path] = newHash;
                    LogAlert("MODIFIED", path, ConsoleColor.DarkYellow);
                    lastAlertTime[path] = DateTime.Now;
                }
            }
            else
            {
                if (fileHashes.TryAdd(path, newHash))
                {
                    Interlocked.Increment(ref itemsTracked);
                    LogAlert("CREATED", path, ConsoleColor.Green);
                    lastAlertTime[path] = DateTime.Now;
                }
            }
        }

        static void HandleDeleted(string path)
        {
            if (fileHashes.TryRemove(path, out string hash))
            {
                Interlocked.Decrement(ref itemsTracked);
                LogAlert("DELETED", hash == "<DIR>" ? $"[DIR] {path}" : path, ConsoleColor.Red);

                if (hash == "<DIR>")
                {
                    string prefix = path + Path.DirectorySeparatorChar;
                    var nestedKeys = fileHashes.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var key in nestedKeys)
                    {
                        if (fileHashes.TryRemove(key, out _))
                            Interlocked.Decrement(ref itemsTracked);
                    }
                }
            }
            lastAlertTime[path] = DateTime.Now;
        }

        static void HandleRenamed(RenamedEventArgs e)
        {
            bool wasTracked = fileHashes.TryRemove(e.OldFullPath, out string hash);
            
            if (hash == "<DIR>" || Directory.Exists(e.FullPath))
            {
                string oldPrefix = e.OldFullPath + Path.DirectorySeparatorChar;
                string newPrefix = e.FullPath + Path.DirectorySeparatorChar;
                
                var childKeys = fileHashes.Keys.Where(k => k.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var key in childKeys)
                {
                    if (fileHashes.TryRemove(key, out string childHash))
                    {
                        string newPath = newPrefix + key.Substring(oldPrefix.Length);
                        fileHashes.TryAdd(newPath, childHash);
                    }
                }
                if (hash == null) hash = "<DIR>";
            }

            if (wasTracked || hash != null)
            {
                fileHashes.TryAdd(e.FullPath, hash);
                LogAlert("RENAMED", $"{e.OldName} -> {e.Name}", ConsoleColor.Cyan);
            }
            else
            {
                string newHash = ComputeHashWithRetry(e.FullPath);
                if (newHash != null && fileHashes.TryAdd(e.FullPath, newHash))
                {
                    Interlocked.Increment(ref itemsTracked);
                    LogAlert("RENAMED", $"{e.OldName} -> {e.Name}", ConsoleColor.Cyan);
                }
            }
            lastAlertTime[e.FullPath] = DateTime.Now;
        }

        static void LogAlert(string type, string msg, ConsoleColor color)
        {
            Interlocked.Increment(ref alertsGenerated);
            lock (logLock)
            {
                string time = DateTime.Now.ToString("HH:mm:ss.fff");
                recentLogs.Enqueue($"[{time}] [{type}] {msg}");
                if (recentLogs.Count > 15) recentLogs.Dequeue();
            }
        }

        static void UpdateUI()
        {
            while (isMonitoring)
            {
                try
                {
                    Console.SetCursorPosition(0, 0);
                    DrawBanner();
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"Target: {targetDirectory}");
                    Console.WriteLine($"Status: ACTIVE (Buff: 64KB, Queue: {eventQueue.Count})".PadRight(Console.WindowWidth - 1));
                    Console.WriteLine($"Items Tracked: {itemsTracked}".PadRight(Console.WindowWidth - 1));
                    Console.WriteLine($"Alerts: {alertsGenerated}".PadRight(Console.WindowWidth - 1));
                    Console.WriteLine(new string('-', 50));

                    lock (logLock)
                    {
                        foreach (var log in recentLogs.ToArray())
                        {
                            if (log.Contains("[MODIFIED]")) Console.ForegroundColor = ConsoleColor.DarkYellow;
                            else if (log.Contains("[CREATED]")) Console.ForegroundColor = ConsoleColor.Green;
                            else if (log.Contains("[DELETED]")) Console.ForegroundColor = ConsoleColor.Red;
                            else if (log.Contains("[RENAMED]")) Console.ForegroundColor = ConsoleColor.Cyan;
                            else if (log.Contains("[ERROR]")) Console.ForegroundColor = ConsoleColor.Magenta;
                            else Console.ForegroundColor = ConsoleColor.Gray;

                            Console.WriteLine(log.PadRight(Console.WindowWidth - 1));
                        }
                    }
                    
                    int currentTop = Console.CursorTop;
                    for (int i = 0; i < Console.WindowHeight - currentTop - 1; i++)
                        Console.WriteLine(new string(' ', Console.WindowWidth - 1));
                }
                catch { }
                Thread.Sleep(150);
            }
        }
    }
}
