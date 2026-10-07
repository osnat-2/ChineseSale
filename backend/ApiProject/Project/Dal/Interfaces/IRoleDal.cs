using Project.Models;

namespace Project.Dal.Interfaces
{
    public interface IRoleDal
    {
        Task<IReadOnlyList<Role>> GetAllRolesAsync();
        Task<Role?> GetRoleByIdAsync(int id);
        Task<bool> RoleNameExistsAsync(string name, int? excludedRoleId = null);
        Task<bool> IsRoleAssignedToUsersAsync(int roleId);
        Task<Role> CreateRoleAsync(Role role);
        Task<Role> UpdateRoleAsync(Role role);
        Task SeedDefaultRolesAsync(IReadOnlyCollection<Role> defaultRoles);
    }
}
