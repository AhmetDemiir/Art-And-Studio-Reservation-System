# 🎨 Art & Studio - Online Sanat Galerisi ve Atölye Rezervasyon Sistemi

Modern, kullanıcı dostu ve kapsamlı bir sanat galerisi e-ticaret ve atölye rezervasyon platformu.

## 🚀 Teknoloji Yığını
- **Backend:** C#, ASP.NET Core MVC (.NET 10)
- **Veritabanı:** Microsoft SQL Server, Entity Framework Core (Code-First)
- **Kimlik Doğrulama:** ASP.NET Core Identity (Role-based Auth)
- **Frontend:** HTML5, CSS3, Bootstrap 5, Razor

## ✅ Gereksinimler
- **.NET SDK:** `10.0.x`
- **SQL Server:** LocalDB veya SQL Server instance
- **EF Core CLI (opsiyonel):** `dotnet tool install --global dotnet-ef`

SDK kontrolü:
```bash
dotnet --version
dotnet --list-sdks
```

## ✨ Temel Modüller
- **Eser ve Atölye Yönetimi:** Eser/etkinlik listeleme, stok ve kontenjan takibi.
- **Rezervasyon:** Seans seçme, güncelleme ve iptal.
- **E-Ticaret:** Sepet, kupon/indirim, sipariş akışı.
- **Yorum Sistemi:** Doğrulanmış kullanıcı yorumu ve etkileşim.
- **Admin Panel:** İçerik yönetimi, raporlama ve operasyon yönetimi.

## ⚙️ Kurulum
1. **Repoyu klonlayın**
```bash
git clone https://github.com/AhmetDemiir/Art-And-Studio-Reservation-System.git
cd Art-And-Studio-Reservation-System
```

2. **Bağlantı dizesini ayarlayın**
- `appsettings.json` içindeki `DefaultConnection` değerini kendi SQL Server ortamınıza göre güncelleyin.

3. **Veritabanını güncelleyin**
```bash
dotnet ef database update
```

4. **Projeyi başlatın**
```bash
dotnet run
```

## 🌐 Varsayılan Geliştirme Portları
- HTTP: `http://localhost:5305`
- HTTPS: `https://localhost:7205`

## 🔧 Sık Karşılaşılan Sorunlar
- **Port kullanımda hatası (`address already in use`)**
  - Aynı portu kullanan başka bir süreç vardır. Eski `dotnet run` sürecini kapatıp tekrar deneyin.
- **Build sırasında `MSB3021/MSB3027` dosya kilidi**
  - Uygulama hâlâ çalışıyordur ve `bin/Debug/...exe` dosyasını kilitliyordur.
  - Çözüm: çalışan uygulamayı kapatın (`Ctrl+C`) ve tekrar `dotnet build` çalıştırın.

## 🔑 Varsayılan Admin Hesabı
- **E-posta:** `admin@artgallery.local`
- **Şifre:** `Admin123!`

## 👥 Geliştirici Ekibi
Bu proje, 4 kişilik bir ekibin ortak çalışmasıyla geliştirilmiştir.
