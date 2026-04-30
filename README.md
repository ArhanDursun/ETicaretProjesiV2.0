````
4. Tarayıcınızda `http://localhost:4200` adresine giderek projeyi görüntüleyin.

## Katkıda Bulunma

Bu proje kişisel gelişim ve portfolyo amacıyla geliştirilmektedir. Bulduğunuz hatalar (bug) veya geliştirme önerileri için `Issues` sekmesini kullanabilir veya `Pull Request` gönderebilirsiniz.

## Lisans
BuKanka, bilgisayarında proje ana dizinine gidip **`README.md`** adında bir dosya oluştur ve aşağıdaki kodun tamamını kopyalayıp o dosyanın içine yapıştır. Saf markdown (.md) formatı tam olarak budur:
```markdown
# ETicaretProjesi - Gerçek Zamanlı ve Modern E-Ticaret Platformu

Modern web teknolojileri kullanılarak geliştirilmiş, mikroservis haberleşme prensiplerini barındıran, gerçek zamanlı (real-time) bildirim ve mesajlaşma özelliklerine sahip kapsamlı bir B2C/C2C e-ticaret platformudur.

Proje, kullanıcı deneyimini asenkron arka plan işlemleri ve anlık soket bağlantıları ile en üst düzeye çıkarmayı hedeflemektedir.

## Öne Çıkan Özellikler

* **Gerçek Zamanlı Fiyat ve Stok Alarmları:** Admin panelinden bir ürünün fiyatı düşürüldüğünde, ürünü favorilerine ekleyen tüm kullanıcılara *MassTransit (RabbitMQ)* üzerinden asenkron olarak ulaşılarak *SignalR* ile anlık Toast bildirimi gönderilir ve Veritabanı Bildirim Merkezine kaydedilir.
* **Anlık Mesajlaşma (Direct Message):** Alıcılar ve satıcılar arasında `ChatHub` üzerinden gerçek zamanlı mesajlaşma altyapısı.
* **Gelişmiş Bildirim Merkezi:** Teklif durumları (Kabul/Ret/Karşı Teklif), stok uyarıları ve favori bildirimlerinin tutulduğu, dinamik yönlendirmelere sahip okunmuş/okunmamış bildirim yönetimi.
* **Canlı Trafik ve Kullanıcı Durumu:** `TrafficHub` ile sistemde o an aktif/online olan kullanıcıların takibi.
* **Akıllı Sepet ve Teklif Sistemi:** Kullanıcıların ürünlere özel fiyat teklifi verebilmesi ve satıcı ile pazarlık döngüsü.
* **Çoklu Dil Desteği:** `@ngx-translate` ile dinamik TR/EN dil seçenekleri.

## Kullanılan Teknolojiler

Bu proje, ölçeklenebilir ve sürdürülebilir bir mimari kurmak amacıyla modern araçlarla geliştirilmiştir.

### Frontend (İstemci)
* **Framework:** Angular 17+ (Standalone Components Mimari)
* **Reaktif Programlama:** RxJS (Observables, Subjects)
* **Real-time İletişim:** `@microsoft/signalr`
* **Dil Yönetimi:** `@ngx-translate/core`
* **Stil:** SCSS, CSS Variables, Responsive Tasarım

### Backend (Sunucu)
* **Platform:** .NET 10 (C# 14)
* **API Mimari:** RESTful API & ASP.NET Core
* **Message Broker:** RabbitMQ (MassTransit Entegrasyonu)
* **Real-time İletişim:** SignalR (NotificationHub, ChatHub, TrafficHub)
* **ORM:** Entity Framework Core
* **Veritabanı:** PostgreSQL / SQLite
* **Kimlik Doğrulama:** JWT (JSON Web Token) & ASP.NET Core Identity

## Mimari Akış Örneği (Fiyat Alarmı Senaryosu)
Sistemin asenkron yapısını anlamak için fiyat güncelleme akışı:
1. **Tetikleyici:** Admin ürün fiyatını günceller.
2. **Yayıncı:** .NET Core, `ProductPriceChangedEvent` nesnesini RabbitMQ'ya publish eder.
3. **Tüketici:** Arka planda çalışan `ProductPriceChangedConsumer` mesajı yakalar, ürünü favorileyen `UserId`'leri bulur.
4. **Soket:** Bulunan ID'lere `SignalService` aracılığıyla `ReceivePriceAlert` komutu fırlatılır. Aynı anda DB'ye kayıt atılır.
5. **İstemci:** Angular'daki `NotificationSignalR` servisi sinyali dinler, DOM'u güncelleyerek anında Toast bildirimi çıkarır.

## Kurulum Adımları (Local Environment)

Projeyi kendi bilgisayarınızda çalıştırmak için aşağıdaki adımları sırasıyla uygulayın.

### Ön Koşullar
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Node.js (v18+)](https://nodejs.org/)
* [RabbitMQ](https://www.rabbitmq.com/download.html) (Docker Desktop üzerinden `rabbitmq:3-management` imajı tavsiye edilir)
* PostgreSQL veya SQLite ortamı

### 1. RabbitMQ'yu Ayağa Kaldırma (Docker ile)
```bash
docker run -d --hostname my-rabbit --name e-ticaret-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
````

_(Yönetim paneline `http://localhost:15672` adresinden guest/guest ile erişebilirsiniz.)_

### 2. Backend Kurulumu

1. Terminalde `Backend` (API) dizinine gidin.
2. `appsettings.json` dosyasını açarak `ConnectionStrings` (PostgreSQL/SQLite) ve `RabbitMQ` ayarlarınızı kendi lokal ortamınıza göre düzenleyin.
3. Veritabanını oluşturmak için EF Core migrasyonlarını çalıştırın:

```bash
dotnet ef database update
```

4. Projeyi başlatın:

```bash
dotnet run
```

_API varsayılan olarak `https://localhost:7185` portunda ayağa kalkacaktır._

### 3. Frontend Kurulumu

1. Terminalde `Frontend` dizinine gidin.
2. Gerekli NPM paketlerini yükleyin:

```bash
npm install
```

3. Angular projesini geliştirme modunda çalıştırın:

```bash
ng serve
```

4. Tarayıcınızda `http://localhost:4200` adresine giderek projeyi görüntüleyin.

## Katkıda Bulunma

Bu proje kişisel gelişim ve portfolyo amacıyla geliştirilmektedir. Bulduğunuz hatalar (bug) veya geliştirme önerileri için `Issues` sekmesini kullanabilir veya `Pull Request` gönderebilirsiniz.

```

```
