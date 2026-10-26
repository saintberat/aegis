# 🛡️ AEGIS — File Integrity Monitor (FIM)

[![Release](https://img.shields.io/github/v/release/saintberat/aegis?color=blue&label=Latest%20Release)](https://github.com/saintberat/aegis/releases/latest)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance, real-time **File Integrity Monitoring (FIM)** engine built in **C# (.NET)**.

AEGIS captures a cryptographic snapshot of a target directory and continuously monitors all file operations, immediately alerting on unauthorized additions, modifications, renames, and deletions.

### 🚀 Download & Run
Download the latest standalone executable directly from [**Releases (`AegisFIM.exe`)**](https://github.com/saintberat/aegis/releases/latest/download/AegisFIM.exe). No installation or runtime setup required.

---

## 🧠 Architecture & How It Works

AEGIS operates on a two-phase model:
1. **Baseline Phase:** Scans the target directory recursively, calculating cryptographic SHA-256 hashes in parallel.
2. **Monitoring Phase:** Utilizes the OS-level `FileSystemWatcher` with thread-safe queues to track changes in real time.

```
[Target Directory] ──> [Parallel SHA-256 Baseline] ──> [fileHashes Dictionary]
         │
         ▼ (Real-Time Events)
[FileSystemWatcher] ──> [eventQueue] ──> [Deduplication & Hash Verification] ──> [Console Alert UI]
```

---

## ⚙️ Core Components

- **`fileHashes` (`ConcurrentDictionary<string, string>`):** Thread-safe store mapping file paths to their verified SHA-256 hashes.
- **`lastAlertTime` (`ConcurrentDictionary<string, DateTime>`):** Throttles rapid bursts to eliminate spam alerts.
- **`eventQueue` (`ConcurrentQueue<FileSystemEventArgs>`):** Decouples event detection from disk I/O and hash verification.

---

## 🚀 Execution Lifecycle

1. **Path Initialization:** The user specifies the target directory to supervise.
2. **Parallel Baseline Computation:** All existing files are hashed in parallel across CPU cores. Directories are logged with a `<DIR>` marker.
3. **Watcher Activation:** Attaches OS-level listeners for `Created`, `Changed`, `Deleted`, and `Renamed` notifications.
4. **Worker Loop:** Background threads process queued events sequentially without blocking UI updates.

---

## 🔐 Cryptographic Hashing Engine

- **Algorithm:** SHA-256 (`System.Security.Cryptography.SHA256`).
- **Non-Blocking I/O:** Opened with `FileShare.ReadWrite` so ongoing file writes are not locked by the monitor.
- **Retry Mechanism:** Built-in exponential backoff retry loop for temporarily locked files.

---

## 🧩 Event Dispatch & Responses

| Event | Action Taken |
| :--- | :--- |
| **🟢 Created** | Registers the new file into the hash table and computes its baseline hash. |
| **🟡 Changed** | Recomputes SHA-256. If hash differs from baseline, triggers an immediate modification alert. |
| **🔴 Deleted** | Removes the record from the hash table (and child paths if a directory). |
| **🔵 Renamed** | Migrates the existing hash record from the old path to the new path seamlessly. |

---

## 📊 Live Console UI

- **Rolling Audit Log:** Displays the last 15 security events in an interactive terminal dashboard.
- **Color-Coded Status:**
  - 🟢 **Created** (Green)
  - 🟡 **Modified** (Yellow)
  - 🔴 **Deleted** (Red)
  - 🔵 **Renamed** (Blue)

---

## 🛡️ Specifications & Highlights

- **Low Resource Usage:** Minimal CPU and memory overhead during idle monitoring.
- **Large Directory Stability:** Tested on complex directory trees with high file counts.
- **Zero Third-Party Dependencies:** Pure .NET standard library implementation.

---

## 📄 License

MIT License. Free to use, adapt, and build upon.

<!-- sync-activity: 2026-10-26T17:01:24Z (b5a994a6) -->
