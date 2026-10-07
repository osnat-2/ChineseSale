using Project.Dto;
using Project.Models;

namespace Project.Bll.Interfaces
{
    public interface IRoleService
    {
        Task<Result<Role>> GetAllRolesAsync();
        Task<Result<Role>> GetRoleByIdAsync(int id);
        Task<Result<Role>> CreateRoleAsync(RoleDto roleDto, int createdBy);
        Task<Result<Role>> UpdateRoleAsync(int id, RoleDto roleDto);
        Task<Result<Role>> DeleteRoleAsync(int id);
        Task SeedDefaultRolesAsync();
    }
}
