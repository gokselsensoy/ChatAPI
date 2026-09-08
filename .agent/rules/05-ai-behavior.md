# 5. YAPAY ZEKA (AI) DAVRANIŞ KURALLARI (STRICT)

- **Eksik Kod Bırakma:** Kod üretirken `// ... existing code ...` veya `// ... diğer ayarlar ...` gibi geçiştirmeler yapma. Kodu çalıştığı haliyle, TAM blok olarak ver.
- **İçe Aktarmalar (Usings):** Yeni bir sınıf/metot/arayüz kullandığında gerekli C# `using` satırlarını dosyanın en üstüne mutlaka ekle.
- **Adlandırma ve DI:** Interface'ler `I` ile başlamalı, sınıflardaki private field'lar `_` ile başlamalıdır (Örn: `_userRepository`). Yeni bir Repository veya Service oluşturduğunda, DI (Dependency Injection) kayıtlarına (`AddScoped` vs.) eklenmesi gerektiğini mutlaka göz önünde bulundur/hatırlat.
