using Microsoft.EntityFrameworkCore;
using Project.Dal.Interfaces;
using Project.Models;

namespace Project.Dal
{
    public class RoleDal : IRoleDal
    {
        private readonly AppDBContext _dbContext;

        public RoleDal(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Role>> GetAllRolesAsync() =>
            await _dbContext.Role
                .AsNoTracking()
                .OrderBy(role => role.Name)
                .ToListAsync();

        public Task<Role?> GetRoleByIdAsync(int id) =>
            _dbContext.Role.FirstOrDefaultAsync(role => role.Id == id);

        public async Task<bool> RoleNameExistsAsync(string name, int? excludedRoleId = null)
        {
            var existingNames = await _dbContext.Role
                .IgnoreQueryFilters()
                .Where(role =>
                    !role.IsDeleted &&
                    (excludedRoleId == null || role.Id != excludedRoleId.Value))
                .Select(role => role.Name)
                .ToListAsync();

            return existingNames.Any(existingName =>
                string.Equals(existingName, name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Task<bool> IsRoleAssignedToUsersAsync(int roleId) =>
            _dbContext.User
                .IgnoreQueryFilters()
                .AnyAsync(user => user.RoleId == roleId);

        public async Task<Role> CreateRoleAsync(Role role)
        {
            await _dbContext.Role.AddAsync(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateRoleAsync(Role role)
        {
            _dbContext.Role.Update(role);
            await _dbContext.SaveChangesAsync();
            return role;
        }

        public async Task SeedDefaultRolesAsync(IReadOnlyCollection<Role> defaultRoles)
        {
            var existingRoles = await _dbContext.Role
                .IgnoreQueryFilters()
                .ToListAsync();
            var now = DateTime.UtcNow;
            var changed = false;

            foreach (var defaultRole in defaultRoles)
            {
                var matches = existingRoles
                    .Where(role => string.Equals(
                        role.Name,
                        defaultRole.Name,
                        StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matches.Count > 1)
                {
                    throw new InvalidOperationException(
                        $"Multiple records exist for built-in role '{defaultRole.Name}'.");
                }

                if (matches.Count == 0)
                {
                    await _dbContext.Role.AddAsync(defaultRole);
                    existingRoles.Add(defaultRole);
                    changed = true;
                    continue;
                }

                var existingRole = matches[0];
                var roleChanged = false;
                if (!string.Equals(existingRole.Name, defaultRole.Name, StringComparison.Ordinal))
                {
                    existingRole.Name = defaultRole.Name;
                    roleChanged = true;
                }

                if (!existingRole.IsActive)
                {
                    existingRole.IsActive = true;
                    roleChanged = true;
                }

                if (existingRole.IsDeleted)
                {
                    existingRole.IsDeleted = false;
                    roleChanged = true;
                }

                if (existingRole.DeletedAt != null)
                {
                    existingRole.DeletedAt = null;
                    roleChanged = true;
                }

                if (roleChanged)
                {
                    existingRole.UpdatedAt = now;
                    changed = true;
                }
            }

            if (changed)
            {
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
