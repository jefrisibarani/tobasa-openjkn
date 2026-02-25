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
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tobasa.App;
using Tobasa.Models.Vclaim;

namespace Tobasa.Services.Vclaim
{
    public class VclaimMonitoringService : VclaimService
    {
        public VclaimMonitoringService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
            : base(appSettings, logger, configuration)
        {
            _typeBpjsService = "VCLAIM";
        }

        public async Task<List<HistoryPelayanan>> GetDataHistoriPelayananPeserta(string nomorBPJS, string tanggalAwal, string tanggalAkhir)
        {
            // monitoring/HistoriPelayanan/NoKartu/{Parameter 1}/tglMulai/{Parameter 2}/tglAkhir/{Parameter 3}
            string urlPath = $"monitoring/HistoriPelayanan/NoKartu/{nomorBPJS.Trim()}/tglMulai/{tanggalAwal.Trim()}/tglAkhir/{tanggalAkhir.Trim()}";

            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode() == 200)
                {
                    if (Result.HasResponse())
                    {
                        var responseDecrypted = Result.Decrypt(this.EncryptKey);
                        var resHistory = JsonSerializer.Deserialize<ResponseGetDataHistoriPelayananPeserta>(responseDecrypted, _jsonOptions);

                        if (resHistory != null && resHistory.Histori != null)
                            return resHistory.Histori;
                    }

                    // we got error
                    var message = $"Data history pelayanan: {nomorBPJS} error: null value";
                    throw new AppException(message);
                }
                else
                {
                    _logger.LogDebug($"    VclaimRencanaKontrolService: Data kontrol nomor {nomorBPJS} tidak ditemukan pada WS VCLAIM, Code:  {ResultMetaCode().ToString()} Message: {ResultMetaMessage()}") ;
                    // Return null for non 200 response from bpjs
                    return null;
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimMonitoringService: GetDataHistoriPelayananPeserta " + ex.Message);
                throw new AppException("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimMonitoringService: GetDataHistoriPelayananPeserta 5513- " + ex.Message);
                throw new AppException("Internal error code: 5513");
            }
        }
    }

}