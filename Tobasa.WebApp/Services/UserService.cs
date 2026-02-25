/*
    Tobasa OpenJKN Bridge
    Copyright (C) 2020-2026 Jefri Sibarani
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System;
using Tobasa.App;
using Tobasa.Data;
using Tobasa.Entities;

namespace Tobasa.Services
{
    public interface IUserService
    {
        BaseUsers Authenticate(string username, string password);
        IEnumerable<BaseUsers> GetAll();
        BaseUsers GetById(int id);
        BaseUsers GetByUsername(string username);
        BaseUsers Create(BaseUsers user, string password);
        void Update(BaseUsers user, string password = null);
        void Delete(int id);
        string GenerateJwt(int userID, string userName);
    }

    public class UserService : IUserService
    {
        private DataContextAntrian   _context;
        private readonly AppSettings _appSettings;
        private readonly ILogger     _logger;

        public UserService(DataContextAntrian context, 
            IOptions<AppSettings> appSettings, 
            ILogger<MjknService> logger)
        {
            _context     = context;
            _appSettings = appSettings.Value;
            _logger      = logger;
        }

        public BaseUsers Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.BaseUsers.SingleOrDefault(x => x.UserName == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Validate user account properties
            if (user.Enabled == false)
                throw new AppException("Account belum diaktivasi");
            else if (user.AllowLogin == false)
                throw new AppException("Account tidak diizinkan login");
            else if (user.Expired < DateTime.Now)
                throw new AppException("Account sudah expired");

            // authentication successful
            _logger.LogInformation("UserService Authenticate User: " + username + " berhasil login ke system" );
            return user;
        }

        public IEnumerable<BaseUsers> GetAll()
        {
            return _context.BaseUsers;
        }

        public BaseUsers GetById(int id)
        {
            return _context.BaseUsers.Find(id);
        }

        public BaseUsers GetByUsername(string username)
        {
            var user = _context.BaseUsers.SingleOrDefault(x => x.UserName == username);
            return user;
        }

        public BaseUsers Create(BaseUsers user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_context.BaseUsers.Any(x => x.UserName == user.UserName))
                throw new AppException("Username " + user.UserName + " is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            user.Uuid = PushIDGen.GeneratePushID();

            _context.BaseUsers.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Update(BaseUsers userParam, string password = null)
        {
            var user = _context.BaseUsers.Find(userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.UserName) && userParam.UserName != user.UserName)
            {
                // throw error if the new username is already taken
                if (_context.BaseUsers.Any(x => x.UserName == userParam.UserName))
                    throw new AppException("Username " + userParam.UserName + " is already taken");

                user.UserName = userParam.UserName;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
                user.LastName = userParam.LastName;

            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.BaseUsers.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.BaseUsers.Find(id);
            if (user != null)
            {
                _context.BaseUsers.Remove(user);
                _context.SaveChanges();
            }
        }


        public string GenerateJwt(int userID, string userName)
        {
            var tokenHandler    = new JwtSecurityTokenHandler();
            var key             = Encoding.ASCII.GetBytes(_appSettings.AuthJwtSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _appSettings.AuthJwtIssuer,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id",        userID.ToString()),
                    new Claim("user_name", userName),
                    new Claim("developer", "jefrisibarani@gmail.com"),
                }),
                Expires = DateTime.UtcNow.AddMinutes(_appSettings.AuthJwtExpireTimeSpanMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("UserService GenerateJwt JWT token baru digenerate untuk user: " + userName);

            return tokenString;
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password)) 
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            if (storedHash.Length != 64) 
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

            if (storedSalt.Length != 128) 
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}