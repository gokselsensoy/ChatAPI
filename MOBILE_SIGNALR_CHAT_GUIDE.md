# Mobil Rehber: SignalR Chat Ayrımı (`/chathub`)

Bu rehber, branch sohbet listesi ile private inbox'ın **ayrı abonelik katmanlarıyla** nasıl kullanıldığını anlatır.  
Amaç: her odaya SignalR join etmeden son mesaj / yeni mesaj göstermek; açık sohbette anlık full mesaj almak.

---

## 1) Mimari (3 katman)

| Katman | Ne zaman | SignalR grup | Event |
|--------|----------|--------------|-------|
| Kişisel kanal | Hub connect | `identityId` (otomatik) | `PrivateInboxUpdated` |
| Branch kanalı | Branch chat listesi açıkken | `branch:{branchId}` | `BranchRoomPreviewUpdated` |
| Oda kanalı | Sadece oda ekranı açıkken | `chatroom:{roomId}` | `ReceiveMessage` |

**Ayrı hub yok.** Tek hub: `/chathub`. Ayrım grup + event tipiyle yapılır.

```
Login → Hub connect (kişisel kanal)
  ├─ Branch chat listesi → JoinBranchChannel(branchId)
  │     └─ BranchRoomPreviewUpdated (hafif)
  ├─ Private tab → sadece PrivateInboxUpdated dinle (join gerekmez)
  └─ Belirli oda aç → JoinRoomGroup(roomId)
        └─ ReceiveMessage (full)
```

---

## 2) Hub baglantisi

- Endpoint: `/chathub`
- Auth: `Authorization: Bearer <token>` veya `?access_token=...`
- Connect olunca kişisel grup otomatik eklenir (JWT `sub` / `NameIdentifier`).

---

## 3) Hub methodlari (client → server)

### `JoinBranchChannel(branchId)`
- Branch public sohbet **listesi** açılınca çağır.
- Check-in (aynı branch) veya branch admin/owner gerekir.
- Her public odaya `JoinRoomGroup` **atma**.

### `LeaveBranchChannel(branchId)`
- Liste kapanınca / check-out / başka branch'e geçince çağır.

### `JoinRoomGroup(roomId)`
- **Sadece** sohbet ekranı açılınca çağır.
- Private/Group: üye olmalısın.
- Public: check-in veya admin/owner.

### `LeaveRoomGroup(roomId)`
- Sohbet ekranından çıkınca çağır.

---

## 4) Dinlenecek eventler (server → client)

### `ReceiveMessage` (açık oda)
Sadece `chatroom:{roomId}` grubuna gider.

```json
{
  "id": "guid",
  "chatRoomId": "guid",
  "senderUserId": "guid",
  "senderUserName": "kullanici",
  "message": "Merhaba",
  "createdDate": "2026-07-30T13:00:00Z",
  "isMine": false,
  "senderRole": "Admin"
}
```

> Gönderen HTTP `201` response ile de mesajı alır. Hub grubundaysa `ReceiveMessage` da gelebilir → `message.id` ile duplicate engelle.

### `BranchRoomPreviewUpdated` (branch liste)
Public odada yeni mesaj olunca `branch:{branchId}` grubuna gider. Full mesaj değil:

```json
{
  "roomId": "guid",
  "roomType": "Public",
  "branchId": "guid",
  "lastMessagePreview": "Merhaba...",
  "lastMessageAt": "2026-07-30T13:00:00Z",
  "senderUserId": "guid",
  "hasNew": true
}
```

Liste satırındaki son mesaj + badge'i güncelle.

### `PrivateInboxUpdated` (private / group inbox)
Private veya Group mesajında üyelere (kişisel kanal) gider. Aynı hafif payload; `roomType` = `Private` | `Group`.

Private tab listesini güncelle. Branch listesine karıştırma.

### Diğer
- `UserJoined` / `UserLeft` → `chatroom:{roomId}` (oda açıkken anlamlı)
- `BannedFromBranch` → kişisel kanal

---

## 5) HTTP endpointler

### Branch public odalar (önizlemeli)
`GET /api/chatrooms/public`

Token'dan user çözülür; check-in branch'inin public odaları +:
- `lastMessagePreview`, `lastMessageAt`, `lastMessageSenderUserId`
- `hasNew`, `unreadCount`

### Private inbox
`GET /api/chatrooms/private-inbox`

Üye olunan Private + Group odalar, son mesaja göre sıralı, aynı önizleme alanları.

### Mesaj geçmişi (okundu işaretler)
`GET /api/chatrooms/messages/{roomId}`

Odayı açınca çağır; `LastReadAt` güncellenir → `hasNew` temizlenir.

### Mesaj gönder
`POST /api/chatrooms/messages/{roomId}` body: `{ "message": "..." }`

Server:
1. DB kaydı
2. `ReceiveMessage` → `chatroom:{id}` (sadece oda ekranı açık olanlar)
3. Public ise `BranchRoomPreviewUpdated` → `branch:{branchId}`
4. Private/Group ise `PrivateInboxUpdated` → üyelerin kişisel kanalları

### Private geo-lock
Private sohbet **lokasyondan bağımsız** devam eder (ileride premium bayrağı eklenebilir).  
Group için geo-lock hâlâ geçerlidir.

---

## 6) Önerilen mobil ekran akışları

### Branch chat listesi
1. `GET /api/chatrooms/public`
2. `JoinBranchChannel(branchId)`
3. `BranchRoomPreviewUpdated` dinle → liste satırını güncelle
4. Bir odaya tıkla → sohbet ekranı

### Private inbox
1. `GET /api/chatrooms/private-inbox`
2. `PrivateInboxUpdated` dinle (hub zaten connect)
3. Odaya tıkla → sohbet ekranı

### Sohbet ekranı
1. (Public ise) gerekirse `POST .../join/{roomId}`
2. `JoinRoomGroup(roomId)`
3. `GET .../messages/{roomId}`
4. `ReceiveMessage` dinle
5. Çıkışta `LeaveRoomGroup(roomId)`

### Branch'ten ayrılınca
1. `LeaveBranchChannel(branchId)`
2. Açık oda varsa `LeaveRoomGroup`

---

## 7) Checklist

- [ ] Connect sonrası kişisel kanal çalışıyor mu?
- [ ] Branch listesinde **sadece** `JoinBranchChannel` var mı? (her odaya join yok)
- [ ] Private inbox için gereksiz branch/room join yok mu?
- [ ] Oda açıkken `JoinRoomGroup` + `ReceiveMessage`?
- [ ] `message.id` ile duplicate engeli?
- [ ] Reconnect sonrası aktif branch/room join yenileniyor mu?
- [ ] Public list / private-inbox HTTP alanları parse ediliyor mu?

---

## 8) Neden böyle?

- Liste için 10 odaya SignalR join = gereksiz yük.
- Private full mesajın branch listesine gitmesi = yanlış kanal.
- Hafif preview event + HTTP ilk yükleme = badge/son mesaj için yeterli; full stream sadece açık odada.
