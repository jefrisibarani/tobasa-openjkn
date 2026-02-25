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
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Tobasa.App;
using Tobasa.Models.Jkn;
using Tobasa.Models.Vclaim;
using System.Collections.Generic;

namespace Tobasa.Services.Vclaim
{
    public class VclaimRujukanService : VclaimService
    {
        public VclaimRujukanService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
            : base(appSettings, logger, configuration)
        {
            _typeBpjsService = "VCLAIM";
        }


        public async Task<Rujukan> GetRujukanByNoRujukan(string nomorRujukan)
        {
            if (string.IsNullOrEmpty(nomorRujukan))
            {
                var message = "Nomor referensi (nomor rujukan) null atau empty";
                _logger.LogError("    GetRujukanByNoRujukan: " + nomorRujukan);
                throw new AppException(message);
            }

            if (await GetRujukan(nomorRujukan, "SINGLE", "NOMOR_RUJUK"))
            {
                if (Result.HasResponse())
                {
                    var responseDecrypted = Result.Decrypt(this.EncryptKey);
                    var responseRujukan = JsonSerializer.Deserialize<ResponseGetRujukan>(responseDecrypted, _jsonOptions);

                    if (responseRujukan != null && responseRujukan.Rujukan != null)
                        return responseRujukan.Rujukan;
                }

                // we got error
                var message = "Data rujukan: " + nomorRujukan + " error: null value";
                _logger.LogInformation("    VclaimRujukanService: " + message);
                throw new AppException(message);
            }
            
            return null;
        }

        public async Task<Rujukan> GetRujukanByNocardSingle(string nomorBPJS)
        {
            if (string.IsNullOrEmpty(nomorBPJS))
            {
                var message = "Nomor referensi (nomor peserta bpjs) null atau empty";
                _logger.LogError("    GetRujukanByNocardSingle: " + nomorBPJS);
                throw new AppException(message);
            }

            if (await GetRujukan(nomorBPJS, "SINGLE", "NOMOR_BPJS"))
            {
                if (Result.HasResponse())
                {
                    var responseDecrypted = Result.Decrypt(this.EncryptKey);
                    var responseRujukan = JsonSerializer.Deserialize<ResponseGetRujukan>(responseDecrypted, _jsonOptions);
                    
                    if (responseRujukan != null && responseRujukan.Rujukan != null) 
                        return responseRujukan.Rujukan;
                }
                
                // we got error
                var message = "Data rujukan: " + nomorBPJS + " error: null value";
                _logger.LogInformation("    VclaimRujukanService: " + message);
                throw new AppException(message);
            }

            return null;
        }

        public async Task<List<Rujukan>> GetRujukanByNocardMulti(string nomorBPJS)
        {
            if (string.IsNullOrEmpty(nomorBPJS))
            {
                var message = "Nomor referensi (nomor peserta bpjs) null atau empty";
                _logger.LogError("    GetRujukanByNocardMulti: " + nomorBPJS);
                throw new AppException(message);
            }


            if (await GetRujukan(nomorBPJS, "MULTI", "NOMOR_BPJS"))
            {
                if (Result.HasResponse())
                {
                    var responseDecrypted = Result.Decrypt(this.EncryptKey);
                    var responseRujukan = JsonSerializer.Deserialize<ResponseListRujukan>(responseDecrypted, _jsonOptions);

                    if (responseRujukan != null && responseRujukan.Rujukan != null)
                        return responseRujukan.Rujukan;
                }

                // we got error
                var message = "Data rujukan: " + nomorBPJS + " error: null value";
                _logger.LogInformation("    VclaimRujukanService: " + message);
                throw new AppException(message);
            }

            return null;
        }


