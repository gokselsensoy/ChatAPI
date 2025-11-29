# Faz I: Derleme (SDK ortamı)
# .NET 9 SDK ortamını başlatıyoruz
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Deponun tüm içeriğini Docker konteynerine kopyala
COPY . .

# Çalışma dizinini, derlenecek WebApi projesinin bulunduğu klasöre taşıyoruz.
# Bu, WebApi.csproj dosyasındaki göreceli referansların (../Application/Application.csproj) 
# doğru şekilde çalışmasını sağlamak için kritik.
WORKDIR /src/WebApi 

# Yayınlama işlemi (restore da dahil)
# WebApi.csproj dosyasının adını kullanarak yayınlama yaparız.
RUN dotnet publish WebApi.csproj -c Release -o /app/publish

# Faz II: Çalıştırma (Runtime ortamı)
# Sadece uygulamayı çalıştırmak için gereken hafif runtime ortamını kullanıyoruz
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Sadece yayınlanmış çıktıları kopyala
COPY --from=build /app/publish .

# Uygulamayı başlatma komutu
# DLL adınız muhtemelen WebApi.dll'dir.
ENTRYPOINT ["dotnet", "WebApi.dll"]