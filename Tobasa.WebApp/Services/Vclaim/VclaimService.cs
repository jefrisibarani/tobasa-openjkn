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
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using Tobasa.App;
using Tobasa.Models.Vclaim;

namespace Tobasa.Services.Vclaim
{
    public class VclaimService
    {
        protected readonly AppSettings _appSettings;
        protected readonly ILogger _logger;

        protected string _consID;
        protected string _consSecret;
        protected string _baseUrl;
        protected string _service;
        protected string _typeFaskes;
        protected string _userKey;
        protected string _decryptKey;
        protected string _kodeRs;

        protected bool   _initOK           = false;
        protected string _responseBody;
        protected bool   _isRequestSuccess = false;
        protected string _typeBpjsService  = "VCLAIM";

        protected JsonSerializerOptions   _jsonOptions;
        protected VclaimResultBase        _result;
        protected readonly IConfiguration _configuration;

        public string EncryptKey
        {
            get
            {
                return _decryptKey;
            }
        }

        public JsonSerializerOptions JsonSrlzrOption
        {
            get
            {
                return _jsonOptions;
            }
        }

        public VclaimResultBase Result
        { 
            get 
            {
                if (_typeBpjsService == "VCLAIM")
                {
                    if (_result is VclaimResult result)
                        return result;
                }
                else // MJKN
                {
                    VclaimResultJKN result = _result as VclaimResultJKN;
                    if (result != null)
                        return result;
                }

                return null;
            }
        }

        public VclaimService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
        {
            _appSettings   = appSettings;
            _logger        = logger;
            _configuration = configuration;

            SetupBpjsProperties();

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
        }


        protected virtual void SetupBpjsProperties()
        {
            if (_typeBpjsService == "VCLAIM")
            {
                if (_appSettings.BpjsVclaimUseProduction)
                {
                    _consID      = _appSettings.BpjsVclaimProduction.ConsID;
                    _consSecret  = _appSettings.BpjsVclaimProduction.ConsSecret;
                    _baseUrl     = _appSettings.BpjsVclaimProduction.Url;
                    _service     = _appSettings.BpjsVclaimProduction.Service;
                    _typeFaskes  = _appSettings.BpjsVclaimProduction.TypeFaskesRujukan;
                    _userKey     = _appSettings.BpjsVclaimProduction.UserKey;
                }
                else 
                {
                    _consID      = _appSettings.BpjsVclaimDevelopment.ConsID;
                    _consSecret  = _appSettings.BpjsVclaimDevelopment.ConsSecret;
                    _baseUrl     = _appSettings.BpjsVclaimDevelopment.Url;
                    _service     = _appSettings.BpjsVclaimDevelopment.Service;
                    _typeFaskes  = _appSettings.BpjsVclaimDevelopment.TypeFaskesRujukan;
                    _userKey     = _appSettings.BpjsVclaimDevelopment.UserKey;
                }

                _jsonOptions = new JsonSerializerOptions
                {
                    //PropertyNameCaseInsensitive = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
            }
            else // MJKN
            {
                if (_appSettings.BpjsMjknUseProduction)
                {
                    _consID      = _appSettings.BpjsMjknProduction.ConsID;
                    _consSecret  = _appSettings.BpjsMjknProduction.ConsSecret;
                    _baseUrl     = _appSettings.BpjsMjknProduction.Url;
                    _service     = _appSettings.BpjsMjknProduction.Service;
                    _typeFaskes  = "";
                    _userKey     = _appSettings.BpjsMjknProduction.UserKey;
                }
                else
                {
                    _consID      = _appSettings.BpjsMjknDevelopment.ConsID;
                    _consSecret  = _appSettings.BpjsMjknDevelopment.ConsSecret;
                    _baseUrl     = _appSettings.BpjsMjknDevelopment.Url;
                    _service     = _appSettings.BpjsMjknDevelopment.Service;
                    _typeFaskes  = "";
                    _userKey     = _appSettings.BpjsMjknDevelopment.UserKey;
                }

                _jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                };
            }
        }

        public int ResultMetaCode()
        {
            if (_typeBpjsService == "VCLAIM")
            {
                if (_result is VclaimResult result)
                    return Convert.ToInt32(result.MetaData.Code);
            }
            else // MJKN
            {
                VclaimResultJKN result = _result as VclaimResultJKN;
                if (result != null)
                    return result.MetaData.Code;
            }
            
            return -1;
        }

        public string ResultMetaMessage()
        {
            if (_typeBpjsService == "VCLAIM")
            {
                VclaimResult result = _result as VclaimResult;
                if (result != null)
                    return result.MetaData.Message;
            }
            else // MJKN
            {
                VclaimResultJKN result = _result as VclaimResultJKN;
                if (result != null)
                    return result.MetaData.Message;
            }

            return "";
        }

