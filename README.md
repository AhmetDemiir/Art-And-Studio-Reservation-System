# 🎨 Art & Studio - Online Sanat Galerisi ve Atölye Rezervasyon Sistemi

Modern, kullanıcı dostu ve kapsamlı bir sanat galerisi e-ticaret ve atölye rezervasyon platformu. Bu proje, üniversite "Veritabanı Yönetim Sistemleri" dersi kapsamında 16 farklı iş kuralı ve gereksinimin %100 oranında karşılandığı profesyonel bir .NET Core uygulamasıdır.

## 🚀 Kullanılan Teknolojiler (Tech Stack)
* **Backend:** C#, ASP.NET Core MVC
* **Veritabanı:** Microsoft SQL Server, Entity Framework Core (Code-First)
* **Kimlik Doğrulama:** ASP.NET Core Identity (Role-based Auth)
* **Frontend:** HTML5, CSS3, Bootstrap 5, Razor Pages

## ✨ Temel Özellikler ve Modüller

Sistem, karmaşık veritabanı ilişkileri (One-to-Many, Many-to-Many) kullanılarak aşağıdaki modülleri barındırmaktadır:

* **Eser ve Atölye Yönetimi:** Sanat eserlerinin ve atölye/etkinlik seanslarının detaylı gösterimi, stok ve kontenjan takibi.
* **Akıllı Rezervasyon Sistemi:** Kullanıcıların kontenjan durumuna göre seans seçebildiği, güncelleyebildiği ve iptal edebildiği (stok iadeli) rezervasyon modülü.
* **E-Ticaret ve Sepet:** Eser satın alma, kupon/indirim kodu uygulama ve sipariş geçmişi takibi.
* **Doğrulanmış Sosyal Etkileşim:** Sadece eseri satın alanların veya atölyeye katılanların yorum yapabildiği, "Faydalı Buldum" oylaması ve yönetici yanıtı içeren gelişmiş inceleme (Review) sistemi.
* **Kullanıcı Karşılaştırma Aracı:** Eserleri (fiyat/sanatçı/kategori) ve Etkinlikleri (tarih/kontenjan/ücret) yan yana kıyaslama modülü.
* **Yönetim ve Raporlama (Admin Dashboard):** Yöneticiler için eser ve etkinlik ekleme/silme (CRUD), satış istatistikleri, doluluk oranları ve canlı destek bilet (Ticket) yönetim paneli.

## ⚙️ Kurulum ve Çalıştırma

Projeyi kendi bilgisayarınızda çalıştırmak için aşağıdaki adımları izleyebilirsiniz:

1. **Repoyu Klonlayın:**
   git clone https://github.com/KULLANICI_ADIN/Art-And-Studio-Reservation-System.git

2. **Bağlantı Dizesini (Connection String) Ayarlayın:**
   `appsettings.json` dosyası içerisindeki `DefaultConnection` kısmını kendi yerel SQL Server örneğinize (örn: `(localdb)\mssqllocaldb`) göre güncelleyin.

3. **Veritabanını Güncelleyin:**
   Paket Yöneticisi Konsolu (PMC) veya terminal üzerinden veritabanını oluşturun:
   dotnet ef database update

   *(Not: Proje ilk kez ayağa kalktığında `DbInitializer` sınıfı otomatik olarak çalışarak örnek sanatçıları, eserleri, atölyeleri ve varsayılan `Admin` rolünü veritabanına ekleyecektir.)*

4. **Projeyi Çalıştırın:**
   Visual Studio üzerinden `F5`'e basarak veya terminalden `dotnet run` komutuyla uygulamayı başlatın.

**🔑 Varsayılan Yönetici (Admin) Hesabı:**
* **E-posta:** admin@artgallery.local
* **Şifre:** Admin123!

## 👥 Geliştirici Ekibi
Bu proje, 4 kişilik bir ekibin ortak çalışmasıyla geliştirilmiştir:
* **Proje Yöneticisi ve Dokümantasyon:** Ahmet Demir
* **Veritabanı Mimarı:** [İsim Eklenecek]
* **Backend Geliştirici:** [İsim Eklenecek]
* **Arayüz (UI) Tasarımcısı:** [İsim Eklenecek]
