# 🎯 GÖREV: OpenIddict Refresh Token "Geçersiz Token" Hatasının Çözümü

## 📝 BAĞLAM VE SORUN
Projede OpenIddict ile Password Flow ve Refresh Token Flow kullanıyoruz. İlk giriş (login) işlemi başarılı oluyor ve `access_token` ile birlikte `refresh_token` başarıyla üretiliyor. Ancak, alınan bu `refresh_token` kullanılarak `/connect/token` endpoint'ine yeni bir token almak için istek atıldığında sistem **"Geçersiz Token" (Invalid Token / Invalid Grant)** hatası veriyor. 

Mevcut yapı Clean Architecture, CQRS ve ASP.NET Core Identity ile kurgulanmıştır.

## 🔍 İNCELENECEK DOSYALAR VE KONTROL LİSTESİ
Lütfen projedeki ilgili dosyaları (özellikle `AuthController.cs` veya token üreten controller ile `Program.cs` / `DependencyInjection.cs` içerisindeki OpenIddict ayarlarını) bul ve aşağıdaki 4 kritik ihtimali analiz et:

### 1. `offline_access` Scope'u Kontrolü
OpenIddict'te refresh token üretebilmek ve kullanabilmek için initial (ilk) login isteğinde client'ın mutlaka `offline_access` scope'unu istemiş olması gerekir.
- **Görev:** `Exchange` metodundaki Password Grant senaryosunda, claim'ler basılırken `offline_access` scope'unun token'a başarıyla eklendiğinden emin ol. 

### 2. Destination (Hedef) Eksikliği
`CreateUserPrincipalAsync` metodunun içinde claim'ler oluşturulurken ve `GetDestinations` metodu çağrılırken, Refresh Token için gerekli hedeflerin ayarlanıp ayarlanmadığını kontrol et.
- **Görev:** Identity'nin ürettiği varsayılan claim'lerin (ve bizim eklediğimiz `sub` claim'inin) sadece `AccessToken`'a değil, Refresh Token'ın da içerisine gidebilmesi için `GetDestinations` yardımcı metodunda `Destinations.RefreshToken`'ın ekli olup olmadığını kontrol et. Eksikse ekle.

### 3. Security Stamp (Güvenlik Damgası) Doğrulaması
Refresh token ile yeni bir token istendiğinde (`IsRefreshTokenGrantType` senaryosu), ASP.NET Core Identity kullanıcının `SecurityStamp` değerini kontrol eder. Eğer token üretildikten sonra kullanıcının veritabanındaki SecurityStamp değeri değiştiyse (veya ilk üretilirken düzgün basılmadıysa) token geçersiz sayılır.
- **Görev:** `Exchange` metodundaki Refresh Token bloğunu incele. `SignInManager.ValidateSecurityStampAsync` veya `CanSignInAsync` süreçlerinde Identity'nin neden fail verdiğini analiz et ve düzelt.

### 4. Şifreleme (Encryption/Signing) Anahtarları
- **Görev:** OpenIddict konfigürasyonunda `AddEphemeralEncryptionKey()` kalıntıları var mı kontrol et. Proje yeniden başlatıldığında token'ların geçersiz olmaması için `.AddDevelopmentEncryptionCertificate()` ve `.AddDevelopmentSigningCertificate()` kullanıldığını teyit et.

## 🛠️ BEKLENEN ÇIKTI
1. Yukarıdaki maddeleri analiz et ve hatanın kaynağını tespit et.
2. `AuthController` (veya ilgili sınıf) içerisindeki `Exchange` metodunu, `CreateUserPrincipalAsync` metodunu ve `GetDestinations` yardımcı metodunu OpenIddict Refresh Token standartlarına göre düzelt.
3. Bana sadece değişen kod bloklarını eksiksiz (geçiştirme yapmadan) ver ve sorunun neden kaynaklandığını kısaca açıkla.