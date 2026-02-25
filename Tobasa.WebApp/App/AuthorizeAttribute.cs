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

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Net;
using Tobasa.App;
using Tobasa.Entities;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (BaseUsers)context.HttpContext.Items["User"];
        if (user == null)
        {
            // not logged in
            string msg = "Unauthorized";
            if ( context.HttpContext.Response.Headers.ContainsKey("X-TokenAntrian-Expired") )
            {
                StringValues expiredVal;
                var success = context.HttpContext.Response.Headers.TryGetValue("X-TokenAntrian-Expired", out expiredVal);
                if (success && expiredVal.ToString() == "True" ) {
                    msg = "Unauthorized - JWT: TokenAntrian Expired";
                }
            }
            else if (context.HttpContext.Response.Headers.ContainsKey("X-User-Failed"))
            {
                StringValues failedVal;
                var success = context.HttpContext.Response.Headers.TryGetValue("X-User-Failed", out failedVal);
                if (success) {
                    msg = "Unauthorized - JWT: " + failedVal.ToString();
                }
            }
            else if (context.HttpContext.Response.Headers.ContainsKey("X-Auth-Failed"))
            {
                StringValues failedVal;
                var success = context.HttpContext.Response.Headers.TryGetValue("X-Auth-Failed", out failedVal);
                if (success) {
                    msg = "Unauthorized - JWT: " + failedVal.ToString();
                }
            }
            else if (context.HttpContext.Response.Headers.ContainsKey("X-TokenAntrian-Error"))
            {
                StringValues failedVal;
                var success = context.HttpContext.Response.Headers.TryGetValue("X-TokenAntrian-Error", out failedVal);
                if (success) {
                    msg = "Unauthorized - JWT: TokenAntrian error";
                }
            }

            /*
             BPJS menggunakan metadata code untuk menentukan suskses tidaknya sebuah request
             HTTP status code tetap kita kirim dengan nilai 200 - OK
             HTTP status code 500, diterjemahkan BPJS menjadi : Gagal terhubung ke server

                "metadata": {
                    "message": "Unauthorized - TokenAntrian Expired",
                    "code": 201
                }

            Untuk negative result, aman mengunakan http status code 200, 400 atau 401
            */

            context.Result = new ApiResult( (int) HttpStatusCode.Unauthorized, msg, 201 );
        }
    }
}