# 1. BACKEND MİMARİ STANDARTLARI (CLEAN ARCHITECTURE, CQRS & DDD)

- **Katmanlı Mimari:** Proje Clean Architecture prensiplerine uygundur. Domain katmanı asla dışarıya bağımlı olamaz.
- **DbContext İzolasyonu:** `DbContext` (Örn: `ApplicationDbContext`) sadece ve sadece **Repository** ve **QueryRepository** sınıfları içerisinde kullanılabilir. Handler'lar veya Controller'lar içinde ASLA DbContext çağrılamaz veya inject edilemez.
- **Domain Mantığı (DDD):** İş kuralları (business logic) Entity'ler içinde olmalıdır. Entity'lerin property setter'ları `private` olmalıdır. Nesneler sadece kendi içlerindeki metotlarla (Örn: `Menu.AddItem(...)`) veya Factory metotlarla (`Menu.Create(...)`) oluşturulmalı/güncellenmelidir. Sınıflardaki boş constructor'lar EF Core için `private` bırakılmalıdır.
- **Aggregate Root & Include:** Bir Aggregate Root (Örn: `Menu`) üzerinden alt elemanlara (Örn: `MenuItem`) işlem yapılacaksa, Command Repository'den nesne çekilirken mutlaka alt elemanlar `.Include()` ile getirilmelidir.
