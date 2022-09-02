using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entity;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AppUserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.UserName)) return BadRequest("Username is taken");

            using var hmac = new HMACSHA512();

            var appUser = new AppUser
            {
                UserName = registerDto.UserName,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };
            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync();

            return new AppUserDto
            {
                UserName = appUser.UserName,
                Token = _tokenService.CreateToken(appUser)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<AppUserDto>> Login(LoginDto loginDto)
        {
            var appUser = await _context.AppUsers.SingleOrDefaultAsync(userResponse => userResponse.UserName == loginDto.UserName);

            if (appUser == null) return Unauthorized("Invalid Username");

            using var hmac = new HMACSHA512(appUser.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != appUser.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new AppUserDto
            {
                UserName = appUser.UserName,
                Token = _tokenService.CreateToken(appUser)
            };

        }

        private async Task<bool> UserExists(string userName)
        {
            return await _context.AppUsers.AnyAsync(appUser => appUser.UserName.ToLower() == userName.ToLower());
        }


    }
}