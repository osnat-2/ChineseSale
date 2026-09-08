using AutoMapper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project.Bll.Interfaces;
using Project.Dal;
using Project.Dal.Interfaces;
using Project.Dto;
using Project.Models;
using Project.Validators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Project.Bll
{
    public class UserService : IUserService
    {
        private const string UserRole = "User";
        private const string DonorRole = "Donor";
        private const string AdminRole = "Admin";
        private readonly JWTSettings _jwtSettings;  // הוספת שדה להגדרות ה־JWT
        private readonly IUserDal _userDal;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        public UserService(IOptions<JWTSettings> jwtSettings, IUserDal user, IMapper mapper, ILogger<UserService> logger)
        {
            _jwtSettings = jwtSettings.Value;
            _userDal = user;
            _mapper = mapper;
            _logger = logger;
        }


        public async Task<Result<string>> Login(string email, string password)
        {
            if (!Validator.ValidEmail(email) || string.IsNullOrWhiteSpace(password))
            {
                return new Result<string>
                {
                    Success = false,
                    Message = "Email and password are required",
                    Data = null
                };
            }

            var user = await _userDal.GetUserByEmail(email);
            if (user == null)
            {
                return new Result<string>
                {
                    Success = false,
                    Message = "Invalid email or password format"
                };
            }

            // השוואת סיסמאות: האם הסיסמה שהוזנה תואמת לסיסמה המוצפנת
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);

            if (!isPasswordValid)
            {
                _logger.LogWarning($"Failed login attempt for email: {email}. Incorrect password.");
                return new Result<string>
                {
                    Success = false,
                    Message = "One or more of the identification details are incorrect. Please try again."
                };
            }

            // יצירת טוקן JWT
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role?.Name ?? UserRole),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.MobilePhone, user.Phone ?? ""),
                    new Claim("isActive", user.IsActive.ToString().ToLower()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
                Expires = DateTime.Now.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation($"User with email {email} logged in successfully.");
            return new Result<string>
            {
                Success = true,
                Message = tokenString
            };
        }

        public async Task<Result<User>> Register(UserDto userDto)
        {
            if (userDto != null)
            {
                var user = await _userDal.GetUserByEmail(userDto.Email);
                if (user == null && Validator.ValidateData(userDto.Name, userDto.Email, userDto.Phone))
                {
                    var u = _mapper.Map<User>(userDto);
                    if (u == null)
                        return new Result<User>
                        {
                            Success = false,
                            Message = "failed to map object",
                            Data = null
                        };
                    return await CreateUserAsync(u, UserRole);
                }
            }
            return new Result<User>
            {
                Success = false,
                Message = "Error details",
                Data = null
            };
        }

        public async Task<Result<User>> AddDonor(UserDto userDto) => await AddPrivilegedUserAsync(userDto, DonorRole);

        public async Task<Result<User>> AddAdmin(UserDto userDto) => await AddPrivilegedUserAsync(userDto, AdminRole);

        private async Task<Result<User>> AddPrivilegedUserAsync(UserDto userDto, string roleName)
        {
            if (userDto == null || !Validator.ValidateData(userDto.Name, userDto.Email, userDto.Phone))
            {
                return new Result<User> { Success = false, Message = "Invalid user details", Data = null };
            }

            if (await _userDal.GetUserByEmail(userDto.Email) != null)
            {
                return new Result<User> { Success = false, Message = "Email already exists", Data = null };
            }

            var user = _mapper.Map<User>(userDto);
            return await CreateUserAsync(user, roleName);
        }

        private async Task<Result<User>> CreateUserAsync(User user, string roleName)
        {
            var role = await _userDal.GetRoleByName(roleName);
            if (role == null)
            {
                return new Result<User> { Success = false, Message = $"Role '{roleName}' was not found", Data = null };
            }

            user.RoleId = role.Id;
            user.IsActive = true;
            user.IsDeleted = false;
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            return await _userDal.Register(user);
        }
    }
}