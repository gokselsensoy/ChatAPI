# Mobil Rehber: SignalR ile Anlik Mesajlasma (`/chathub`)

Bu rehber, mobil ekip icin chat tarafindaki SignalR kullanimini netlestirmek icin hazirlandi.

---

## 1) Mimari Ozet

Chat akisinda iki kanal birlikte kullanilir:

- **HTTP API**  
  Odaya katilma/ayrilma, mesaj gonderme, gecmis mesaj cekme gibi isler.
- **SignalR Hub (`/chathub`)**  
  Anlik event alma (mesaj geldi, biri katildi, biri ayrildi, ban bildirimi vb).

Bu sayede kullanici mesaji gonderdikten sonra diger kullanicilar sayfa yenilemeden mesaji gorur.

---

## 2) Hub baglantisi

Hub endpoint:
- `/chathub`

Auth:
- Access token gereklidir.
- Query string ile token gonderebilirsiniz: `?access_token=...`

Onerilen mobil akisi:
1. Login (`/connect/token`)
2. Hub baglantisi ac
3. Chat ekranina girince ilgili odaya hub grubundan join ol
4. Ekrandan cikinca gruptan ayril

---

## 3) Hub methodlari (client -> server)

### `JoinRoomGroup(roomId: string)`
- Chat ekrani acildiginda cagir.
- Server artik oda erisimini kontrol eder:
  - Ozel/grup odada uye degilsen join edemezsin.
  - Public odada check-in veya ilgili sube admin/owner yetkisi gerekir.

### `LeaveRoomGroup(roomId: string)`
- Chat ekranindan cikarken cagir.

---

## 4) Dinlenecek eventler (server -> client)

### `ReceiveMessage`
Yeni mesaj geldiginde tetiklenir.

Ornek payload:
```json
{
  "id": "guid",
  "chatRoomId": "guid",
  "senderUserId": "guid",
  "senderUserName": "kullanici",
  "message": "Merhaba",
  "createdDate": "2026-04-08T20:10:11Z",
  "isMine": false,
  "senderRole": "Admin"
}
```

### `UserJoined`
Bir kullanici odaya katildiginda tetiklenir.

### `UserLeft`
Bir kullanici odadan ayrildiginda tetiklenir.

### `BannedFromBranch`
Kullanici bulundugu subeden banlandiginda kendi user grubuna gelir.

---

## 5) HTTP endpointlerle beraber kullanim

### Odaya giris
1. `POST /api/chatrooms/join/{roomId}`
2. Basariliysa `JoinRoomGroup(roomId)` cagir

### Odaya mesaj gonderme
1. `POST /api/chatrooms/messages/{roomId}`
2. Server mesaji kaydeder
3. Ayni anda SignalR ile `ReceiveMessage` yayini yapar

> Not: Gonderen client hem HTTP response alir hem de (gruptaysa) `ReceiveMessage` eventi alabilir.  
> Uygulamada duplicate'i engellemek icin `message.id` ile uniq kontrol yapin.

### Odadan cikis
1. `POST /api/chatrooms/leave/{roomId}`
2. `LeaveRoomGroup(roomId)`

---

## 6) Hata senaryolari ve oneriler

- `JoinRoomGroup` hatasi:
  - Odaya yetkin olmayabilir.
  - Room id gecersiz olabilir.
  - Token suresi dolmus olabilir.

Onerilen davranis:
- Hub baglanti hatalarinda token yenile + reconnect dene.
- `401/403` benzeri durumda kullaniciyi chat listesine geri yonlendir.
- Room eventleri icin local state'i idempotent guncelle (aynı event tekrar gelebilir).

---

## 7) Mobil taraf icin kisa checklist

- [ ] Login sonrasi hub baglantisi aciliyor mu?
- [ ] Chat ekraninda `JoinRoomGroup` cagriliyor mu?
- [ ] Chat ekranindan cikarken `LeaveRoomGroup` cagriliyor mu?
- [ ] `ReceiveMessage` eventinde liste anlik guncelleniyor mu?
- [ ] Duplicate mesaj kontrolu (`message.id`) var mi?
- [ ] Reconnect sonrasi aktif oda gruplarina tekrar join oluyor mu?

---

## 8) Bilinen davranislar

- Hub tarafinda room group join'i artik yetki kontrollu.
- Admin/owner kullanicilar ilgili kurallara gore bazi geo-kisitlari bypass edebilir.
- Public odalarda yetkisiz kullanici sadece roomId bilerek gruba dinleyici olarak giremez.

---

Istersen sonraki adimda React Native ve Flutter icin hazir SignalR baglanti kodu orneklerini de ekleyebilirim.