        /*
        ** function ExecuteGetRequest
        ** Menjalankan GET Request ke server BPJS
        ** params  : url - string : web service url
        ** returns : 
        */
        public async Task ExecuteGetRequest(string url)
        {
            await ExecuteRequest("GET", url, null);
        }

        /*
        ** function ExecutePostRequest
        ** Menjalankan POST Request ke server BPJS
        ** params  : url - string : web service url
        ** returns : 
        */
        public async Task ExecutePostRequest(string url, string jsonData = "")
        {
            await ExecuteRequest("POST", url, jsonData);
        }

        /*
        ** function ExecuteRequest
        ** Menjalankan semua jenis Request ke server BPJS
        ** params : reqType - string : jenis request(GET,POST,PUT,DELETE)
        **        : url - string : web service url
        **        : jsonData - string : data yang akan dikirim
        ** returns: 
        */
        public async Task ExecuteRequest(string reqType, string urlPath, string jsonData = "")
        {
            SetupBpjsProperties();

            string timestamp;
            string signature;

            var currentTime      = DateTime.UtcNow;
            var currentTimeUnix  = ((DateTimeOffset)currentTime).ToUnixTimeSeconds();
            timestamp            = currentTimeUnix.ToString();
            var data             = _consID + "&" + timestamp;
            var hasher           = new HMACSHA256(Encoding.UTF8.GetBytes(_consSecret));
            var hash             = hasher.ComputeHash(Encoding.UTF8.GetBytes(data));
            signature            = Convert.ToBase64String(hash);
            _decryptKey          = _consID + _consSecret + timestamp;

            try
            {
                string serviceUrlPath = _service + "/" + urlPath;

                _logger.LogInformation("  Connecting to "+ _typeBpjsService + " service : " + _baseUrl + serviceUrlPath);
                _logger.LogDebug("    X-Cons-id   : " + _consID);
                _logger.LogDebug("    X-Timestamp : " + timestamp);
                _logger.LogDebug("    X-Signature : " + signature);
                _logger.LogDebug("    Cons Secret : " + _consSecret);
                _logger.LogDebug("    User Key    : " + _userKey);

                HttpClient httpClient  = new HttpClient();
                httpClient.BaseAddress = new Uri(_baseUrl);
                httpClient.DefaultRequestHeaders.Add("X-Cons-id",   _consID);
                httpClient.DefaultRequestHeaders.Add("X-Timestamp", timestamp);
                httpClient.DefaultRequestHeaders.Add("X-Signature", signature);
                httpClient.DefaultRequestHeaders.Add("user_key",    _userKey);
                
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                
                HttpResponseMessage response = null;
                if (reqType=="GET")
                {
                    response = await httpClient.GetAsync(serviceUrlPath);
                }
                else if (reqType=="POST")
                {
                    HttpContent payload = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    _logger.LogDebug("    JSON : " + jsonData);
                    response = await httpClient.PostAsync(serviceUrlPath, payload);
                }

                //response.EnsureSuccessStatusCode();

                _responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                //using var jsonDoc = JsonDocument.Parse(_responseBody);
                //var resString = jsonDoc.RootElement.ToString();
                //_result = JsonSerializer.Deserialize<VclaimResult>(resString, _jsonOptions);
                //var jsonString = JsonSerializer.Serialize(_result, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    int metaCode = -1;

                    if (_typeBpjsService == "VCLAIM")
                    {
                        var res = JsonSerializer.Deserialize<VclaimResult>(_responseBody, _jsonOptions);
                        bool hasResponse = res.Response != null;
                        res.HasResponse(hasResponse);
                        _result = res;
                        metaCode = Convert.ToInt32(res.MetaData.Code);
                    }
                    else // MJKN
                    {
                        var res = JsonSerializer.Deserialize<VclaimResultJKN>(_responseBody, _jsonOptions);
                        bool hasResponse = res.Response != null;
                        res.HasResponse(hasResponse);
                        _result = res;
                        metaCode = res.MetaData.Code;
                    }

                    if (metaCode == 200) {
                        _logger.LogInformation("    Koneksi http sukses dengan result 200");
                    }
                    else
                    {
                        _logger.LogInformation("    Koneksi http sukses dengan result: " + metaCode.ToString());
                        _logger.LogTrace("    Response body: " + _responseBody);
                    }
                }
                else
                {
                    var scode = (int)(response.StatusCode);
                    _logger.LogError($"    VclaimService: ExecuteRequest http status code: {scode}, response: {_responseBody}");
                    throw new AppException($"Koneksi ke webservice BPJS {_typeBpjsService} code: {scode}, response: {_responseBody}");
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError("    VclaimService: ExecuteRequest : " + ex.Message);
                throw new AppException($"Koneksi ke webservice BPJS {_typeBpjsService} : {ex.Message}");
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimService: ExecuteRequest : " + ex.Message);
                throw new AppException("Error terjadi pada koneksi ke web service BPJS "+ _typeBpjsService);
            }
        }
    }
}