# Mobil Ekip Notu: RBAC ve Yetki Güncellemesi

Bu dokuman, son backend degisikliklerini mobil ekip icin sade bir dille ozetler.  
Amacimiz: geciste hata riskini azaltmak ve yeni akislari netlestirmek.

---

## 1) Ne degisti? (Kisa ozet)

- `UserType` tamamen kaldirildi.
- Yetki modeli artik **sube bazli**:
  - Brand sahibi (`OwnerUserId`) dogal olarak o brand'in tum subelerinde yetkili.
  - Ek olarak kullanicilar belirli subelere `BranchAdminMap` ile admin olarak atanabiliyor.
- Bazi endpointlerde artik "check-in zorunlulugu" admin/owner icin esnetildi.

---

## 2) UserType kaldirildi - Mobil tarafinda ne yapilmali?

### Kaldirilan alanlar
- Kayit (`/register`) isteginde artik `userType` gonderilmiyor.
- Profil modelinde `UserType` beklenmemeli.

### Beklenen davranis
- Tum kullanicilar temel olarak "musteri" kabul edilir.
- Yetki kararini frontend'de `UserType` yerine asagidaki yeni alanlarla verin:
  - `isAnyBrandOwner`
  - `isAdminAtCheckedInBranch`

---

## 3) Yeni profil bilgileri (`GET /api/users/me`)

Profil cevabina asagidaki alanlar eklendi:

- `isAnyBrandOwner`  
  Kullanici en az bir brand'in sahibi mi?

- `isAdminAtCheckedInBranch`  
  Kullanici aktif check-in yaptigi subede admin/owner yetkisine sahip mi?

- `branchId`  
  Aktif check-in subesi (varsa).

### Mobilde onerilen kullanim
- "Yonetim" butonlarini ac/kapa:
  - Genel owner ozellikleri icin `isAnyBrandOwner`
  - Sube odakli admin islemleri icin `isAdminAtCheckedInBranch`

---

## 4) Branch admin yapisi (yeni `BranchAdminMap`)

Bir kullanici birden fazla subede admin olabilir.  
Yazma islemleri `Branch` aggregate uzerinden yonetiliyor, ama mobil tarafin bilmesi gereken API'ler:

### Admin listeleme
- `GET /api/branches/{branchId}/admins`

### Admin atama
- `POST /api/branches/{branchId}/admins`
- Body:
```json
{
  "userId": "GUID"
}
```

### Admin kaldirma
- `DELETE /api/branches/{branchId}/admins/{userId}`

### Liste item mantigi
Donen kayitlarda owner/admin ayrimi icin:
- `isBrandOwner`
- `isDelegatedAdmin`

> Not: Brand owner zaten dogal yetkili oldugu icin "atanmis admin" olarak tekrar eklenemez.

---

## 5) Konum (lat/long) ile ilgili yeni esneklikler

Asagidaki noktalarda **brand owner** veya ilgili subede **admin atamasi olan** kullanici icin konum kisiti esnetildi:

- Yakin sube listeleme (`/api/branches/nearby`):
  - Admin/owner kullanici mesafe filtresi olmadan subeleri gorebilir.

- Sube menusunu goruntuleme:
  - Normalde check-in gerekir.
  - Admin/owner, ilgili sube icin check-in olmasa da gorebilir.

- Chat room'a katilim:
  - Normalde aktif lokasyon kontrolu vardir.
  - Admin/owner icin bu kontrol bypass edilir (ilgili sube icin).

- Otomatik check-out kontrolu:
  - Normalde 100m uzaklasinca check-out olabilir.
  - Admin/owner kullanicida otomatik check-out tetiklenmez.

---

## 6) Mobilde dikkat edilmesi gereken kritik noktalar

1. **Body'e `userId` guvenmeyin**  
   Backend bircok yerde kimligi token'dan aliyor. Mobilde baska kullanici ID'siyle islem tasarlamayin.

2. **Eski `UserType` kosullarini temizleyin**  
   UI/guard kararlarini yeni alanlara tasiyin:
   - `isAnyBrandOwner`
   - `isAdminAtCheckedInBranch`

3. **Yetki hatalarini dogru yonetin**  
   Admin olmayan kullaniciya yonetim endpointleri acik olmamali.  
   Backend'den `Unauthorized` / yetki hatasi gelirse kullaniciya yumusak mesaj gostermek iyi olur.

4. **Check-in bagimli ekranlarda fallback dusunun**  
   Admin/owner kullanicilar check-in olmadan da bazi verileri gorebilir.  
   UI'da "check-in yoksa her sey kapali" varsayimini her ekranda ayni uygulamayin.

5. **Cache temizligi**  
   Rol/yetki bilgisi degistiginde (`admin atandi/kaldirildi`) profil ve ilgili liste ekranlarini yeniden cekin.

---

## 7) Onerilen mobil gecis plani

- Adim 1: Model guncelle
  - `UserType` kaldir
  - yeni profil alanlarini ekle

- Adim 2: Yetki kurallarini guncelle
  - eski role kontrollerini yeni bayraklara tasiyin

- Adim 3: Admin ekranlarini bagla
  - admin listeleme / atama / kaldirma endpointleri

- Adim 4: Konum bagimli ekranlari test et
  - normal kullanici vs admin/owner senaryolari

---

## 8) Kisa test checklist (mobil)

- [ ] Kayit akisi `userType` olmadan calisiyor mu?
- [ ] `GET /api/users/me` yeni alanlari dogru parse ediliyor mu?
- [ ] Admin/owner olmayan kullanici yonetim ekrani goremiyor mu?
- [ ] Admin/owner kullanici check-in olmadan ilgili menu/ekranlari gorebiliyor mu?
- [ ] Admin atama/kaldirma sonrasi UI state yenileniyor mu?

---

Herhangi bir endpointte ornek request/response payload isterseniz backend tarafinda tek tek cikarabiliriz.
