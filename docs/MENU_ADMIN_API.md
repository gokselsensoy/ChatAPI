# Menü yönetimi API’si (marka sahibi ve şube admini)

Bu doküman, yönetim paneli / mobil admin ekranları için eklenen **menü listeleme** uçlarını açıklar. Amaç: marka sahibinin tüm şubeleri görmesi; atanmış şube yöneticisinin ise yalnızca yetkili olduğu şubenin menüsünü görmesi.

---

## Kim neyi kullanır?

| Rol | Açıklama | Hangi endpoint? |
|-----|----------|------------------|
| **Marka sahibi** | `Brand.OwnerUserId`, yani domain `User` tablosundaki kullanıcı ID’si markanın sahibi olarak kayıtlı. | Tüm şubeler: `GET .../admin/by-brand` |
| **Şube admini (atanmış)** | `BranchAdminMap` tablosunda o şube için kayıt var; marka sahibi değil. | Sadece o şube: `GET .../admin/branches/{branchId}` |
| **Marka sahibi (tek şube)** | İster toplu listeyi, ister tek şube endpoint’ini kullanabilir. | İkisi de |

**Not:** Şube admini, “tüm şubeleri markaya göre grupla” endpoint’ine **erişemez**; bu ekran yalnızca marka sahibine açıktır. Atanmış admin, yönettiği her şube için ayrı ayrı `branchId` ile istek atar.

---

## Endpoint’ler

Temel yol: `api/menu` (controller route’u projenizdeki gibi, genelde `/api/menu`).

### 1) Marka sahibi — tüm şubeler, şubeye göre gruplanmış

```http
GET /api/menu/admin/by-brand
Authorization: Bearer <access_token>
```

**Yetki:** Token’daki kullanıcı gerçekten bir markanın sahibi olmalı (`IsUserBrandOwnerAsync`). Aksi halde yetkisiz erişim hatası döner.

**Cevap:** `BranchMenusGroupDto` listesi. Her eleman bir şubeyi temsil eder:

- `branchId`, `branchName` — şube bilgisi  
- `brandId`, `brandName` — bağlı marka (birden fazla markası olan sahip için ayırt etmek kolay)  
- `menus` — o şubeye ait `MenuDto` listesi; her menünün içinde `menuItems` (ürün adı, açıklama, kategori, fiyat, dosya id vb.) bulunur  

Boş şubeler de listelenebilir (`menus` boş liste olabilir).

---

### 2) Marka sahibi veya şube admini — tek şubenin menüleri

```http
GET /api/menu/admin/branches/{branchId}
Authorization: Bearer <access_token>
```

**Yetki:** `CanUserManageBranchAsync`:

- Şubenin markasının sahibi **veya**  
- `BranchAdminMap` içinde `branchId` + kullanıcı eşleşmesi  

**Cevap:** `MenuDto` listesi (ilgili şubenin tüm menüleri ve her menüde `menuItems`).

---

## İstek örnekleri

Her iki uç da **GET** olduğu için **gövde (body) yoktur**. Kimlik, `Authorization: Bearer` ile taşınır.

### Marka sahibi — tüm şubeler

**Tam URL örneği** (sunucu adresinize göre değiştirin):

```http
GET https://localhost:5001/api/menu/admin/by-brand
Authorization: Bearer eyJhbGciOi...
Accept: application/json
```

**cURL:**

```bash
curl -sS -X GET "https://localhost:5001/api/menu/admin/by-brand" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Accept: application/json"
```

**JavaScript (`fetch`):**

```javascript
const res = await fetch(`${baseUrl}/api/menu/admin/by-brand`, {
  method: "GET",
  headers: {
    Authorization: `Bearer ${accessToken}`,
    Accept: "application/json",
  },
});
const data = await res.json(); // BranchMenusGroupDto[]
```

---

### Şube admini veya marka sahibi — tek şube

`{branchId}` yerine gerçek şube GUID’ini koyun.

```http
GET https://localhost:5001/api/menu/admin/branches/3fa85f64-5717-4562-b3fc-2c963f66afa6
Authorization: Bearer eyJhbGciOi...
Accept: application/json
```

**cURL:**

```bash
curl -sS -X GET "https://localhost:5001/api/menu/admin/branches/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Accept: application/json"
```

**JavaScript (`fetch`):**

```javascript
const branchId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
const res = await fetch(`${baseUrl}/api/menu/admin/branches/${branchId}`, {
  method: "GET",
  headers: {
    Authorization: `Bearer ${accessToken}`,
    Accept: "application/json",
  },
});
const data = await res.json(); // MenuDto[]
```

