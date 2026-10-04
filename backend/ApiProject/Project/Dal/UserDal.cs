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

        public async Task<User?> GetUserByEmail(string email)
        {
            logger.LogInformation("GetUserByEmail function");
            try
            {
                return await dbContext.User.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
            }

            catch
            {
                logger.LogError("could not find check duplicate email");
                return null;
            }
        }

        public async Task<Role?> GetRoleByName(string name)
        {
            return await dbContext.Role.FirstOrDefaultAsync(role => role.Name == name && role.IsActive);
        }

        public async Task<Result<User>> Register(User user)
        {
            try
            {
                await dbContext.User.AddAsync(user);
                await dbContext.SaveChangesAsync();

                logger.LogInformation($"User with email {user.Email} registered successfully.");
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
                return new Result<User>
                {
                    Success = false,
                    Message = "An error occurred during the registration process. Please try again later.",
                    Data = null
                };
            }
        }
    }
}
