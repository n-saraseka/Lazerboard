using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Lazerboard.Data.Services;

public class MigrationService
{
    private readonly IUserRepository _userRepository;
    private readonly int _adminUserId;

    public MigrationService(IUserRepository userRepository, IConfiguration config)
    {
        _userRepository = userRepository;
        _adminUserId = config.GetValue<int>("AdminUserId");
    }

    public async Task SetAdminUserAsync()
    {
        var user = await _userRepository.GetByIdAsync(_adminUserId);
        if (user == null) return;
        var userGroups = user.UserGroups;
        if (userGroups.Any(g => g == UserGroup.Administrator)) return;
        userGroups.Add(UserGroup.Administrator);
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();
    }
}