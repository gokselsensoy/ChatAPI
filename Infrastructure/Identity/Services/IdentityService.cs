using Application.Abstractions.Services;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task UpdateUserNameAsync(Guid userId, string newUserName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                // İstersen hata fırlatabilirsin ama Domain Eventler genellikle sessiz çalışır
                return;
            }

            if (user.UserName != newUserName)
            {
                var result = await _userManager.SetUserNameAsync(user, newUserName);
                if (result.Succeeded)
                {
                    await _userManager.UpdateAsync(user);
                }
            }
        }

        public async Task<bool> IsUserNameUniqueAsync(string userName, Guid currentUserId)
        {
            // Bu isme sahip bir kullanıcı var mı?
            var user = await _userManager.FindByNameAsync(userName);

            // Kullanıcı yoksa isim boştur -> true
            if (user == null) return true;

            // Kullanıcı varsa ama bu "bizim" kullanıcımızsa sorun yok -> true
            // (Yani kendi adımızı değiştirmeden güncelle tuşuna basmışızdır)
            if (user.Id == currentUserId) return true;

            // Başkası almış -> false
            return false;
        }
    }
}
