## 🧠 Kod Mimarisi ve Çalışma Mantığı

AEGIS, yüksek performanslı ve thread-safe bir mimari ile tasarlanmıştır. Amaç, dosya sistemi olaylarını minimum gecikmeyle yakalayıp güvenilir şekilde doğrulamaktır.

---

### ⚙️ Core Bileşenler

#### 📦 Concurrent Veri Yapıları

- `ConcurrentDictionary<string, string> fileHashes`  
  → Tüm dosya ve klasörlerin hash değerlerini saklar  

- `ConcurrentDictionary<string, DateTime> lastAlertTime`  
  → Aynı dosya için kısa sürede tekrar eden event spam’ini engeller  

- `BlockingCollection<FileSystemEventArgs> eventQueue`  
  → FileSystemWatcher’dan gelen olayları kuyruğa alır ve kontrollü işler  

---

### 🚀 Başlangıç Süreci (Main)

Uygulama başlatıldığında:

1. Kullanıcıdan hedef dizin alınır  
2. **Baseline oluşturulur** (tüm dosyalar hash’lenir)  
3. `FileSystemWatcher` devreye girer  
4. Arka planda:
   - Event queue işlenir  
   - UI sürekli güncellenir  

---

### 🧬 Baseline Oluşturma

```csharp
BuildBaselineAsync()
