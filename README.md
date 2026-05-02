# 🛡️ AEGIS: Advanced File Integrity Monitor

AEGIS, sistem dosyalarınızın bütünlüğünü korumak ve yetkisiz değişiklikleri anında tespit etmek için tasarlanmış, yüksek performanslı bir **File Integrity Monitoring (FIM)** aracıdır.  

Kriptografik doğrulama yöntemlerini modern asenkron mimariyle birleştirerek sistem kaynaklarını minimum düzeyde kullanır.

---

## 🚀 Öne Çıkan Özellikler

| Özellik | Açıklama |
|--------|---------|
| ⚡ Real-Time Monitoring | `FileSystemWatcher` ile dosya sistemi olaylarını anında yakalar |
| 🔐 Deep Integrity Check | SHA-256 ile dosya içeriğini doğrular |
| 🧵 Multi-Threaded Baseline | Binlerce dosyayı paralel olarak hızlıca tarar |
| 📊 Live Dashboard | Konsolda anlık istatistikler ve renkli uyarılar |
| 🪶 Zero-Impact | `FileShare.ReadWrite` ile diğer işlemleri engellemez |

---

## 🛠️ Nasıl Çalışır?

AEGIS üç aşamada çalışır:

### 1️⃣ Baseline (Referans)
Hedef dizindeki tüm dosyaların hash değerleri alınır ve bellekte saklanır.

### 2️⃣ Detection (Tespit)
Dosya sistemi olayları dinlenir:
- Dosya oluşturma
- Silme
- Değiştirme
- Yeniden adlandırma

### 3️⃣ Verification (Doğrulama)
Değişen dosyanın hash’i tekrar hesaplanır ve referansla karşılaştırılır.

---

## 🖥️ Kullanılan Teknolojiler

- **Dil:** C# (.NET)
- **Kütüphaneler:**
  - `System.Security.Cryptography`
  - `System.Collections.Concurrent`
  - `System.Threading.Tasks`

---

## 📸 Çalışma Mantığı (Log Sistemi)

Uygulama gerçek zamanlı olarak şu olayları loglar:

- 🟢 **CREATED** → Yeni dosya oluşturuldu  
- 🟡 **MODIFIED** → Dosya içeriği değişti (Hash mismatch)  
- 🔴 **DELETED** → Dosya silindi  
- 🔵 **RENAMED** → Dosya adı değiştirildi  

---

## ⚠️ Kullanım Notu

Bu araç:
- Siber güvenlik analizleri  
- Malware inceleme  
- Sistem izleme  

gibi amaçlarla geliştirilmiştir.

---

## ▶️ Kullanım (Kurulum Gerektirmez)

Projeyi kurmakla uğraşmak istemeyenler için:
 
1. `.exe` dosyasını çalıştırın  
3. İzlemek istediğiniz dizini belirtin  

Hepsi bu. Hayat bazen bu kadar basit olabiliyor.

---

## 📦 Manuel Kurulum (Opsiyonel)

```bash
git clone https://github.com/kullaniciadiniz/AegisFIM.git
cd AegisFIM
dotnet run
