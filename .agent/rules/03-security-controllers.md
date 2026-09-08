# 3. GÜVENLİK (OPENIDDICT) VE CONTROLLER STANDARTLARI

- **Kimlik Doğrulama Şeması:** Projede standart JWT kütüphanesi değil, **OpenIddict** kullanılmaktadır. Controller'larda yetkilendirme KESİNLİKLE `[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]` şeklinde olmalıdır.
- **User Context:** Kullanıcı ID bilgileri `[FromBody]` veya URL'den alınmaz! Token içindeki claim'lerden (öncelikle `sub` veya `ClaimTypes.NameIdentifier`) güvenli bir şekilde okunmalıdır.
- **Kalıtım:** Tüm Controller sınıfları projeye özgü Base Controller'dan (Örn: `ApiControllerBase`) kalıtım almalıdır.
- **Response Modelleri:** API yanıtları standartlaştırılmalıdır. Her zaman `Result<T>` modeli kullanılacak. Büyük listelerde `Result<PaginatedResponse<T>>` yapısı zorunludur.
