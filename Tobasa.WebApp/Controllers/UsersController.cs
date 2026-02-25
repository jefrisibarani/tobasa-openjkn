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

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Tobasa.App;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Tobasa.Services;
using Tobasa.Entities;
using Tobasa.Models.Users;
using Microsoft.Extensions.Configuration;

namespace Tobasa.Controllers
{
    // api/users
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService            _userService;
        private IMapper                 _mapper;
        private readonly AppSettings    _appSettings;
        private readonly IConfiguration _configuration;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings,
            IConfiguration configuration)
        {
            _userService   = userService;
            _mapper        = mapper;
            _appSettings   = appSettings.Value;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        //[RestrictToLocalhost]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);
            if (user == null)
            {
                return new ApiNegativeResult("Username or password is incorrect");
            }

            var tokenString = _userService.GenerateJwt(user.Id, user.UserName);

            /*
            return Ok(new
            {
                Response = new { TokenAntrian = tokenString },
                Metadata = new { Message = "Ok", Code = 200 }
            });
            */

            return new ApiResult(new { Token = tokenString });

            // AppException and other Exceptions will be handled by ErrorHandlerMiddleWare
        }

        /*
            {
                "firstName" : "Admin",
                "lastName"  : "Tobasa",
                "username"  : "admin",
                "password"  : "@JKKJK65658DKFJUR",
                "email"     : "admin@mangapul.net"
            }
        */
        [AllowAnonymous]
        [HttpPost("register")]
        [RestrictToLocalhost]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            var user = _mapper.Map<BaseUsers>(model);
            _userService.Create(user, model.Password);

            return new ApiResult("User created");
        }

        [Authorize]
        [HttpGet]
        [RestrictToLocalhost]
        public IActionResult GetAll()
        {
            var loggedInUser = (BaseUsers)HttpContext.Items["User"];
            if (loggedInUser.UserName == "admin")
            {
                var users = _userService.GetAll();
                var model = _mapper.Map<IList<UserModel>>(users);

                return new ApiResult(new { List = model } );
            }

            return new ApiNegativeResult("No Access!");
        }

        [Authorize]
        [HttpGet("{id}")]
        [RestrictToLocalhost]
        public IActionResult GetById(int id)
        {
            var loggedInUser = (BaseUsers)HttpContext.Items["User"];
            if (loggedInUser.Id == id || loggedInUser.UserName == "admin")
            {
                var user = _userService.GetById(id);
                var model = _mapper.Map<UserModel>(user);

                if( user==null)
                    return new ApiNegativeResult("User not found");

                return new ApiResult(new { User = model });
            }

            return new ApiNegativeResult("No Access!");
        }

        [Authorize]
        [HttpPut("{id}")]
        [RestrictToLocalhost]
        public IActionResult Update(int id, [FromBody]UpdateModel model)
        {
            var loggedInUser = (BaseUsers)HttpContext.Items["User"];
            if (loggedInUser.Id == id || loggedInUser.UserName == "admin")
            {
                // map model to entity and set id
                var user = _mapper.Map<BaseUsers>(model);
                user.Id = id;

                _userService.Update(user, model.Password);
                return new ApiResult("User updated");
            }

            return new ApiNegativeResult("No Access!");
        }

        [Authorize]
        [HttpDelete("{id}")]
        [RestrictToLocalhost]
        public IActionResult Delete(int id)
        {
            var loggedInUser = (BaseUsers)HttpContext.Items["User"];
            if (loggedInUser.UserName == "admin" && id != 1)
            {
                _userService.Delete(id);

                return new ApiResult("User deleted");
            }

            return new ApiNegativeResult("No Access!");
        }

        ///  http://localhost:8084/api/users/encrypt?data=source
        [AllowAnonymous]
        [RestrictToLocalhost]
        [HttpGet("encrypt")]
        public IActionResult Encrypt(string data)
        {
            string securitySalt = _configuration["AppSettings:SecuritySalt"];
            string encrypted = Utils.EncryptPassword(data, securitySalt);
            
            return new ApiResult(new { Source = data, Encrypted = encrypted });
        }


        ///  http://localhost:8084/api/users/decrypt?data=5ee5bbb6020724399634e489b60bcc2e
        [AllowAnonymous]
        [RestrictToLocalhost]
        [HttpGet("decrypt")]
        public IActionResult Decrypt(string data)
        {
            string securitySalt = _configuration["AppSettings:SecuritySalt"];
            string decrypted = Utils.DecryptPassword(data, securitySalt);

            return new ApiResult(new { Source = data, Decrypted = decrypted });
        }
    }
}