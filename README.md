🛡️ AEGIS - Advanced File Integrity Monitor
AEGIS, belirli bir dizindeki dosya değişikliklerini gerçek zamanlı olarak takip eden, yüksek performanslı ve asenkron çalışan bir güvenlik aracıdır. Dosyaların dijital parmak izlerini (SHA-256) kullanarak, yetkisiz modifikasyonları, silme işlemlerini veya yeni dosya oluşturma faaliyetlerini anında tespit eder.

🚀 Temel Özellikler
Gerçek Zamanlı İzleme: FileSystemWatcher entegrasyonu ile dosya sistemindeki her hareketi (Oluşturma, Değiştirme, Silme, Ad Değiştirme) milisaniyeler içinde yakalar.

SHA-256 Hashing: Dosya bütünlüğünü doğrulamak için kriptografik olarak güvenli SHA-256 algoritmasını kullanır. Sadece dosya boyutu veya tarihine değil, dosyanın içeriğine odaklanır.

Asenkron Baseline Oluşturma: Uygulama başladığında Task.Run ve WhenAll kullanarak binlerce dosyayı paralel bir şekilde tarar ve hızlıca bir "başlangıç referansı" (baseline) oluşturur.

Thread-Safe Mimari: ConcurrentDictionary ve Interlocked gibi yapılar sayesinde çok iş parçacıklı (multithreading) ortamlarda veri bütünlüğünü korur.

Dinamik Konsol UI: Kullanıcı dostu, renk kodlu ve canlı güncellenen bir dashboard üzerinden anlık istatistikleri ve logları takip etmenizi sağlar.

🛠️ Teknik Detaylar
AEGIS, verimliliği artırmak ve sistem kaynaklarını optimize etmek için şu teknolojileri kullanır:

Parallel Processing: Başlangıç taramasında işlemci çekirdeklerini verimli kullanarak tarama süresini minimize eder.

Event-Driven: Sürekli döngü (polling) yerine olay tabanlı (event-driven) bildirimler kullanarak CPU kullanımını düşük tutar.

Resource Sharing: Dosyalar başka uygulamalar tarafından kullanımdayken bile okuma yapabilmek için FileShare.ReadWrite modunu destekler.

📊 İzleme Parametreleri
Uygulama aşağıdaki dosya özniteliklerindeki değişimleri takip eder:

Dosya Adı

Son Yazılma Tarihi (Last Write)

Dosya Boyutu

Güvenlik İzinleri

Oluşturulma Zamanı

Nasıl Çalışır?
Giriş: Kullanıcıdan izlenecek hedef dizin alınır.

Baseline: Tüm dosyaların SHA-256 hash değerleri hesaplanarak güvenli bir belleğe kaydedilir.

Monitor: Arka planda dosya sistemi dinlenmeye başlanır.

Alert: Herhangi bir değişiklik algılandığında yeni hash değeri eskisiyle karşılaştırılır; eğer fark varsa konsol üzerinde anlık uyarı (Alert) oluşturulur.

Not: Bu proje siber güvenlik farkındalığı ve sistem savunma mekanizmalarını anlamak amacıyla geliştirilmiştir.
