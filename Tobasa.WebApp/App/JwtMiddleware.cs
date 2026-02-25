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

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tobasa.Data;

namespace Tobasa.App
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings     _appSettings;
        private readonly ILogger         _logger;

        public JwtMiddleware(RequestDelegate next, IOptions<AppSettings> appSettings, ILogger<JwtMiddleware> logger)
        {
            _next         = next;
            _appSettings  = appSettings.Value;
            _logger       = logger;
        }

        public async Task Invoke(HttpContext context, DataContextAntrian dataContext)
        {
            var jwtToken = "";
            // get jwt token first from x-token, else get from Authorization
            var token1 = context.Request.Headers["x-token"].FirstOrDefault();
            var token2 = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token1 != null) { 
                jwtToken = token1;
            }
            else if (token2 != null) { 
                jwtToken = token2;
            }

            await AttachAccountToContext(context, dataContext, jwtToken);

            await _next(context);
        }


        private async Task AttachAccountToContext(HttpContext context, DataContextAntrian dataContext, string token)
        {
            try
            {
                if (token.Length == 0) {
                    throw new AppException("TokenAntrian tidak ada pada header");
                }

                // Note: mJkn versi 2, menambahkan x-username header untuk authentikasi
                var xUserName    = "";
                bool apiVersion2 = false;
                var routePath    = context.Request.Path.Value;

                if (routePath.ToString().StartsWith("/api/"))
                {
                    apiVersion2 = true;
                    xUserName = context.Request.Headers["x-username"].FirstOrDefault();
                    
                    if (xUserName == null) {
                        throw new AppException("Username tidak ada pada header");
                    }
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.AuthJwtSecret);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(key),
                    ValidateIssuer           = true,
                    ValidIssuer              = _appSettings.AuthJwtIssuer,
                    ValidateAudience         = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew                = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userName = jwtToken.Claims.First(x => x.Type == "user_name").Value;
                var userId   = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // Note: mJkn versi 2, menambahkan x-username header untuk authentikasi
                // cocokkan x-username dengan user_name pada jwt token
                if (apiVersion2 && (xUserName != userName)) {
                    throw new AppException("Username pada header tidak valid");
                }

                // attach account to context on successful jwt validation
                var user = await dataContext.BaseUsers.FindAsync(userId);

                // Validate user account properties
                if (user == null)
                    throw new AppException("User Id tidak ditemukan");
                else if (user.UserName != userName)
                    throw new AppException("Username pada token tidak cocok");
                else if (user.Enabled == false)
                    throw new AppException("Account belum diaktivasi");
                else if (user.AllowLogin == false)
                    throw new AppException("Account tidak diizinkan login");
                else if (user.Expired < DateTime.Now)
                    throw new AppException("Account sudah expired");

                context.Items["User"] = user;
            }
            catch(Exception ex)
            {
                var errorMEssage = "";

                if (ex.GetType() == typeof(SecurityTokenExpiredException))
                {
                    StringValues expiredVal;
                    var success = context.Response.Headers.TryGetValue("X-TokenAntrian-Expired", out expiredVal);
                    if (!success)
                        context.Response.Headers.Add("X-TokenAntrian-Expired", "True");

                    errorMEssage = "TokenAntrian expired";
                }
                else if (ex.Message == "Username tidak ada pada header")
                {
                    StringValues failedVal;
                    var success = context.Response.Headers.TryGetValue("X-User-Failed", out failedVal);
                    if (!success)
                        context.Response.Headers.Add("X-User-Failed", ex.Message);

                    errorMEssage = ex.Message;
                }
                else if (ex.GetType() == typeof(AppException))
                {
                    StringValues failedVal;
                    var success = context.Response.Headers.TryGetValue("X-Auth-Failed", out failedVal);
                    if (!success)
                        context.Response.Headers.Add("X-Auth-Failed", ex.Message);

                    errorMEssage = ex.Message;
                }
                else
                {
                    StringValues errorVal;
                    var success = context.Response.Headers.TryGetValue("X-TokenAntrian-Error", out errorVal);
                    if (!success)
                    {
                        errorMEssage = ex.Message;
                        context.Response.Headers.Add("X-TokenAntrian-Error", "JWT TokenAntrian error");
                    }
                    else
                    {
                        errorMEssage = "Unexpected error";
                    }
                }

                var routePath = context.Request.Path.Value;

                if (errorMEssage == "TokenAntrian tidak ada pada header")
                {
                    if ( routePath.ToString().StartsWith("/api/fktp/token") ||
                         routePath.ToString().StartsWith("/api/fktrl/token") ||
                         routePath.ToString().StartsWith("/api/users/authenticate") )
                    {
                        _logger.LogTrace($"JwtMiddleware, {errorMEssage}");
                    }
                }
                else
                {
                    _logger.LogDebug($"JwtMiddleware, {errorMEssage}");
                }

                // Do nothing if jwt validation fails
                // account is not attached to context so request won't have access to secure routes
                // AuthorizeAttribute will do further processing 
            }
        }
    }
}