using Project.Bll.Interfaces;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;

namespace Project.Bll
{
    public class RoleService : IRoleService
    {
        private static readonly string[] BuiltInRoleNames = ["User", "Donor", "Admin"];
        private readonly IRoleDal _roleDal;

        public RoleService(IRoleDal roleDal)
        {
            _roleDal = roleDal;
        }

        public async Task<Result<Role>> GetAllRolesAsync()
        {
            var roles = await _roleDal.GetAllRolesAsync();
            return new Result<Role>
            {
                Success = true,
                Message = "Roles retrieved successfully.",
                Data = roles
            };
        }

        public async Task<Result<Role>> GetRoleByIdAsync(int id)
        {
            if (id <= 0)
            {
                return Failure("Invalid role ID.");
            }

            var role = await _roleDal.GetRoleByIdAsync(id);
            return role == null
                ? Failure($"Role with ID {id} was not found.")
                : Success(role, "Role retrieved successfully.");
        }

        public async Task<Result<Role>> CreateRoleAsync(RoleDto roleDto, int createdBy)
        {
            if (!IsValid(roleDto) || createdBy <= 0)
            {
                return Failure("Valid role details and an authenticated creator are required.");
            }

            var name = roleDto.Name.Trim();
            if (IsBuiltInRole(name))
            {
                return Failure("Built-in roles are created by startup seeding.");
            }

            if (await _roleDal.RoleNameExistsAsync(name))
            {
                return Failure($"A role named '{name}' already exists.");
            }

            var role = new Role
            {
                Name = name,
                Description = roleDto.Description.Trim(),
                IsActive = roleDto.IsActive,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            return Success(
                await _roleDal.CreateRoleAsync(role),
                "Role created successfully.");
        }

        public async Task<Result<Role>> UpdateRoleAsync(int id, RoleDto roleDto)
        {
            if (id <= 0 || !IsValid(roleDto))
            {
                return Failure("A valid role ID and role details are required.");
            }

            var role = await _roleDal.GetRoleByIdAsync(id);
            if (role == null)
            {
                return Failure($"Role with ID {id} was not found.");
            }

            var name = roleDto.Name.Trim();
            var isBuiltInRole = IsBuiltInRole(role.Name);
            if (isBuiltInRole &&
                (!string.Equals(name, role.Name, StringComparison.Ordinal) || !roleDto.IsActive))
            {
                return Failure("Built-in role names and active status cannot be changed.");
            }

            if (!isBuiltInRole && IsBuiltInRole(name))
            {
                return Failure("Custom roles cannot be renamed to a built-in role name.");
            }

            if (await _roleDal.RoleNameExistsAsync(name, id))
            {
                return Failure($"A role named '{name}' already exists.");
            }

            if (role.IsActive && !roleDto.IsActive &&
                await _roleDal.IsRoleAssignedToUsersAsync(id))
            {
                return Failure("A role assigned to users cannot be deactivated.");
            }

            role.Name = name;
            role.Description = roleDto.Description.Trim();
            role.IsActive = roleDto.IsActive;
            role.UpdatedAt = DateTime.UtcNow;

            return Success(
                await _roleDal.UpdateRoleAsync(role),
                "Role updated successfully.");
        }

        public async Task<Result<Role>> DeleteRoleAsync(int id)
        {
            if (id <= 0)
            {
                return Failure("Invalid role ID.");
            }

            var role = await _roleDal.GetRoleByIdAsync(id);
            if (role == null)
            {
                return Failure($"Role with ID {id} was not found.");
            }

            if (IsBuiltInRole(role.Name))
            {
                return Failure("Built-in roles cannot be deleted.");
            }

            if (await _roleDal.IsRoleAssignedToUsersAsync(id))
            {
                return Failure("A role assigned to users cannot be deleted.");
            }

            var now = DateTime.UtcNow;
            role.IsActive = false;
            role.IsDeleted = true;
            role.DeletedAt = now;
            role.UpdatedAt = now;

            return Success(
                await _roleDal.UpdateRoleAsync(role),
                "Role deleted successfully.");
        }

        public Task SeedDefaultRolesAsync()
        {
            var now = DateTime.UtcNow;
            IReadOnlyCollection<Role> defaults =
            [
                CreateDefaultRole("User", "Standard user role", now),
                CreateDefaultRole("Donor", "Gift donor role", now),
                CreateDefaultRole("Admin", "Administrator role", now)
            ];

            return _roleDal.SeedDefaultRolesAsync(defaults);
        }

        private static Role CreateDefaultRole(string name, string description, DateTime createdAt) =>
            new()
            {
                Name = name,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedAt = createdAt,
                CreatedBy = 0
            };

        private static bool IsValid(RoleDto? roleDto) =>
            roleDto != null &&
            !string.IsNullOrWhiteSpace(roleDto.Name) &&
            !string.IsNullOrWhiteSpace(roleDto.Description);

        private static bool IsBuiltInRole(string name) =>
            BuiltInRoleNames.Contains(name, StringComparer.OrdinalIgnoreCase);

        private static Result<Role> Success(Role role, string message) =>
            new()
            {
                Success = true,
                Message = message,
                Data = new[] { role }
            };

        private static Result<Role> Failure(string message) =>
            new()
            {
                Success = false,
                Message = message,
                Data = null
            };
    }
}
