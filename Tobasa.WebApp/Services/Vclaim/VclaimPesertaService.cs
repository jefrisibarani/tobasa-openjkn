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

namespace Tobasa.Services.Vclaim
{
    public class VclaimPesertaService : VclaimService
    {
        public VclaimPesertaService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
            : base(appSettings, logger, configuration)
        {
            _typeBpjsService = "VCLAIM";
        }


        public async Task<Peserta> GetPesertaByNik(string noNik, string tglSep="")
        {
            noNik = noNik.Trim();

            if (string.IsNullOrEmpty(tglSep))
                tglSep = DateTime.Now.ToString("yyyy-MM-dd");

            var urlPath = "Peserta/nik/" + noNik + "/tglSEP/" + tglSep;

            if ( await GetPeserta(urlPath, noNik, "NIK_CARD") )
            {
                if (Result.HasResponse())
                {
                    var responseDecrypted = Result.Decrypt(this.EncryptKey);
                    var responsePeserta = JsonSerializer.Deserialize<ResponsePeserta>(responseDecrypted, _jsonOptions);

                    if (responsePeserta != null && responsePeserta.Peserta != null)
                        return responsePeserta.Peserta;
                }

                // we got error
                var message = "Data peserta No. BPJS: " + noNik + " error: null value";
                _logger.LogInformation("    VclaimPesertaService: " + message);
                throw new AppException(message);
            }

            return null;
        }

        public async Task<Peserta> GetPesertaByNocard(string noCard, string tglSep="")
        {
            noCard = noCard.Trim();

            if (string.IsNullOrEmpty(tglSep))
                tglSep = DateTime.Now.ToString("yyyy-MM-dd");

            var urlPath = "Peserta/nokartu/" + noCard + "/tglSEP/" + tglSep;

            if (await GetPeserta(urlPath, noCard, "BPJS_CARD"))
            {
                if (Result.HasResponse())
                {
                    var responseDecrypted = Result.Decrypt(this.EncryptKey);
                    var responsePeserta = JsonSerializer.Deserialize<ResponsePeserta>(responseDecrypted, _jsonOptions);

                    if (responsePeserta != null && responsePeserta.Peserta != null)
                        return responsePeserta.Peserta;
                }

                // we got error
                var message = "Data peserta No. BPJS: " + noCard + " error: null value";
                _logger.LogInformation("    VclaimPesertaService: " + message);
                throw new AppException(message);
            }

            return null;
        }

        /*
        ** function GetPeserta
        ** params : noPeserta     - string : Keyword pencarian
        **          byWhat        - string :  
        ** returns: True, bila WEB Service BPJS status code == 200
        */
        private async Task<bool> GetPeserta(string urlPath, string number, string byWhat)
        {
            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode() == 200)
                {
                    return true;
                }
                else
                {
                    var info = "No. NIK: ";
                    if (byWhat == "BPJS_CARD")
                        info = "No. BPJS: ";

                    _logger.LogInformation("    VclaimPesertaService: Data peserta " + info + number + " tidak ditemukan pada  WS VCLAIM, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                    var message = "Pencarian peserta " + info + number + " di  WS VCLAIM gagal dengan Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage();
                    return false;
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimPesertaService: GetPeserta " + ex.Message);
                throw new AppException("Error: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimPesertaService: GetPeserta 5514- " + ex.Message);
                throw new AppException("Internal error code: 5514");
            }
        }


        /*
        ** function GetPesertaForAnreanJkn
        ** params : noPeserta     - string : Keyword pencarian
        ** returns: True, bila WEB Service BPJS status code == 200
        */
        public async Task<PesertaJkn> GetPesertaForAntrianJkn(string noPeserta)
        {
            noPeserta = noPeserta.Trim();
            string tglSep = DateTime.Now.ToString("yyyy-MM-dd");
            var urlPath = "Peserta/nokartu/" + noPeserta + "/tglSEP/" + tglSep;

            try
            {
                await ExecuteGetRequest(urlPath);

                if (ResultMetaCode()==200)
                {
                    if ( Result.HasResponse() )
                    {
                        var responseDecrypted = Result.Decrypt(this.EncryptKey);
                        var responsePeserta = JsonSerializer.Deserialize<ResponsePeserta>(responseDecrypted, _jsonOptions);
                        
                        if (responsePeserta != null && responsePeserta.Peserta != null)
                        {
                            var peserta = responsePeserta.Peserta;
                            return new PesertaJkn
                            {
                                NomorBpjs       = peserta.NoKartu,
                                Nama            = peserta.Nama,
                                Alamat          = "",
                                NomorTelepon    = peserta.Mr.NoTelepon,
                                TglLahir        = DateTime.ParseExact(peserta.TglLahir, "yyyy-MM-dd", null),
                                StatusPeserta   = peserta.StatusPeserta
                            };
                        }
                    }
                    
                    // we got error
                    var message = "Data peserta : " + noPeserta + " error: null value";
                    throw new AppException(message);
                }
                else
                {
                    _logger.LogInformation("    VclaimPesertaService: Data peserta nomor: " + noPeserta + " tidak ditemukan pada webservice vClaim, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                    var message = "Pencarian peserta nomor " + noPeserta + " di webservice vclaim gagal dengan Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage();
                    throw new AppException(message);
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimPesertaService: GetPesertaForAntrianJkn " + ex.Message);
                throw new AppException("Error terjadi pada waktu verifikasi perserta BPJS: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimPesertaService: GetPesertaForAntrianJkn 5515- " + ex.Message);
                throw new AppException("Internal error code: 5515");
            }
        }
    }
}