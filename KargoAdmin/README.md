# Aleris Global Lojistik Web Sitesi

Bu proje, Aleris Global Lojistik �irketi i�in bir tan�t�m web sitesi ve admin panelini i�ermektedir. ASP.NET Core 8.0 MVC kullan�larak geli�tirilmi�tir.

## �zellikler

### Kullan�c� Taraf�
- Ana sayfa
- Hakk�m�zda sayfas�
- Hizmetlerimiz sayfas�
- Blog listesi ve detay sayfalar�
- �leti�im sayfas�
- Kargo takip alan� (entegrasyon i�in haz�r)

### Admin Paneli
- G�venli oturum y�netimi (ASP.NET Identity)
- Blog yaz�lar�n�n y�netimi (ekleme, d�zenleme, silme)
- Profil y�netimi
- Site ayarlar�

## Kurulum

1. Projeyi klonlay�n veya indirin
2. Visual Studio'da a��n
3. Veritaban�n� olu�turmak i�in a�a��daki komutlar� Package Manager Console'da �al��t�r�n:
   ```
   update-database
   ```
4. Projeyi �al��t�r�n
5. Admin paneline eri�mek i�in:
   - Kullan�c� ad�: admin@kargo.com
   - �ifre: Admin123!

## Geli�tirme Ad�mlar�

### Veritaban� G�ncellemeleri

Veritaban� modellerinde de�i�iklik yapt���n�zda a�a��daki komutlar� kullan�n:

```
add-migration MigrationAdi
update-database
```

### Gerekli G�rseller

wwwroot/img klas�r� alt�nda a�a��daki g�rselleri eklemeniz gerekmektedir:

- `logo.png` - Site logosu
- `hero-image.png` - Ana sayfa hero g�rseli 
- ve di�er g�rseller (README-img.md dosyas�na bak�n�z)

## Teknik Detaylar

- ASP.NET Core 8.0 MVC
- Entity Framework Core
- ASP.NET Core Identity
- Bootstrap 5
- Font Awesome 6
- CKEditor 5
- jQuery

## Kargo Takip Entegrasyonu

Kargo takip sistemi i�in entegrasyon altyap�s� haz�rd�r. Bu alan� gelecekte ger�ek bir kargo takip API'si ile entegre edebilirsiniz.

## �leti�im Formu

�leti�im formundan gelen mesajlar �u anda sadece ge�ici veri olarak saklanmaktad�r. E-posta g�nderimi i�in bir e-posta servisi entegre edilmesi gerekmektedir.

## Lisans

Bu proje size �zel olarak geli�tirilmi�tir.