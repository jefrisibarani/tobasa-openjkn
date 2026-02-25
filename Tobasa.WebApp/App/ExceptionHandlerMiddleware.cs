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
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tobasa.App
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            /*
             BPJS menggunakan metadata code untuk menentukan suskses tidaknya sebuah request
             HTTP status code tetap kita kirim dengan nilai 200 - OK
             HTTP status code 500, diterjemahkan BPJS menjadi : Gagal terhubung ke server

                "metadata": {
                    "message": "Unauthorized - TokenAntrian Expired",
                    "code": 201
                }

            Untuk negative result, aman mengunakan http status code 200 atau 400
            */

            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                var resultMessage = "";
                switch (error)
                {
                    case AppException e:
                        response.StatusCode = (int) HttpStatusCode.OK;
                        resultMessage = error?.Message;
                        _logger.LogError(error.Message);
                        break;

                    case SqlException e:
                        response.StatusCode = (int) HttpStatusCode.OK; // (int)HttpStatusCode.InternalServerError;
                        resultMessage = "Database error code: 5554";
                       
                        
                        if (context.Request.Path== "/api/users/authenticate") {
                            _logger.LogError("5554- SqlException: " + error.Message);
                        }
                        else
                        {
                            _logger.LogError("5554- SqlException: " + error.Message);
                            _logger.LogDebug("5554- " + e.StackTrace);
                        }
                        
                        break;

                    default:
                        response.StatusCode = (int) HttpStatusCode.OK; // (int)HttpStatusCode.InternalServerError;
                        resultMessage = "Internal error code: 5555";
                        
                        if (error.Source == "Microsoft.EntityFrameworkCore")
                        {
                            if (error.GetType() == typeof(InvalidOperationException))
                                _logger.LogError("5555- Database query result error: " + error.Message);
                        }
                        else {
                            _logger.LogError("5555- " + error.Message);
                        }

                        _logger.LogDebug("5555- " + error.StackTrace);

                        break;
                }
                // Gunakan metadata code 201
                var content = new ApiResponse(201, resultMessage);
                var result = JsonSerializer.Serialize( content, new JsonSerializerOptions { PropertyNamingPolicy = new LowerCaseNamingPolicy() });
                await response.WriteAsync(result);
            }
        }
    }
}