using Application.Abstractions.QueryRepositories;
using Domain.Repositories;
using Domain.SeedWork;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.QueryRepositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"), // Burası aynı kalsın
                    npgsqlOptions =>
                    {
                        npgsqlOptions.UseNetTopologySuite();

                        // BURAYI EKLE/GÜNCELLE:
                        npgsqlOptions.CommandTimeout(180); // 3 Dakika bekleme süresi veriyoruz
                        //npgsqlOptions.EnableRetryOnFailure(3); // Hata olursa 3 kere daha dene
                    }
                ));

            services.AddScoped<IUnitOfWork>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IBrandQueryRepository, BrandQueryRepository>();

            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IBranchQueryRepository, BranchQueryRepository>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();

            services.AddScoped<IChatRoomRepository, ChatRoomRepository>();
            services.AddScoped<IChatRoomQueryRepository, ChatRoomQueryRepository>();

            services.AddScoped<IChatRoomInviteRepository, ChatRoomInviteRepository>();

            // 5. Diğer servisler (Email vb.)
            // services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
