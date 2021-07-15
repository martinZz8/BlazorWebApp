using BlazorWebApp.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace BlazorWebApp.Server.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user != null)
            {
                if(VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                {
                    response.Data = CreateToken(user);
                    response.Message = "Successfully login!";
                }
                else
                {
                    response.Success = false;
                    response.Message = "Wrong password."; //Message should be set to: "Wrong input data."
                }
            }
            else
            {
                response.Success = false;
                response.Message = "User not found"; //Message should be set to: "Wrong input data."
            }

            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            if (!await UserExist(user.Email))
            {
                CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return new ServiceResponse<int> { Data=user.Id, Message="Registration successful!" };
            }
            else
            {
                return new ServiceResponse<int> { Success=false, Message="User with given email already exists." };
            }            
        }

        public async Task<bool> UserExist(string email)
        {
            if (await _context.Users.AnyAsync(u => u.Email.ToLower().Equals(email)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for(int i=0;i<computedHash.Length;i++)
                {
                    if(computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
