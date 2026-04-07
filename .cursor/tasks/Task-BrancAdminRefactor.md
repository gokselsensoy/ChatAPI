# 🎯 GÖREV: Şube Bazlı Admin Yönetimi (RBAC) ve Yetkilendirme Refactor'ı

## 📝 BAĞLAM VE GÖREV TANIMI
Şu anki sistemde kullanıcıların statik bir `UserType` alanı var. Bu yapıyı kaldırıp, çoklu kiracı (Multi-tenant) tarzı şube bazlı bir yetkilendirme modeline geçiyoruz. Brand (Marka) sahibi (`OwnerUserId`), kendi markasına ait tüm şubelerde doğal olarak en yetkili kişidir. Ayrıca Brand sahibi, sisteme normal kayıt olmuş diğer kullanıcıları belirli şubelere "Admin" olarak atayabilecektir.

## Not
.cursorfiles dosyasındaki kuralları unutma QueryRepositorylerde AutoMapper kullan ".ProjectTo" ve Gerekli Dto Profillerini oluştur ilgili yerler için

## 🛠️ ADIM 1: DOMAIN VE DATABASE DEĞİŞİKLİKLERİ
1. **UserType Kaldırılması:** `User` entity'si (ve ilgili DTO/Command'ler) içerisindeki `UserType` property'sini tamamen sil. Artık sistemdeki herkes temel olarak bir "Müşteri"dir.
2. **Yeni Mapping Tablosu (`BranchAdminMap`):** - `BranchId` ve `UserId` (Admin olarak atanan kişi) tutan yeni bir entity oluştur.
   - Bu tablonun Entity Configuration ayarlarını (Composite Key veya tekil Guid ID, Foreign Key'ler vs.) EF Core standartlarında yaz.
3. **Mimarinin Belirlenmesi (Senin Kararın):** Brand entity'sinde bir `OwnerUserId` var. Brand sahibi, o markanın tüm şubelerinde doğal admindir. 
   - **GÖREV:** Sistemde yeni bir şube oluştuğunda brand sahibini otomatik olarak `BranchAdminMap` tablosuna eklemek mi, yoksa Handler'ların içindeki yetki kontrollerinde `if (userId == branch.Brand.OwnerUserId || IsBranchAdmin(userId))` gibi mantıksal bir kontrol yapmak mı daha "Clean" olur? Mimarimize en uygun olanı seç ve uygula.

## 🔐 ADIM 2: BAN İŞLEMLERİ YETKİLENDİRMESİ (COMMAND HANDLERS)
Aşağıdaki Command Handler'larda işlem yapan kullanıcının (`request.CurrentUserId` veya Token'dan gelen ID), işlem yapılan şubede yetkili olup olmadığını kontrol et. Yetkili değilse `UnauthorizedAccessException` fırlat:
- `BanUserCommandHandler`
- `UpdateBanHandler` (veya UpdateBanCommandHandler)
- `LiftBanHandler` (veya LiftBanCommandHandler)
*Not: Bu kontrol için QueryRepository içinde "Kullanıcı bu şubede admin mi veya brand sahibi mi?" sonucunu (bool) dönen temiz bir metot yazman tavsiye edilir.*

## 🍔 ADIM 3: MENÜ İŞLEMLERİ YETKİLENDİRMESİ (COMMAND HANDLERS)
Tıpkı Ban işlemlerinde olduğu gibi, aşağıdaki Menü Command Handler'larına da aynı admin/brand owner yetki kontrolünü entegre et:
- `CreateMenuCommandHandler`
- `AddMenuItemHandler` (veya AddMenuItemCommandHandler)
- `DeleteMenuItemHandler` (veya DeleteMenuItemCommandHandler)
- `UpdateMenuItemHandler` (veya UpdateMenuItemCommandHandler)

## 💬 ADIM 4: CHAT MESAJLARI ZENGİNLEŞTİRMESİ (QUERY HANDLERS)
- Chat odasındaki mesajları getiren Query Handler'ı bul (Örn: `GetChatRoomMessagesQueryHandler`).
- Dönen mesaj DTO'suna (`ChatMessageDto`) **`SenderRole`** (veya `UserTag`) adında yeni bir string/enum property ekle.
- Mesajları dönerken, mesajı atan kullanıcının o şubede `BranchAdminMap`'te kaydı varsa veya o şubenin bağlı olduğu Brand'in sahibi ise bu tag'i **"Admin"** olarak set et. Aksi takdirde **"Müşteri"** olarak set et. (Frontend bu tag'i kullanarak UI'da admin rozeti gösterecektir).

## 🤖 CURSOR AI İÇİN KESİN KURALLAR (STRICT INSTRUCTIONS)
- **Asla geçiştirme yapma (`// existing code` kullanma):** Değiştirdiğin Handler'ları ve Entity/Configuration dosyalarını BAŞTAN SONA eksiksiz ver.
- **Kodu Bozma:** `DbContext`'i asla Handler'a inject etme. Yeni yazdığın "Bu kişi admin mi?" kontrolünü, ilgili QueryRepository'ler (Örn: `IBranchAdminQueryRepository` veya `IBranchQueryRepository`) içine yaz ve Handler'lardan bu repoları çağır.
- **Performans:** Chat mesajlarını getirirken "N+1" sorgu problemine düşme. Mesaj listesi çekerken Admin listesini de performanslı (Örn: memory'de hashset veya optimized SQL query) bir şekilde harmanla.