---

## Örnek cevap gövdeleri (JSON)

Aşağıdaki alan adları, ASP.NET Core’un varsayılan **camelCase** JSON serileştirmesine göre yazılmıştır (`menuItems`, `branchId`, vb.).

### 1) `GET /api/menu/admin/by-brand` — `200 OK`

Dizi: her eleman bir şube grubu; içinde o şubenin menüleri ve her menüde ürünler.

```json
[
  {
    "branchId": "a1111111-1111-1111-1111-111111111111",
    "branchName": "Kadıköy Şubesi",
    "brandId": "b2222222-2222-2222-2222-222222222222",
    "brandName": "Örnek Kahve",
    "menus": [
      {
        "id": "c3333333-3333-3333-3333-333333333333",
        "branchId": "a1111111-1111-1111-1111-111111111111",
        "title": "Ana Menü",
        "description": "Günlük ürünler",
        "menuType": "Food",
        "menuUrl": null,
        "fileId": "menu-cover-123",
        "menuItems": [
          {
            "id": "d4444444-4444-4444-4444-444444444444",
            "name": "Filtre Kahve",
            "description": "Orta kavrum",
            "categoryType": "Beverage",
            "price": 85.5,
            "fileId": "item-photo-456"
          }
        ]
      }
    ]
  },
  {
    "branchId": "e5555555-5555-5555-5555-555555555555",
    "branchName": "Beşiktaş Şubesi",
    "brandId": "b2222222-2222-2222-2222-222222222222",
    "brandName": "Örnek Kahve",
    "menus": []
  }
]
```

- İkinci şubede `menus` boş dizi olabilir (henüz menü yoksa).

---

### 2) `GET /api/menu/admin/branches/{branchId}` — `200 OK`

Tek şubenin menü listesi; yapı, yukarıdaki her bir `menus` elemanıyla aynı tipte **dizi**.

```json
[
  {
    "id": "c3333333-3333-3333-3333-333333333333",
    "branchId": "a1111111-1111-1111-1111-111111111111",
    "title": "Ana Menü",
    "description": "Günlük ürünler",
    "menuType": "Food",
    "menuUrl": null,
    "fileId": "menu-cover-123",
    "menuItems": [
      {
        "id": "d4444444-4444-4444-4444-444444444444",
        "name": "Filtre Kahve",
        "description": "Orta kavrum",
        "categoryType": "Beverage",
        "price": 85.5,
        "fileId": "item-photo-456"
      },
      {
        "id": "f6666666-6666-6666-6666-666666666666",
        "name": "Kruvasan",
        "description": "Tereyağlı",
        "categoryType": "Pastry",
        "price": 45,
        "fileId": null
      }
    ]
  }
]
```

Menü hiç yoksa genelde `[]` döner (boş dizi).

---

### Hata durumları (özet)

| Durum | Tipik anlam |
|--------|----------------|
| `401 Unauthorized` | Token yok, süresi dolmuş veya geçersiz. |
| Yetkisiz işlem | Marka sahibi değilseniz `admin/by-brand` için; şube için marka sahibi veya `BranchAdminMap` yoksa tek şube ucu reddedilir (uygulama genelde `UnauthorizedAccessException` ile yanıt üretir; HTTP kodu global exception handler’a bağlıdır). |

İstemci tarafında `res.ok` / `res.status` ve hata gövdesini kontrol etmek iyi olur.

---

## Teknik notlar (kısa)

- Kullanıcı kimliği token’dan alınır; iş kurallarında domain **`User.Id`** kullanılır (JWT’deki `sub` / `NameIdentifier` önce profil üzerinden `User` kaydına çevrilir).
- Menü ve ürün satırları veritabanından **AutoMapper `ProjectTo`** ile DTO’ya yansıtılır; şube gruplamasında şube satırı için `Branch` → `BranchMenusGroupDto` eşlemesi kullanılır.
- Müşteri menüsü (`GET /api/menu` + müşteri sorgusu) bu yönetim uçlarından ayrıdır; check-in vb. kurallar orada geçerlidir.

---

## Özet cümle

- **Owner:** “Tüm şubelerimi bir ekranda marka/şube gruplu görmek istiyorum” → `admin/by-brand`.  
- **Sadece şube admini:** “Sadece bana atanan şubenin menüsünü düzenleyeceğim” → `admin/branches/{branchId}` (her atanmış şube için `branchId` değişir).  
- **BranchAdminMap** yalnızca **tek şube** endpoint’inin yetkilendirmesinde kullanılır; toplu liste sadece **marka sahibine** açıktır.
