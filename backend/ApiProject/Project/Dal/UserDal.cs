using BCrypt.Net;  // חשוב להשתמש ב-Bcrypt
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project.Dal.Interfaces;
using Project.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Project.Dal
{
    public class UserDal : IUserDal
    {
        private readonly AppDBContext dbContext;
        private readonly ILogger<UserDal> logger;
        //private readonly JWTSettings jwtSettings;  // הוספת שדה להגדרות ה־JWT

        public UserDal(AppDBContext context, ILogger<UserDal> logger)
        {
            dbContext = context;
            this.logger = logger;
            //this.jwtSettings = jwtSettings.Value;
        }

        public async Task<User?> GetUserById(int id)
        {
            logger.LogInformation("GetUserById function");
            try
            {
                return await dbContext.User
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while looking up a user by ID.");
                throw;
            }
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            logger.LogInformation("GetUserByEmail function");
            try
            {
                return await dbContext.User.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while looking up a user.");
                throw;
            }
        }

        public async Task<Role?> GetRoleByName(string name)
        {
            return await dbContext.Role.FirstOrDefaultAsync(role => role.Name == name && role.IsActive);
        }

        public Task<Role?> GetRoleById(int id) =>
            dbContext.Role.FirstOrDefaultAsync(role => role.Id == id);

        public async Task<Result<User>> Register(User user)
        {
            try
            {
                await dbContext.User.AddAsync(user);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("User registration completed successfully.");
                return new Result<User>
                {
                    Success = true,
                    Message = "User registered successfully.",
                    Data = new List<User> { user } // מחזירים את המשתמש שנרשם
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the registration process.");
                throw;
            }
        }

        public async Task<Result<User>> GetDonorsAsync()
        {
            try
            {
                var donors = await dbContext.User
                    .AsNoTracking()
                    .Include(user => user.Role)
                    .Where(user => user.Role != null && user.Role.Name == "Donor")
                    .OrderBy(user => user.Name)
                    .ToListAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "Donors fetched successfully.",
                    Data = donors
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading donors.");
                throw;
            }
        }

        public async Task<Result<User>> UpdateDonorAsync(int id, User donorDetails)
        {
            try
            {
                var donor = await dbContext.User
                    .Include(user => user.Role)
                    .FirstOrDefaultAsync(user =>
                        user.Id == id &&
                        user.Role != null &&
                        user.Role.Name == "Donor");

                if (donor == null)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Donor was not found.",
                        Data = null
                    };
                }

                var emailInUse = await dbContext.User.AnyAsync(user =>
                    user.Id != id && user.Email == donorDetails.Email);
                if (emailInUse)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Email already exists.",
                        Data = null
                    };
                }

                donor.Name = donorDetails.Name;
                donor.Phone = donorDetails.Phone;
                donor.Email = donorDetails.Email;
                donor.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "Donor updated successfully.",
                    Data = new[] { donor }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating donor {DonorId}.", id);
                throw;
            }
        }

        public async Task<Result<User>> DeleteDonorAsync(int id)
        {
            try
            {
                var donor = await dbContext.User
                    .Include(user => user.Role)
                    .FirstOrDefaultAsync(user =>
                        user.Id == id &&
                        user.Role != null &&
                        user.Role.Name == "Donor");

                if (donor == null)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Donor was not found.",
                        Data = null
                    };
                }

                var hasPresentReferences = await dbContext.Present.AnyAsync(
                    present => present.DonorId == donor.Id);
                if (hasPresentReferences)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Donor is assigned to one or more presents and cannot be deleted.",
                        Data = null
                    };
                }

                donor.IsActive = false;
                donor.IsDeleted = true;
                donor.DeletedAt = DateTime.UtcNow;
                donor.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "Donor deleted successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting donor {DonorId}.", id);
                throw;
            }
        }

        public async Task<Result<User>> GetAllManagedUsersAsync()
        {
            try
            {
                var users = await dbContext.User
                    .AsNoTracking()
                    .Include(user => user.Role)
                    .OrderBy(user => user.Name)
                    .ToListAsync();
                return new Result<User>
                {
                    Success = true,
                    Message = "Users fetched successfully.",
                    Data = users
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while loading managed users.");
                throw;
            }
        }

        public async Task<Result<User>> CreateManagedUserAsync(User user)
        {
            try
            {
                if (await dbContext.User.AnyAsync(existing => existing.Email == user.Email))
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Email already exists.",
                        Data = null
                    };
                }

                user.IsActive = true;
                user.IsDeleted = false;
                await dbContext.User.AddAsync(user);
                await dbContext.SaveChangesAsync();
                await dbContext.Entry(user).Reference(item => item.Role).LoadAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "User created successfully.",
                    Data = new[] { user }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while creating a managed user.");
                throw;
            }
        }

        public async Task<Result<User>> UpdateManagedUserAsync(int id, User userDetails)
        {
            try
            {
                var user = await dbContext.User
                    .Include(item => item.Role)
                    .FirstOrDefaultAsync(item => item.Id == id);
                if (user == null)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "User was not found.",
                        Data = null
                    };
                }

                if (await dbContext.User.AnyAsync(item => item.Id != id && item.Email == userDetails.Email))
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Email already exists.",
                        Data = null
                    };
                }

                var currentlyAdmin = string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);
                var willRemainActiveAdmin = string.Equals(
                    userDetails.Role?.Name,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase) && userDetails.IsActive;
                if (currentlyAdmin && user.IsActive && !willRemainActiveAdmin &&
                    !await HasOtherActiveAdminAsync(id))
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "The last active administrator cannot be deactivated or reassigned.",
                        Data = null
                    };
                }

                user.Name = userDetails.Name;
                user.Phone = userDetails.Phone;
                user.Email = userDetails.Email;
                user.RoleId = userDetails.RoleId;
                user.Role = userDetails.Role;
                user.IsActive = userDetails.IsActive;
                user.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "User updated successfully.",
                    Data = new[] { user }
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while updating managed user {UserId}.", id);
                throw;
            }
        }

        public async Task<Result<User>> DeleteManagedUserAsync(int id)
        {
            try
            {
                var user = await dbContext.User
                    .Include(item => item.Role)
                    .FirstOrDefaultAsync(item => item.Id == id);
                if (user == null)
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "User was not found.",
                        Data = null
                    };
                }

                var isActiveAdmin = user.IsActive &&
                    string.Equals(user.Role?.Name, "Admin", StringComparison.OrdinalIgnoreCase);
                if (isActiveAdmin && !await HasOtherActiveAdminAsync(id))
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "The last active administrator cannot be deleted.",
                        Data = null
                    };
                }

                var isDonor = string.Equals(user.Role?.Name, "Donor", StringComparison.OrdinalIgnoreCase);
                if (isDonor && await dbContext.Present.IgnoreQueryFilters()
                    .AnyAsync(present => present.DonorId == user.Id))
                {
                    return new Result<User>
                    {
                        Success = false,
                        Message = "Donors referenced by presents cannot be deleted.",
                        Data = null
                    };
                }

                user.IsActive = false;
                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();

                return new Result<User>
                {
                    Success = true,
                    Message = "User deleted successfully.",
                    Data = null
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while deleting managed user {UserId}.", id);
                throw;
            }
        }

        private async Task<bool> HasOtherActiveAdminAsync(int excludedUserId)
        {
            var adminRole = await dbContext.Role
                .FirstOrDefaultAsync(role => role.Name == "Admin");
            return adminRole != null && await dbContext.User.AnyAsync(user =>
                user.Id != excludedUserId &&
                user.RoleId == adminRole.Id &&
                user.IsActive);
        }
    }
}
