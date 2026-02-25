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

using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Tobasa.App
{
    public class ApiResult : JsonResult
    {
        public ApiResult(int statusCode, string message = "Ok", int metadataCode = 200)
            : base( new ApiResponse(metadataCode, message),
                    new JsonSerializerOptions { PropertyNamingPolicy = new LowerCaseNamingPolicy() } )
        {
            StatusCode = statusCode;
        }

        public ApiResult(object value, int statusCode = 200, string message = "Ok", int metadataCode = 200)
            : base( new ApiResponseResult(value, metadataCode, message),
                    new JsonSerializerOptions { PropertyNamingPolicy = new LowerCaseNamingPolicy() } )
        {
            StatusCode = statusCode;
        }
    }

    public class ApiNegativeResult : ApiResult
    {
        public ApiNegativeResult(string message, int metadataCode = 201, int statusCode=200)
            : base(statusCode, message, metadataCode)
        {
        }
    }

    public class ApiResultCamelCase : ApiResult
    {
        public ApiResultCamelCase(int statusCode, string message = "Ok", int metadataCode = 200)
            : base(statusCode, message, metadataCode)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                //PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            SerializerSettings = jsonOptions;
        }

        public ApiResultCamelCase(object value, int statusCode = 200, string message = "Ok", int metadataCode = 200)
            : base(value, statusCode, message, metadataCode)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                //PropertyNameCaseInsensitive = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            SerializerSettings = jsonOptions;
        }
    }


    public class ApiNegativeResultCamelCase : ApiResultCamelCase
    {
        public ApiNegativeResultCamelCase(string message, int metadataCode = 201, int statusCode = 200)
            : base(statusCode, message, metadataCode)
        {
        }
    }


    public class ApiResponse
    {
        public MetaData MetaData { get; set; }

        public ApiResponse(int code, string message)
        {
            MetaData = new MetaData(code, message);
        }
    }

    public class ApiResponseResult : ApiResponse
    {
        public object Response { get; set; }

        public ApiResponseResult(object value, int code = 200, string message = "Ok")
            : base(code, message)
        {
            Response = value;
        }
    }

    public class MetaData
    {
        public string Message { get; set; } = "Ok";
        public int Code { get; set; } = 200;

        public MetaData(int code, string message)
        {
            Code = code;
            Message = message;
        }
    }
}
