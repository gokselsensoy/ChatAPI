using Application.Abstractions.QueryRepositories;
using Domain.Repositories;
using Domain.SeedWork;
using Infrastructure.Persistence.Context;
using Infrastructure.Persistence.QueryRepositories;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))); // UseSqlServer olursa SqlServer paketini indir.

            services.AddScoped<IUnitOfWork>(sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IBrandQueryRepository, BrandQueryRepository>();

            services.AddScoped<IBranchRepository, BranchRepository>();
            services.AddScoped<IBranchQueryRepository, BranchQueryRepository>();

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserQueryRepository, UserQueryRepository>();

            // 5. Diğer servisler (Email vb.)
            // services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
