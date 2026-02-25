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
    public class VclaimMjknService : VclaimService
    {
        public VclaimMjknService(AppSettings appSettings, ILogger logger, IConfiguration configuration)
            : base(appSettings, logger, configuration)
        {
            _typeBpjsService = "MJKN";
        }

        /*
         ** function GetReferensiDokter
         ** params : kodePoliJkn     - string : Kode Poli BPJS (bukan kode sub spesialis)
         ** params : tanggal         - string : tanggal praktek -  YYYY-MM-DD
         ** returns: True, bila WEB Service BPJS status code == 200
         * */
        public async Task<List<MjknJadwalDokterHfis>> GetReferensiJadwalDokter(string kodePoliJKN, string tanggal)
        {
            string urlPath;
            kodePoliJKN = kodePoliJKN.Trim();
            tanggal     = tanggal.Trim();

            urlPath = "jadwaldokter/kodepoli/" + kodePoliJKN + "/tanggal/" + tanggal;

            try
            {
                bool querySuccessfull = false;
                var responseDecryptedSample = "";

                bool demoMode = (_configuration["DemoMode"] == true.ToString());

                if (demoMode)
                {
                    responseDecryptedSample =
                        @"
                    [
                      {
                        ""kodesubspesialis"": ""UMU"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 10,
                        ""libur"": 0,
                        ""namahari"": ""SELASA"",
                        ""jadwal"": ""08:00-10:00"",
                        ""namasubspesialis"": ""UMUM"",
                        ""namadokter"": ""dr. Brown Hat"",
                        ""kodepoli"": ""UMU"",
                        ""namapoli"": ""UMUM"",
                        ""kodedokter"": 3
                      },
                      {
                        ""kodesubspesialis"": ""UMU"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 10,
                        ""libur"": 0,
                        ""namahari"": ""SELASA"",
                        ""jadwal"": ""10:00-12:00"",
                        ""namasubspesialis"": ""UMUM"",
                        ""namadokter"": ""dr. Black Boots"",
                        ""kodepoli"": ""UMU"",
                        ""namapoli"": ""UMUM"",
                        ""kodedokter"": 4
                      },
                      {
                        ""kodesubspesialis"": ""UMU"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 10,
                        ""libur"": 0,
                        ""namahari"": ""SELASA"",
                        ""jadwal"": ""13:00-17:00"",
                        ""namasubspesialis"": ""UMUM"",
                        ""namadokter"": ""dr. Red Jacket"",
                        ""kodepoli"": ""UMU"",
                        ""namapoli"": ""UMUM"",
                        ""kodedokter"": 5
                      },
                      {
                        ""kodesubspesialis"": ""068"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 5,
                        ""libur"": 0,
                        ""namahari"": ""SELASA"",
                        ""jadwal"": ""14:00-16:00"",
                        ""namasubspesialis"": ""NEUROTOLOGI"",
                        ""namadokter"": ""dr. Clark Blue"",
                        ""kodepoli"": ""THT"",
                        ""namapoli"": ""THT"",
                        ""kodedokter"": 1
                      },
                      {
                        ""kodesubspesialis"": ""068"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 5,
                        ""libur"": 0,
                        ""namahari"": ""RABU"",
                        ""jadwal"": ""14:00-16:00"",
                        ""namasubspesialis"": ""NEUROTOLOGI"",
                        ""namadokter"": ""dr. Clark Blue"",
                        ""kodepoli"": ""THT"",
                        ""namapoli"": ""THT"",
                        ""kodedokter"": 1
                      },
                      {
                        ""kodesubspesialis"": ""068"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 5,
                        ""libur"": 0,
                        ""namahari"": ""SENIN"",
                        ""jadwal"": ""14:00-16:00"",
                        ""namasubspesialis"": ""NEUROTOLOGI"",
                        ""namadokter"": ""dr. Clark Blue"",
                        ""kodepoli"": ""THT"",
                        ""namapoli"": ""THT"",
                        ""kodedokter"": 1
                      },
                      {
                        ""kodesubspesialis"": ""068"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 5,
                        ""libur"": 0,
                        ""namahari"": ""KAMIS"",
                        ""jadwal"": ""14:00-16:00"",
                        ""namasubspesialis"": ""NEUROTOLOGI"",
                        ""namadokter"": ""dr. Clark Blue"",
                        ""kodepoli"": ""THT"",
                        ""namapoli"": ""THT"",
                        ""kodedokter"": 1
                      },
                      {
                        ""kodesubspesialis"": ""068"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 5,
                        ""libur"": 0,
                        ""namahari"": ""KAMIS"",
                        ""jadwal"": ""14:00-16:00"",
                        ""namasubspesialis"": ""NEUROTOLOGI"",
                        ""namadokter"": ""dr. Clark Blue"",
                        ""kodepoli"": ""THT"",
                        ""namapoli"": ""THT"",
                        ""kodedokter"": 1
                      },
                      {
                        ""kodesubspesialis"": ""007"",
                        ""hari"": 1,
                        ""kapasitaspasien"": 4,
                        ""libur"": 0,
                        ""namahari"": ""SELASA"",
                        ""jadwal"": ""17:00-19:00"",
                        ""namasubspesialis"": ""GINJAL-HIPERTENSI"",
                        ""namadokter"": ""DR. STRANGE"",
                        ""kodepoli"": ""INT"",
                        ""namapoli"": ""PENYAKIT DALAM"",
                        ""kodedokter"": 2
                      }
                    ]
                    ";

                    querySuccessfull = true;
                }
                else
                {
                    await ExecuteGetRequest(urlPath);
                    querySuccessfull = ResultMetaCode() == 200;
                }

                if (querySuccessfull)
                {
                    var responseDecrypted = "";

                    if (demoMode)
                        responseDecrypted = responseDecryptedSample;
                    else
                        responseDecrypted = Result.Decrypt(this.EncryptKey);

                    var responseJadwal = JsonSerializer.Deserialize<List<MjknJadwalDokterHfis>>(responseDecrypted, _jsonOptions);
                    var total = responseJadwal.Count;
                    return responseJadwal;
                }
                else
                {
                    _logger.LogInformation($"    VclaimMjknService: GetReferensiDokter poli: {kodePoliJKN}, tanggal {tanggal}, Result Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                    var message = "Cek jadwal dokter, poli: " + kodePoliJKN + " pada WS BPJS MJKN, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage();
                    throw new AppException(message);
                }
            }
            catch (AppException ex)
            {
                _logger.LogError("    VclaimMjknService: GetReferensiDokter " + ex.Message);
                throw new AppException("Error pada waktu pengambilan jadwal dokter WS BPJS MJKN: " + ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimMjknService: GetReferensiDokter " + ex.Message);
                throw new AppException("Error pada waktu pengambilan jadwal dokter WS BPJS MJKN");
            }
        }

        public async Task<bool> TambahAntrian(RequestMjknTambahAntrian reservationData)
        {
            string urlPath = "antrean/add";

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                    WriteIndented = false
                };

                string strPayload = JsonSerializer.Serialize<RequestMjknTambahAntrian>(reservationData, jsonOptions);

                await ExecutePostRequest(urlPath, strPayload);

                if (ResultMetaCode() == 200)
                {
                    return true;
                }
                else
                {
                    if (ResultMetaCode() == 201) {
                        throw new BpjsException(ResultMetaMessage());
                    }

                    _logger.LogInformation("    VclaimMjknService: Posting data antrian baru ke WS BPJS MJKN, kodebooking: " + reservationData.KodeBooking + " Gagal, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                }
            }
            catch (BpjsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimMjknService: TambahAntrian, Posting gagal : " + ex.Message);
            }

            return false;
        }

        public async Task<bool> BatalAntrian(RequestMjknBatalAntrian batalData)
        {
            string urlPath = "antrean/batal";

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                    WriteIndented = false
                };

                string strPayload = JsonSerializer.Serialize<RequestMjknBatalAntrian>(batalData, jsonOptions);

                await ExecutePostRequest(urlPath, strPayload);

                if (ResultMetaCode() == 200)
                    return true;
                else
                {
                    if (ResultMetaCode() == 201) {
                        throw new BpjsException(ResultMetaMessage());
                    }

                    _logger.LogInformation("    VclaimMjknService: BatalAntrian Posting data pembatalan antrian ke WS BPJS MJKN, Kodebooking: " + batalData.KodeBooking + " Gagal, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                }
            }
            catch (BpjsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimMjknService: BatalAntrian Posting gagal : " + ex.Message);
            }

            return false;
        }

        public async Task<bool> UpdateWaktuAntrian(RequestMjknUpdateWaktu updateData)
        {
            string urlPath = "antrean/updatewaktu";

            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = new LowerCaseNamingPolicy(),
                    WriteIndented = false
                };

                string strPayload = JsonSerializer.Serialize<RequestMjknUpdateWaktu>(updateData, jsonOptions);

                await ExecutePostRequest(urlPath, strPayload);

                if (ResultMetaCode() == 200)
                    return true;
                else
                {
                    if (ResultMetaCode() == 201) {
                        throw new BpjsException(ResultMetaMessage());
                    }

                    _logger.LogInformation("    VclaimMjknService: Posting data update waktu antrian ke WS BPJS MJKN, Kodebooking: " + updateData.KodeBooking + " Gagal, Code: " + ResultMetaCode().ToString() + " Message: " + ResultMetaMessage());
                }
            }
            catch (BpjsException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("    VclaimMjknService: UpdateWaktuAntrian Posting gagal : " + ex.Message);
            }

            return false;
        }
    }
}