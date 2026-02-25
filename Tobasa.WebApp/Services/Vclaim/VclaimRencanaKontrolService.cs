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
using Tobasa.Models.Vclaim;

namespace Tobasa.Services.Vclaim
{
    public class VclaimRencanaKontrolService : VclaimService
    {
        public VclaimRencanaKontrolService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
            : base(appSettings, logger, configuration)
        {
            _typeBpjsService = "VCLAIM";
        }

        public async Task<SuratKontrol> GetSuratKontrol(string noSuratKontrol)
        {
            if (string.IsNullOrEmpty(noSuratKontrol))
            {
                var message = "Nomor surat kontrol null atau empty";
                _logger.LogError("    VclaimRencanaKontrolService: " + message);
                throw new AppException(message);
            }

            noSuratKontrol = noSuratKontrol.Trim();
            var urlPath = "RencanaKontrol/noSuratKontrol/" + noSuratKontrol;

            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode() == 200)
                {
                    if (Result.HasResponse())
                    {
                        var responseDecrypted = Result.Decrypt(this.EncryptKey);
                        var resSuratKontrol = JsonSerializer.Deserialize<SuratKontrol>(responseDecrypted, _jsonOptions);
                        return resSuratKontrol;
                    }

                    // we got error
                    var message = $"Data surat rencana kontrol: {noSuratKontrol} error: null value";
                    //_logger.LogInformation("    VclaimRencanaKontrolService: " + message);
                    throw new AppException(message);
                }
                else
                {
                    _logger.LogDebug($"    VclaimRencanaKontrolService: Data kontrol nomor {noSuratKontrol} tidak ditemukan pada WS VCLAIM, Code:  {ResultMetaCode().ToString()} Message: {ResultMetaMessage()}") ;
                    // Return null for non 200 response from bpjs
                    return null;
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimRencanaKontrolService: GetSuratKontrol " + ex.Message);
                throw new AppException("Error : " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimRencanaKontrolService: GetSuratKontrol 5515- " + ex.Message);
                throw new AppException("Internal error code: 5515");
            }
        }
    }

}