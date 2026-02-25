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
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Tobasa.App
{
    // NOTE: https://exceptionnotfound.net/using-middleware-to-log-requests-and-responses-in-asp-net-core/

    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings     _appSettings;
        private readonly ILogger         _logger;

        public RequestResponseLoggingMiddleware(
            RequestDelegate                           next,
            IOptions<AppSettings>                     appSettings,
            ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next        = next;
            _appSettings = appSettings.Value;
            _logger      = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var method  = context.Request.Method;
            var srcAddr = context.Connection.RemoteIpAddress.MapToIPv4();
            var dispUrl = context.Request.GetDisplayUrl();
            var userAgn = context.Request.Headers["User-Agent"].ToString();
            var incomingInfo = $"[{srcAddr}] | {method} | {dispUrl} | {userAgn}";

            _logger.LogDebug(incomingInfo);

            await _next(context);
        }
    }
}