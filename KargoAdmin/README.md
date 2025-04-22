# Aleris Global Lojistik Web Sitesi

Bu proje, Aleris Global Lojistik þirketi için bir tanýtým web sitesi ve admin panelini içermektedir. ASP.NET Core 8.0 MVC kullanýlarak geliþtirilmiþtir.

## Özellikler

### Kullanýcý Tarafý
- Ana sayfa
- Hakkýmýzda sayfasý
- Hizmetlerimiz sayfasý
- Blog listesi ve detay sayfalarý
- Ýletiþim sayfasý
- Kargo takip alaný (entegrasyon için hazýr)

### Admin Paneli
- Güvenli oturum yönetimi (ASP.NET Identity)
- Blog yazýlarýnýn yönetimi (ekleme, düzenleme, silme)
- Profil yönetimi
- Site ayarlarý

## Kurulum

1. Projeyi klonlayýn veya indirin
2. Visual Studio'da açýn
3. Veritabanýný oluþturmak için aþaðýdaki komutlarý Package Manager Console'da çalýþtýrýn:
   ```
   update-database
   ```
4. Projeyi çalýþtýrýn
5. Admin paneline eriþmek için:
   - Kullanýcý adý: admin@kargo.com
   - Þifre: Admin123!

## Geliþtirme Adýmlarý

### Veritabaný Güncellemeleri

Veritabaný modellerinde deðiþiklik yaptýðýnýzda aþaðýdaki komutlarý kullanýn:

```
add-migration MigrationAdi
update-database
```

### Gerekli Görseller

wwwroot/img klasörü altýnda aþaðýdaki görselleri eklemeniz gerekmektedir:

- `logo.png` - Site logosu
- `hero-image.png` - Ana sayfa hero görseli 
- ve diðer görseller (README-img.md dosyasýna bakýnýz)

## Teknik Detaylar

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- ASP.NET Core Identity
- Bootstrap 5
- Font Awesome 6
- CKEditor 5
- jQuery

## Kargo Takip Entegrasyonu

Kargo takip sistemi için entegrasyon altyapýsý hazýrdýr. Bu alaný gelecekte gerçek bir kargo takip API'si ile entegre edebilirsiniz.

## Ýletiþim Formu

Ýletiþim formundan gelen mesajlar þu anda sadece geçici veri olarak saklanmaktadýr. E-posta gönderimi için bir e-posta servisi entegre edilmesi gerekmektedir.

## Lisans

Bu proje size özel olarak geliþtirilmiþtir.