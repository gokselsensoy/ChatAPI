# 2. ENTITY FRAMEWORK CORE & VERİTABANI (POSTGRESQL/SUPABASE)

- **ID Üretimi (KRİTİK):** Primary Key'ler (Id) daima `Guid` tipindedir ve Domain katmanında (`Guid.NewGuid()`) üretilir. Bu nedenle EF Core `Configuration` sınıflarında Id'ler için KESİNLİKLE `builder.Property(x => x.Id).ValueGeneratedNever();` kullanılmalıdır! (Aksi takdirde Supabase insert işlemlerinde Concurrency hatası fırlatır).
- **Backing Fields:** Domain içindeki `private readonly List<T>` alanları EF Core'a tanıtılmalıdır. (Örn: `builder.Metadata.FindNavigation(nameof(Menu.MenuItems))?.SetPropertyAccessMode(PropertyAccessMode.Field);`)
- **Query Repoları (Performans):** `I...QueryRepository` arayüzleri hiçbir temel sınıftan kalıtım almaz. Performans için okuma işlemlerinde `.AsNoTracking()` kullanımı ZORUNLUDUR. Soft delete varsa mutlaka `.Where(x => !x.IsDeleted)` filtresi eklenmelidir.
- **Unit of Work:** Yazma (Command) işlemlerinde değişikliklerin kaydedilmesi için mutlaka `UnitOfWork` kullanılmalıdır.
