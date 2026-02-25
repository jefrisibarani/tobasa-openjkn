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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Tobasa.App;
using Tobasa.Models;
using System.Linq;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using System.IO;
using Tobasa.Entities;
using System.Globalization;

namespace Tobasa.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, 
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [Route("api/version")]
        public IActionResult Version()
        {
            Dictionary<string, string> info = new Dictionary<string, string>
            {
                { "Web Service Mobile BPJS Version", BuildInfo.Version },
                { "Build date", BuildInfo.BuildDate },
                { "Curent time:", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
                { "Time zone:", TimeZoneInfo.Local.DisplayName },
                { "Curent UTC time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) },
                { "Unix epoch time", ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString() }
                //{ "UtcNow Unix Epoch Time", ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds().ToString() }
            };

            return new ApiResult(info);
        }

        [AllowAnonymous]
        [Route("error")]
        public IActionResult Error(int? statusCode = null)
        {
            // Kita menggunakan  app.UseStatusCodePagesWithReExecute("/api/error", "?statusCode={0}");
            // untuk mengarahkan status codes 400-599 ke method ini
            // agar status-status dari JWT dan Cookie auth dihandle disini
            var response = HttpContext.Response;
            var code = response.StatusCode;
            var message = ReasonPhrases.GetReasonPhrase(code);

            _logger.LogError(message);

            return new ApiResult(code, message, code);
        }

        [Authorize]
        [HttpPost("api/read_log_file")]
        public IActionResult ReadLogFile([FromBody] LogFileRequest request)
        {
            var loggedInUser = (BaseUsers)HttpContext.Items["User"];
            if ( ! (loggedInUser.UserName == "admin"  || loggedInUser.UserName == "simrs_user") )
                return new ApiNegativeResult("Unauthorized", 401, 401);

            if (request == null)
                return new ApiNegativeResult("Bad request", 400, 400);

            try
            {
                string jsonString = System.IO.File.ReadAllText("appsettings.json");
                var config = JsonDocument.Parse(jsonString).RootElement;
                JsonElement writeTo = config.GetProperty("Serilog").GetProperty("WriteTo");
                foreach (JsonElement writeToItem in writeTo.EnumerateArray())
                {
                    if (writeToItem.GetProperty("Name").GetString() == "File")
                    {
                        JsonElement args = writeToItem.GetProperty("Args");
                        string pathValue = args.GetProperty("path").GetString();
                        string logFolder = Path.GetDirectoryName(pathValue);
                        DirectoryInfo directoryInfo = new DirectoryInfo(logFolder);

                        // Find all files matching the pattern
                        FileInfo[] files = directoryInfo.GetFiles("*.log");

                        // Get the newest file based on creation time
                        FileInfo newestFile = files.OrderByDescending(f => f.CreationTime).FirstOrDefault();
                        if (newestFile != null)
                        {
                            string filePath = newestFile.FullName;
                            IEnumerable<string> filteredLines = ReadFilteredLines(filePath, request.LinesToRead, request.TextToFilter);

                            if (request.ResultAsJson)
                            {
                                return Ok(filteredLines);
                            }
                            else
                            {
                                string textContent = string.Join(Environment.NewLine, filteredLines);
                                return Content(textContent, "text/plain", Encoding.UTF8);
                            }
                        }
                        else {
                            return new ApiNegativeResult("No 'logfile' found in log directory", 201, 200);
                        }
                    }
                }
            }
            catch (JsonException e)
            {
                _logger.LogError($"ReadLogFile: {e.Message}");
            }
            catch (IOException e)
            {
                _logger.LogError($"ReadLogFile: {e.Message}");
            }

            return new ApiNegativeResult("Could not read log file", 500, 500);
        }

        private IEnumerable<string> ReadFilteredLines(string filePath, string linesToRead, string filterText)
        {
            int numberOfLines = -1;
            if (linesToRead == "ALL")
                numberOfLines = 0;
            else
            {
                if (!int.TryParse(linesToRead, out numberOfLines))
                    numberOfLines = -1;
            }

            if (numberOfLines < 0) {
                throw new AppException("Valid values for linesToRead: ALL or a positive number.");
            }

            List<string> readLines = new List<string>();

            // Open the text file for reading with the specified FileShare option
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // Create a StreamReader using the FileStream and specify the encoding
                using (StreamReader reader = new StreamReader(fileStream, System.Text.Encoding.UTF8))
                {
                    // Read each line from the file
                    string line;
                    int lineCount = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineCount++;

                        if (!string.IsNullOrWhiteSpace(filterText))
                        {
                            // Check if the line contains the filter text
                            if (line.Contains(filterText))
                            {
                                // Add the line to the list of filtered lines
                                readLines.Add(line);
                            }
                        }
                        else
                        {
                            readLines.Add(line);
                        }

                        if (numberOfLines > 0)
                        {
                            if (readLines.Count > numberOfLines)
                                readLines.RemoveAt(0);
                        }
                    }
                }
            }

            // Return the filtered lines
            return readLines;
        }
    }
}