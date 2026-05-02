## 🧠 Kod Nasıl Çalışıyor?

AEGIS basit bir mantıkla çalışır: önce sistemi tarar (baseline oluşturur), sonra oluşan değişiklikleri anlık olarak takip eder.

---

### ⚙️ Temel Yapı

- `fileHashes` → Dosyaların hash değerlerini tutar  
- `lastAlertTime` → Aynı dosya için spam alert oluşmasını engeller  
- `eventQueue` → Dosya sistemi olaylarını sıraya alır  

Thread-safe çalışması için `ConcurrentDictionary` kullanılır.

---

### 🚀 Başlangıç Süreci

Program başlatıldığında:

1. Kullanıcıdan izlenecek klasör alınır  
2. Tüm dosyaların hash’i hesaplanır (baseline)  
3. `FileSystemWatcher` ile izleme başlar  
4. Arka planda event işleme ve UI güncelleme çalışır  

---

### 🧬 Baseline

- Tüm dosyalar SHA-256 ile hashlenir  
- Klasörler `<DIR>` olarak kaydedilir  
- İşlem paralel yürütülür (hızlı tarama)

---

### 🔐 Hash Mekanizması

- SHA-256 kullanılır  
- Dosya kilitliyse retry yapılır  
- `FileShare.ReadWrite` ile diğer işlemler engellenmez  

---

### 👁️ İzleme

`FileSystemWatcher` ile:

- Created  
- Changed  
- Deleted  
- Renamed  

olayları dinlenir ve kuyruğa eklenir.

---

### 🔄 Event İşleme

- Olaylar `eventQueue` üzerinden sırayla işlenir  
- Çok kısa sürede gelen tekrar eventler filtrelenir  
- Her event ilgili handler’a yönlendirilir  

---

### 🧩 Event Tepkileri

- **Created** → Dosya eklenir ve hash alınır  
- **Changed** → Hash değiştiyse alert oluşturulur  
- **Deleted** → Kayıt silinir (klasörse içindekiler de)  
- **Renamed** → Eski path kaldırılır, yeni path eklenir  

---

### 📊 Log ve Arayüz

- Son 15 olay tutulur  
- Konsolda anlık güncellenir  
- Renk kodları kullanılır:
  - 🟢 Created  
  - 🟡 Modified  
  - 🔴 Deleted  
  - 🔵 Renamed  

---

### 🛡️ Özet

AEGIS:

- Gerçek zamanlı çalışır  
- Düşük kaynak tüketir  
- Büyük klasörlerde stabil kalır  
- Dosya değişikliklerini güvenilir şekilde tespit eder  