        /*
         * function CariDataRujukan
         * params : nomor      - string : no.Rujukan atau no.kartu BPJS
         *          tipeRecord - string : multi => Cari Multi Record atau 'single' => Cari single(1) record
         *          jenis      - string : norujuk => no.Rujukan atau nokartu => no.kartu BPJS
         * returns: True, bila WEB Service BPJS status code == 200
         * note   :  bila tglLayanan empty, default adalah now
         */
        public async Task<bool> GetRujukan(string nomor, string tipeRecord, string jenis = "NOMOR_RUJUK")
        {
            string urlPath;
            nomor = nomor.Trim();

            if (jenis == "NOMOR_RUJUK")                         // Cari by no.rujukan
            {
                if (_typeFaskes == "RS")
                    urlPath = "rujukan/rs/" + nomor;            // RS
                else
                    urlPath = "rujukan/" + nomor;               // PCare

            }
            else
            {
                if (tipeRecord == "SINGLE")                     // Cari single(1) record
                {
                    if (_typeFaskes == "RS")                    // RS
                        urlPath = "rujukan/rs/peserta/" + nomor;
                    else                                        // PCare
                        urlPath = "rujukan/peserta/" + nomor;
                }
                else
                {
                    if (_typeFaskes == "RS")                    // RS
                        urlPath = "rujukan/rs/list/peserta/" + nomor;
                    else                                        // PCare
                        urlPath = "rujukan/list/peserta/" + nomor;
                }
            }

            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode() == 200) {
                    return true;
                }
                else
                {
                    var msg = "rujukan";
                    if (jenis == "NOMOR_BPJS")
                        msg = "kartu bpjs";

                    _logger.LogInformation("    VclaimRujukanService: Data Rujukan nomor " +  msg + " " + nomor + " tidak ditemukan pada webservice vClaim, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                    var message = "Pencarian rujukan nomor " + msg + " " + nomor + " peserta di webservice vclaim gagal dengan Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage();
                    return false;
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimRujukanService: GetRujukan: " + ex.Message);
                throw new AppException("Error terjadi pada waktu request rujukan BPJS: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimRujukanService: GetRujukan: 5560- " + ex.Message);
                throw new AppException("Internal error code: 5560");
            }
        }


        /*
         * function GetRujukanForAntrianJkn
         * params : noPeserta     - string : Keyword pencarian
         * returns: True, bila WEB Service BPJS status code == 200
         */
        public async Task<RujukanJkn> GetRujukanForAntrianJkn(string noRujukan)
        {
            if (string.IsNullOrEmpty(noRujukan))
            {
                var message = "Nomor rujukan null atau empty";
                _logger.LogError("    VclaimRujukanService: " + noRujukan);
                throw new AppException(message);
            }

            string urlPath;
            noRujukan = noRujukan.Trim();
                
            if (_typeFaskes == "RS")
                urlPath = "rujukan/rs/" + noRujukan;   // RS
            else 
                urlPath = "rujukan/" + noRujukan;      // PCare

            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode() == 200)
                {
                    if (Result.HasResponse())
                    {
                        var responseDecrypted = Result.Decrypt(this.EncryptKey);
                        var responseRujukan = JsonSerializer.Deserialize<ResponseGetRujukan>(responseDecrypted, _jsonOptions);

                        if (responseRujukan != null && responseRujukan.Rujukan != null)
                        {
                            var rujukan = responseRujukan.Rujukan;
                            return new RujukanJkn
                            {
                                NoKunjungan  = rujukan.NoKunjungan,
                                TglKunjungan = DateTime.ParseExact(rujukan.TglKunjungan, "yyyy-MM-dd", null)
                            };
                        }
                    }
                    
                    // we got error
                    var message = "Data rujukan: " + noRujukan + " error: null value";
                    //_logger.LogInformation("    VclaimRujukanService: " + message);
                    throw new AppException(message);
                }
                else
                {
                    //_logger.LogInformation("    VclaimRujukanService: Data rujukan nomor " + noRujukan + " tidak ditemukan pada  WS VCLAIM, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                    var message = "Pencarian rujukan nomor " + noRujukan + " peserta di  WS VCLAIM gagal dengan Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage();
                    throw new AppException(message);
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimRujukanService: GetRujukan: " + ex.Message);
                throw new AppException("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimRujukanService: GetRujukan: 5561- " + ex.Message);
                throw new AppException("Internal error code: 5561");
            }
        }
    }
}
