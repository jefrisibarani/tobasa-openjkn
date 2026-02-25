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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tobasa.App;
using Tobasa.Data;
using System.Data;
using System.Linq;
using Tobasa.Services.Vclaim;
using System.Globalization;
using Tobasa.Entities;
using Tobasa.Models.Jkn;
using Tobasa.Models.Mjkn;
using Tobasa.Models.Vclaim;
using Tobasa.Models.SimrsAntrian;
using System.Data.Common;
using Tobasa.Models.Queue;


namespace Tobasa.Services
{
    public interface IMjknService
    {
        Task<BlPasien> GetPasien(string nomorRM);
        Task<string> CreateNomorRekamMedis();
        Task<string> GetNomorRekamMedis(string nomorJKN);
        Task<string> GetNomorRekamMedis(string nomorNIK, string nomorJKN);
        Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN, string nomorRM);
        Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN);
        Task<bool> CheckPasienExistingByMRN(string nomorRM);
        Task<bool> CheckPasienExistingByJKN(string nomorJKN);

        Task<string> GetKodePoliJKN(string kdSubPoliJKN);
        Task<string> GetKodePoliINT(string kdSubPoliJKN);
        Task<bool> CekKodePoliINT(string kodePoliINT);
        Task<bool> CekKodeDokterINT(string kodeDokter);
        Task<string> GetKodeSubPoliJKN(string kodePoliINT);
        Task<int> GetKodeDokterJKN(string kodeDokterINT);
        Task<string> GetNamaDokterJKN(int kdDokterJKN);
        Task<string> GetKodeDokterINT(int kdDokterJKN);
        Task<string> GetNamaPosPoliINT(string kdSubPoliJKN);
        Task<string> GetNamaPoliINT(string kodePoliINT);
        Task<string> GetNamaSubPoliJKN(string kdSubPoliJKN);

        // --------------------------------------------------------------------
        //
        // --------------------------------------------------------------------
        Task<JadwalDokterView> GetJadwalDokter(string kodePoliINT, string tglPraktek, string kodeDokterINT, string jamMulai, string jamSelesai);
        Task<bool> CheckJadwalPoliTutup(string tanggalPeriksa, int kodeDokterJKN, string kdSubPoliJKN, string jamPraktek);
        Task<bool> CleanJadwalLocalSehari(string kodePoliINT, string tglPraktek, string kodeDokterINT, string jamMulai, string jamSelesai);
        Task<CekSlotResult> CekSlotReservasi(long kodeJadwal, string appSource);
        Task<AntrianSummary> GetAntrianSummary(long jadwalId);
        Task<bool> CheckKodeBooking(string kodeBooking);
        Task<bool> SaveAmbilAntrian(RequestAmbilAntrianFKRTL request, ResultAmbilAntrianFKRTL result);

        // --------------------------------------------------------------------
        // Services terkait untuk diakses BPJS
        // --------------------------------------------------------------------
        Task<ResultAmbilAntrianFKRTL> GetAppointment(DataAmbilAntrianFKRTL request, long jadwalId);
        Task<ResponseSisaAntrianFKRTL> GetSisaAntrian(RequestSisaAntrianFKRTL request);
        Task<ResponseStatusAntrianFKRTL> GetStatusAntrian(RequestStatusAntrianFKRTL request);
        Task<ResponseStatusAntrianFKRTL> GetStatusAntrianSimrs(string kodepoliRs, string kodedokterRs, string tanggalperiksa, string jampraktek);
        Task<ResponsePasienBaruFKRTL> CreatePasienBaru(RequestPasienBaruFKRTL request);
        Task<List<ResponseJadwalOperasiFKRTL>> JadwalOperasiRS(RequestJadwalOperasiFKRTL request);
        Task<List<ResponseJadwalOperasiPasien>> JadwalOperasiPasien(RequestJadwalOperasiPasien request);
        Task<string> BatalAntrian(RequestBatalAntrianFKRTL request);
        Task<string> CheckIn(RequestCheckInFKRTL request);
        Task<CreateQueueResultData> AmbilAntrianFarmasi(CreateQueueRequestData request);
        Task<ResponseStatusAntrianFarmasi> StatusAntrianFarmasi(StatusAntrianRequest request);

        // --------------------------------------------------------------------
        // Services terkait WS disisi BPJS
        // --------------------------------------------------------------------

        // Ambil jadwal dokter dari HFIS, berdasarkan * KODE POLI BPJS * ( BUKAN Kode SubPoli)
        Task<List<MjknJadwalDokterHfis>> GetJadwalDokterHFIS(string kodePoliJKN, string tanggal);
        Task<bool> CreateJadwalPraktekDariHFIS(MjknJadwalDokter jadwalHFIS,string tanggalPeriksa);
        Task<bool> CreateJadwalPraktekDariHFIS(string tglPraktek, string kodePoliJKN, int kodeDokterJKN, string jamMulai, string jamSelesai);
        Task<PesertaJkn> GetPesertaJknDariVClaim(string noPeserta);
        Task TambahAntrian(string kodebooking);
        Task BatalAntrian(string kodebooking, string keterangan);
        Task UpdateWaktuCheckinAntrian(string KodeBooking, ulong waktu, int taskId);
        Task<bool>   RujukanSudahTerpakai(string nomorReferensi);
        Task<bool>   RujukanSudahExpired(string nomorReferensi, string tanggalPeriksa);


        // --------------------------------------------------------------------
        //
        // --------------------------------------------------------------------

        // Digunakan secara internal SIMRS, untuk mensinkron jadwal praktek di HFIS
        // menjadi jadwal praktek di database SIMRS
        Task<object> SinkronJadwalPraktekDokterDariDataHFIS(string tglPraktek, string kodePoliINT);

        // Digunakan secara internal SIMRS untuk menggenerate jadwal praktek
        Task<int> CreateJadwalPraktek(RequestCreateJadwalPraktek request);
    }

    public class MjknService : IMjknService
    {
        private DataContextAntrian      _sqlCtx;
        private readonly AppSettings    _appSettings;
        private readonly ILogger        _logger;
        private readonly IConfiguration _configuration;

        public MjknService(DataContextAntrian context,
            IOptions<AppSettings> appSettings,
            ILogger<MjknService>  logger,
            IConfiguration        configuration)
        {
            _sqlCtx        = context;
            _appSettings   = appSettings.Value;
            _logger        = logger;
            _configuration = configuration;
        }

        public async Task<BlPasien> GetPasien(string nomorRM)
        {
            try
            {
                return await _sqlCtx.BlPasien.SingleAsync(x => x.NomorRekamMedis == nomorRM);
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: GetPasien 5100, Data internal PASIEN No.RM: {nomorRM} tidak valid: {ex.Message}");
                throw new AppException("Internal error code: 5100");
            }
        }

        public async Task<string> CreateNomorRekamMedis()
        {
            // if (_configuration["Corporate:Name"] == "RS_EXAMPLE")
            var result = await _sqlCtx.ExecuteScalarAsync<object>("SELECT COUNT(*) FROM bl_id_gen WHERE kode_faskes='PRIMARY' ");
            if ((result == null) || (result != null && result.ToString() != "1"))
            {
                // Tidak terdapat kode_faskes PRIMARY
                _logger.LogError($"  MjknService: CreateNomorRekamMedis 5101, Data MRN_GEN tidak valid");
                throw new AppException("Internal error: 5596");
            }

            var currentUrut = await _sqlCtx.ExecuteScalarAsync<long>("SELECT current_number FROM bl_id_gen WHERE kode_faskes='PRIMARY'");
            string jumlahKarakter = _configuration["AppSettings:JumlahKarakterNomorRekamMedis"];
            string formatStr = "D" + jumlahKarakter;               // produce ex: D6
            var newRekmed = (currentUrut + 1).ToString(formatStr); // produce ex: 000023

            _logger.LogDebug($"  MjknService: CreateNomorRekamMedis, Checking nomor rekam medis: {newRekmed}");
            var found = await _sqlCtx.BlPasien.AnyAsync(x => x.NomorRekamMedis == newRekmed);
            if (found)
            {
                return "NEW_REKMED_NOT_READY";
            }

            object[] qparams = new object[2];
            qparams[0] = _sqlCtx.Parameter("@currNum", DbType.Int64, currentUrut + 1, 14);
            qparams[1] = _sqlCtx.Parameter("@lastNum", DbType.Int64, currentUrut + 1, 14);
            var result1 = await _sqlCtx.ExecuteNonQueryAsync("UPDATE bl_id_gen set current_number=@currNum, last_number=@lastNum WHERE kode_faskes='PRIMARY' ", qparams);
            if (result1 > 0)
            {
                _logger.LogDebug($"  MjknService: CreateNomorRekamMedis, Nomor Rekam Medis : {newRekmed} OK");
                return newRekmed;
            }
            else
            {
                _logger.LogError($"  MjknService: CreateNomorRekamMedis 5101A, Error membuat nomor rekam medis baru {newRekmed}");
                throw new AppException("Internal error: 5101A");
            }
        }

        public async Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN, string nomorRM)
        {
            _logger.LogDebug($"  MjknService: CheckPasienExisting NIK: {nomorNIK}, No.BPJS: {nomorJKN}, No.RM {nomorRM}");

            var totalFound = await _sqlCtx.BlPasien.CountAsync(x => x.NomorIdentitas == nomorNIK && x.NomorKartuJkn == nomorJKN && x.NomorRekamMedis == nomorRM);
            if (totalFound == 0)
            {
                return false;
            }
            else if (totalFound == 1)
            {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CheckPasienExisting 5583, Ditemukan lebih dari satu data dengan NIK: {nomorNIK}, No.BPJS: {nomorJKN}, No.RM: {nomorRM}");
                throw new AppException("Internal error code: 5583");
            }
        }

        public async Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN)
        {
            _logger.LogDebug($"  MjknService: CheckPasienExisting NIK: {nomorNIK}, No.BPJS: {nomorJKN}");
            var totalFound = await _sqlCtx.BlPasien.CountAsync(x => x.NomorIdentitas == nomorNIK && x.NomorKartuJkn == nomorJKN);
            if (totalFound == 0)
            {
                return false;
            }
            else if (totalFound == 1)
            {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CheckPasienExisting 5102, Ditemukan lebih dari satu data dengan NIK: {nomorNIK}, No.BPJS: {nomorJKN}");
                throw new AppException("Internal error code: 5102");
            }
        }

        public async Task<bool> CheckPasienExistingByMRN(string nomorRM)
        {
            _logger.LogDebug($"  MjknService: CheckPasienExistingByMRN No.RM: {nomorRM}");
            var found = await _sqlCtx.BlPasien.AnyAsync(x => x.NomorRekamMedis == nomorRM);
            return found;
        }

        public async Task<bool> CheckPasienExistingByJKN(string nomorJKN)
        {
            _logger.LogDebug($"  MjknService: CheckPasienExistingByJKN: {nomorJKN}");
            var found = await _sqlCtx.BlPasien.AnyAsync(x => x.NomorKartuJkn == nomorJKN);
            return found;
        }

        public async Task<string> GetNomorRekamMedis(string nomorNIK, string nomorJKN)
        {
            _logger.LogDebug($"  MjknService: GetNomorRekamMedis, NIK: {nomorNIK}, No.BPJS: {nomorJKN}");

            var totalFound = await _sqlCtx.BlPasien.CountAsync(x => x.NomorIdentitas == nomorNIK && x.NomorKartuJkn == nomorJKN);
            if (totalFound == 0)
            {
                return "ERROR_NOMOR_REKAMMEDIS_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var pasien = await _sqlCtx.BlPasien.SingleAsync(x => x.NomorIdentitas == nomorNIK && x.NomorKartuJkn == nomorJKN);
                if (string.IsNullOrWhiteSpace(pasien.NomorRekamMedis))
                {
                    _logger.LogError($"  MjknService: GetNomorRekamMedis 5103, Data MRN null/empty: NIK: {nomorNIK}, No.BPJS: {nomorJKN}");
                    throw new AppException("Internal error: 5103");
                }
                else
                    return pasien.NomorRekamMedis.Trim();
            }
            else
            {
                _logger.LogError($"  MjknService: GetNomorRekamMedis 5103A, Ditemukan lebih dari satu data dengan NIK: {nomorNIK}, No.BPJS: {nomorJKN}");
                throw new AppException("Internal error: 5103A");
            }
        }

        public async Task<string> GetNomorRekamMedis(string nomorJKN)
        {
            var totalFound = await _sqlCtx.BlPasien.CountAsync(x => x.NomorKartuJkn == nomorJKN);
            if (totalFound == 0)
            {
                return "ERROR_NOMOR_REKAMMEDIS_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var pasien = await _sqlCtx.BlPasien.SingleAsync(x => x.NomorKartuJkn == nomorJKN);
                if (string.IsNullOrWhiteSpace(pasien.NomorRekamMedis))
                {
                    _logger.LogError($"  MjknService: GetNomorRekamMedis 5104, Data MRN null/whitespace: No.BPJS: {nomorJKN}");
                    throw new AppException("Internal error: 5104");
                }
                else
                    return pasien.NomorRekamMedis.Trim();
            }
            else
            {
                _logger.LogError($"  MjknService: GetNomorRekamMedis 5104A, Ditemukan lebih dari satu data dengan No.BPJS: {nomorJKN}");
                throw new AppException("Internal error code: 5104A");
            }
        }

        public async Task<bool> CekKodePoliINT(string kodePoliINT)
        {
            var found = await _sqlCtx.BlPoli.CountAsync(x => x.KodePoli == kodePoliINT);
            if (found == 1)
            {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CekKodePoliINT, Poli Kode: {kodePoliINT} ditemukan: {found}");
                return false;
            }
        }

        public async Task<bool> CekKodeDokterINT(string kodeDokterINT)
        {
            var found = await _sqlCtx.BlDokter.CountAsync(x => x.KodeDokter == kodeDokterINT);
            if (found == 1)
            {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CekKodeDokterINT, Dokter Kode: {kodeDokterINT} ditemukan: {found}");
                return false;
            }
        }

        public async Task<string> GetKodePoliJKN(string kdSubPoliJKN)
        {
            var found = await _sqlCtx.MjknPoliSub.AnyAsync(x => x.Kode == kdSubPoliJKN);
            if (!found)
            {
                return "ERROR_POLI_NOT_FOUND";
            }

            var qryPoli = from poJkn in _sqlCtx.MjknPoliSub
                          where poJkn.Kode == kdSubPoliJKN
                          select new { poJkn.KodePoli };

            var totalFound = await qryPoli.CountAsync();
            if (totalFound > 1)
            {
                _logger.LogError($"  MjknService: GetKodePoliJKN, Ditemukan lebih dari satu SubPoli dengan Kode: {kdSubPoliJKN}");
                return "ERROR_POLI_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var poli = await qryPoli.SingleAsync();
                if (string.IsNullOrWhiteSpace(poli.KodePoli))
                {
                    _logger.LogError($"  MjknService: GetKodePoliJKN, Data SubPoli dengan Kode: {kdSubPoliJKN} null/whitespace");
                    return "ERROR_POLI_NOT_FOUND";
                }
                else
                    return poli.KodePoli.Trim();
            }

            return "ERROR_POLI_NOT_FOUND";
        }

        public async Task<string> GetKodePoliINT(string kdSubPoliJKN)
        {
            var found = await _sqlCtx.MjknPoliSub.AnyAsync(x => x.Kode == kdSubPoliJKN);
            if (!found)
            {
                return "ERROR_POLI_NOT_FOUND";
            }

            var qryPoli = from po in _sqlCtx.BlPoli
                          join poJkn in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals poJkn.Kode
                          where poJkn.Kode == kdSubPoliJKN
                          select new { po.KodePoli };

            var totalFound = await qryPoli.CountAsync();
            if (totalFound > 1)
            {
                _logger.LogError($"  MjknService: GetKodePoliINT: 5105, Ditemukan lebih dari satu poli dengan Kode: {kdSubPoliJKN}");
                throw new AppException("Internal error code: 5578");
            }
            else if (totalFound == 1)
            {
                var poli = await qryPoli.SingleAsync();
                if (string.IsNullOrWhiteSpace(poli.KodePoli))
                {
                    _logger.LogError($"  MjknService: GetKodePoliINT: 5105A, Poli dengan Kode: {kdSubPoliJKN} null/whitespace");
                    throw new AppException("Internal error code: 5105A");
                }
                else
                    return poli.KodePoli.Trim();
            }

            return "ERROR_POLI_NOT_FOUND";
        }

        public async Task<string> GetKodeSubPoliJKN(string kodePoliINT)
        {
            if (!await CekKodePoliINT(kodePoliINT))
                return "ERROR_POLI_NOT_FOUND";

            var qryPoli = from po in _sqlCtx.BlPoli
                          join poJkn in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals poJkn.Kode
                          where po.KodePoli == kodePoliINT
                          select new { poJkn.Kode };

            var totalFound = await qryPoli.CountAsync();
            if (totalFound > 1)
            {
                _logger.LogError($"  MjknService: GetKodeSubPoliJKN, Ditemukan lebih dari satu poli dengan Kode: {kodePoliINT}");
                return "ERROR_POLI_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var poli = await qryPoli.SingleAsync();
                if (string.IsNullOrWhiteSpace(poli.Kode))
                {
                    _logger.LogError($"  MjknService: GetKodeSubPoliJKN, Poli dengan Kode: {kodePoliINT} null");
                    return "ERROR_POLI_NOT_FOUND";
                }
                else
                    return poli.Kode.Trim();
            }

            return "ERROR_POLI_NOT_FOUND";
        }

        public async Task<int> GetKodeDokterJKN(string kodeDokterINT)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterInternal == kodeDokterINT);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterInternal == kodeDokterINT);
                return dokter.KodedokterJkn;
            }
            else if (found == 0)
            {
                _logger.LogError($"  MjknService: GetKodeDokterJKN, Dokter Kode: {kodeDokterINT} tidak ditemukan dalam database");
                return -1; // Dokter tidak ditemukan
            }
            else
            {
                _logger.LogError($"  MjknService: GetKodeDokterJKN, Kode dokter {kodeDokterINT} dimapping lebih dari sekali");
                return -2; // Dokter ditemukan lebih dari sekali
            }
        }

        public async Task<string> GetNamaDokterJKN(int kdDokterJKN)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterJkn == kdDokterJKN);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterJkn == kdDokterJKN);
                return dokter.NamaDokter.Trim();
            }
            else if (found == 0)
            {
                _logger.LogError("  MjknService: GetNamaDokterJKN, Dokter Kode: " + kdDokterJKN.ToString() + " tidak ditemukan");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
            else
            {
                _logger.LogError($"  MjknService: GetNamaDokterJKN, Dokter kode: {kdDokterJKN} dimapping lebih dari sekali");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
        }

        public async Task<string> GetKodeDokterINT(int kdDokterJKN)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterJkn == kdDokterJKN);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterJkn == kdDokterJKN);
                if (string.IsNullOrWhiteSpace(dokter.KodedokterInternal))
                {
                    _logger.LogError("  MjknService: GetKodeDokterINT, Dokter Kode: " + kdDokterJKN.ToString() + ", Nama: " + dokter.NamaDokter.Trim() + " belum dimapping ke dokter internal");
                    return "ERROR_DOKTER_NOT_FOUND_SIMRS";
                }
                else
                    return dokter.KodedokterInternal.Trim();
            }
            else if (found == 0)
            {
                _logger.LogError("  MjknService: GetKodeDokterINT, Dokter Kode: " + kdDokterJKN.ToString() + " tidak ditemukan");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
            else
            {
                _logger.LogError($"  MjknService: GetKodeDokterINT, Kode dokter {kdDokterJKN} dimapping lebih dari sekali");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
        }

        public async Task<string> GetNamaPoliINT(string kodePoliINT)
        {
            var found = await _sqlCtx.BlPoli.CountAsync(x => x.KodePoli == kodePoliINT);
            if (found == 1)
            {
                var poli = await _sqlCtx.BlPoli.SingleAsync(x => x.KodePoli == kodePoliINT);
                return poli.NamaPoli.Trim();
            }
            else
            {
                _logger.LogError($"  MjknService: GetNamaPoliINT Kode: {kodePoliINT} ditemukan: {found}");
                return "ERROR_POLI_NOT_FOUND";
            }
        }

        public async Task<string> GetNamaPosPoliINT(string kdSubPoliJKN)
        {
            var exists = await _sqlCtx.MjknPoliSub.AnyAsync(x => x.Kode == kdSubPoliJKN);
            if (!exists)
            {
                _logger.LogError($"  MjknService: GetNamaPosPoliINT Kode SubSpesialis JKN: {kdSubPoliJKN} tidak ditemukan");
                return "ERROR_POLI_NOT_FOUND";
            }

            // cek apakah kode sub spesialis JKN ini ada di mapping ke poli FKTP
            var qryPoli = from po in _sqlCtx.BlPoli
                          join poJkn in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals poJkn.Kode
                          where poJkn.Kode == kdSubPoliJKN
                          select new { po.NamaPoli };

            var found = await qryPoli.CountAsync();
            if (found == 1)
            {
                var poli = await qryPoli.SingleAsync();

                if (string.IsNullOrWhiteSpace(poli.NamaPoli))
                    return "ERROR_POLI_NOT_FOUND";
                else
                    return poli.NamaPoli.Trim();
            }
            else if (found == 0)
            {
                _logger.LogError($"  MjknService: GetNamaPosPoliINT Kode SubSpesialis JKN: {kdSubPoliJKN} tidak ditemukan");
                return "ERROR_POLI_NOT_FOUND";
            }
            else
            {
                _logger.LogError($"  MjknService: GetNamaPosPoliINT Kode SubSpesialis JKN: {kdSubPoliJKN} dimapping lebih dari sekali");
                return "ERROR_POLI_NOT_FOUND";
            }
        }

        public async Task<string> GetNamaSubPoliJKN(string kdSubPoliJKN)
        {
            var found = await _sqlCtx.MjknPoliSub.CountAsync(x => x.Kode == kdSubPoliJKN);
            if (found == 1)
            {
                var poliJkn = await _sqlCtx.MjknPoliSub.SingleAsync(x => x.Kode == kdSubPoliJKN);
                return poliJkn.Nama.Trim();
            }
            else
            {
                return "ERROR_POLI_NOT_FOUND";
            }
        }

        // --------------------------------------------------------------------


        // Periksa apakah terjadi perubahan jadwal(jam awal,jam akhir) bila berubah, update jam praktek dengan jadwal baru
        public async Task<bool> CleanJadwalLocalSehari(string kodePoliRs, string tglPraktek, string kodeDokterRs, string jamMulai, string jamSelesai)
        {
            _logger.LogDebug($"  MjknService: CleanJadwalLocalSehari Poli: {kodePoliRs}, Tanggal: {tglPraktek}, Dokter: {kodeDokterRs}, Jam Mulai: {jamMulai}, Jam Selesai: {jamSelesai}");
            // NOTE: jamMulai dan jamSelesai must in format: HH:mm (5 character)
            var tanggalPraktek = DateTime.ParseExact(tglPraktek, "yyyy-MM-dd", null);

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            // Hapus data yang jam mulainya sama namun jam selesai berbeda atau mark dengan "_"
            try
            { 
                // ambil jadwal praktek sehari
                var qryJadwal = from j in _sqlCtx.BlJadwal
                                join p in _sqlCtx.BlPoli   on j.KodePoli   equals p.KodePoli
                                join d in _sqlCtx.BlDokter on j.KodeDokter equals d.KodeDokter
                                where j.KodePoli      == kodePoliRs
                                    && j.KodeDokter   == kodeDokterRs
                                    && j.Tanggal      == DateOnly.FromDateTime(tanggalPraktek.Date) 
                                orderby j.Id ascending
                                select j;

                var oldDatas = await qryJadwal.ToListAsync();
                if (oldDatas.Count > 0) 
                {
                    foreach (var old in oldDatas)
                    {
                        var operation     = "";
                        var oldJamMulai   = old.JamMulai.Trim().Substring(0, 5);
                        var oldJamSelesai = old.JamSelesai.Trim().Substring(0, 5);
                        bool different    = false;

                        if (oldJamMulai == jamMulai && oldJamSelesai == jamSelesai)
                        {
                            operation = "Data sama persis";

                            // Hanya ada satu jadwal, dan sama persis. tidak perlu lakukan apapun
                            if (oldDatas.Count == 1) {
                                return true;
                            }
                            else {
                            }
                        }
                        else if (oldJamMulai == jamMulai && oldJamSelesai != jamSelesai)
                        {
                            operation = $"Data jam mulai sama, jam selesai berbeda. (oldStart:{oldJamMulai}, start:{jamMulai}, oldFinish:{oldJamSelesai}, finish:{jamSelesai})";
                            different = true;
                        }
                        else if (oldJamMulai != jamMulai && oldJamSelesai == jamSelesai)
                        {
                            operation = $"Data jam mulai berbeda, jam selesai sama. (oldStart:{oldJamMulai}, start:{jamMulai}, oldFinish:{oldJamSelesai}, finish:{jamSelesai})";
                            different = true;
                        }
                        else
                        {
                            operation = $"Data lainnya. (oldStart:{oldJamMulai}, start:{jamMulai}, oldFinish:{oldJamSelesai}, finish:{jamSelesai})";
                            different = true;
                        }

                        if (different)
                        {
                            try
                            {
                                // Cek apakah slot antrian layanan untuk tanggal praktek yg diinginkan telah tersedia/ diset pada bl_jadwal
                                var reservedSlot = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == old.Id);
                                if (reservedSlot == 0) 
                                {
                                    old.KodeDokter   = "_" + old.KodeDokter.Trim();
                                    old.Keterangan = "Update dari WSMJKN-HFIS";
                                    operation     += " dan Slot Antrian belum ada";
                                }
                                else
                                {
                                    // TODO_JEFRI:
                                    // Bila Slot telah tercipta di bl_jadwal, apakah slot akan diupdate juga jam mulai dan jam selesainya?
                                    // sementara ini kita hanya mengubah data di bl_jadwal

                                    operation += " dan Slot Antrian sudah ada";
                                    bool updateJadwal = true;
                                    // Jika opsi untuk mengupdate jadwal diaktifkan
                                    if (updateJadwal)
                                    {
                                        old.JamMulai   = jamMulai;
                                        old.JamSelesai = jamSelesai;
                                        old.Keterangan = "Update dari WSMJKN-HFIS";
                                        operation     += " - update jam mulai dan jam selesai";
                                    }
                                    else
                                    {
                                        old.KodeDokter   = "_" + old.KodeDokter.Trim();
                                        old.Keterangan = "Update dari WSMJKN-HFIS";
                                        operation     += " - sembunyikan jadwal";
                                    }
                                }
                                _logger.LogDebug($"  MjknService: CleanJadwalLocalSehari {operation}");

                                _sqlCtx.BlJadwal.Update(old);
                                _sqlCtx.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                if (ex.InnerException != null)
                                    _logger.LogError($"  MjknService: CleanJadwalLocalSehari Error {operation} : {ex.Message}  {ex.InnerException.Message}");
                                else
                                    _logger.LogError($"  MjknService: CleanJadwalLocalSehari Error {operation} : {ex.Message}");
                            }
                        }
                    }
                }
                else {}
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: CleanJadwalLocalSehari Error: {ex.Message}");
            }

            return true;
        }

        public async Task<JadwalDokterView> GetJadwalDokter(string kodePoliINT, string tglPraktek, string kodeDokterINT, string jamMulai, string jamSelesai)
        {
            _logger.LogDebug($"  MjknService: GetReferensiJadwalDokter Poli: {kodePoliINT}, Tanggal: {tglPraktek}, Dokter: {kodeDokterINT}, Jam Mulai: {jamMulai}, Jam Selesai: {jamSelesai}");
            
            try
            { 
                // NOTE: jamMulai dan jamSelesai must in format: HH:mm (5 character)
                var tanggalPraktek = DateTime.ParseExact(tglPraktek, "yyyy-MM-dd", null);

                var qryJadwal = from j in _sqlCtx.BlJadwal
                                join p in _sqlCtx.BlPoli    on j.KodePoli  equals p.KodePoli
                                join d in _sqlCtx.BlDokter  on j.KodeDokter equals d.KodeDokter
                                where j.KodePoli                           == kodePoliINT
                                    && j.KodeDokter                        == kodeDokterINT
                                    && j.Tanggal                           == DateOnly.FromDateTime(tanggalPraktek.Date)
                                    && j.JamMulai.Trim().Substring(0,5)    == jamMulai
                                    && j.JamSelesai.Trim().Substring(0, 5) == jamSelesai
                                orderby j.Id descending
                                select new JadwalDokterView
                                {
                                    Id         = j.Id,
                                    NamaPoli   = p.NamaPoli,
                                    NamaDokter = d.NamaDokter
                                };
                var jadwal = await qryJadwal.SingleAsync();
                return jadwal;
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: GetReferensiJadwalDokter ,{ex.Message}");
                return null;
            }
        }

        public async Task<bool> CheckJadwalPoliTutup(string tanggalPeriksa, int kodeDokterJkn, string kdSubPoliJKN, string jamPraktek)
        {
            if (!Utils.IsValidDate(tanggalPeriksa))
                return false;

            try
            {
                string namaHari = Utils.ISODateTimeToIndonesianDay(tanggalPeriksa).ToUpper();

                var sch = await _sqlCtx.MjknJadwalDokter.SingleAsync(x =>
                                           x.KodedokterJkn              == kodeDokterJkn
                                        && x.KodeSubspesialis           == kdSubPoliJKN
                                        && x.NamaHari.Trim().ToUpper()  == namaHari
                                        && x.Jadwal                     == jamPraktek);
                return sch.Libur;
            }
            catch (InvalidOperationException /*ex*/)
            {
                return false;
            }
        }

        public async Task<bool> RujukanSudahExpired(string nomorReferensi, string tanggalPeriksa)
        {
            // Cek masa aktif nomor Rujukan
            var vClaimRujukanService = new VclaimRujukanService(_appSettings, _logger, _configuration);
            var rujukanJkn = await vClaimRujukanService.GetRujukanByNoRujukan(nomorReferensi);
            if (rujukanJkn != null)
            {
                var tglPeriksa = DateTime.ParseExact(tanggalPeriksa, "yyyy-MM-dd", null);
                var tglKunjungan = DateTime.ParseExact(rujukanJkn.TglKunjungan, "yyyy-MM-dd", null);

                return (tglPeriksa.Date >= tglKunjungan.AddDays(90).Date);
            }
            else
                throw new AppException("Data Rujukan tidak ditemukan");
        }

        public async Task<bool> RujukanSudahTerpakai(string nomorReferensi)
        {
            _logger.LogDebug("  MjknService: RujukanSudahTerpakai Periksa nomor referensi: " + nomorReferensi);
            var found = await _sqlCtx.BlAntrian.AnyAsync(x => x.NomorRujukan == nomorReferensi && x.StatusAntri == 1);
            return found;
        }

        public async Task<PesertaJkn> GetPesertaJknDariVClaim(string noPeserta)
        {
            var vClaimPesertaService = new VclaimPesertaService(_appSettings, _logger, _configuration);
            var pesertaJkn = await vClaimPesertaService.GetPesertaForAntrianJkn(noPeserta);
            return pesertaJkn;
        }

        public async Task<AntrianSummary> GetAntrianSummary(long jadwalId)
        {
            _logger.LogDebug($"  MjknService: GetAntrianSummary, JADWAL Id: {jadwalId}");

            try
            { 
                var jadwalDokter = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == jadwalId);
                var currentQueue = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId);
                if (currentQueue == 0)
                {
                    return new AntrianSummary();
                }

                var quotaNonJknUsed   = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId != Insurance.BPJS);
                var quotaJknUsed      = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId == Insurance.BPJS);
                // reservasi antrian bpjs yang belum dilayani
                var sisaAntrianJkn    = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId == Insurance.BPJS && x.StatusAntri == StatusAntri.BOOKED );
                var jknServedCount    = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId == Insurance.BPJS && x.StatusAntri == StatusAntri.SERVED );
                // reservasi Jkn yang dibatalkan
                var jknBatalCount     = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId == Insurance.BPJS && x.StatusAntri == StatusAntri.CANCELLED);
                // semua reservasi yang belum dilayani
                var sisaAntrianAll    = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.StatusAntri == StatusAntri.BOOKED );


                var lastNumber = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalId && x.StatusAntri >= StatusAntri.BOOKED)
                                            .Select(s => s.NomorAntrianInt)
                                            //.DefaultIfEmpty(0)
                                            .MaxAsync();

                // find last served queue
                string lastServedAntrianNo  = "-";
                int lastServedAntrianNumber = 0;
                var antriServedNumber       = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalId && x.StatusAntri == StatusAntri.SERVED ).CountAsync();

                if (antriServedNumber > 0)
                {
                    var antriLastServedNum = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalId && x.StatusAntri == StatusAntri.SERVED ).Select(s => s.NomorAntrianInt).MaxAsync();
                    var antriLastServed    = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalId && x.StatusAntri == StatusAntri.SERVED && x.NomorAntrianInt == antriLastServedNum).SingleAsync();
                    var lastServedPoliName = await _sqlCtx.BlPoli.Where(x => x.KodePoli == antriLastServed.KodePoli).Select(s => s.NamaPoli).SingleAsync();

                    lastServedAntrianNo = lastServedPoliName.Trim() + "-" + Convert.ToString(antriLastServedNum);
                }

                var summ = new AntrianSummary
                {
                    JadwalId          = jadwalId,
                    QuotaTotal        = jadwalDokter.QuotaTotal,
                    QuotaJkn          = jadwalDokter.QuotaJkn,
                    QuotaNonJkn       = jadwalDokter.QuotaNonJkn,
                    QuotaJknUsed      = quotaJknUsed,
                    QuotaNonJknUsed   = quotaNonJknUsed,
                    Totalantrian      = quotaJknUsed + quotaNonJknUsed,
                    SisaAntrianJkn    = sisaAntrianJkn,
                    SisaAntrianNonJkn = sisaAntrianAll - sisaAntrianJkn,
                    SisaAntrianAll    = sisaAntrianAll,
                    LastServedAntrian = lastServedAntrianNo,
                    LastServedAntrianNumber = lastServedAntrianNumber,
                    LastNumber        = lastNumber,
                };
                return summ;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"  MjknService: GetAntrianSummary, 5593 JADWAL Id: {jadwalId} Error: {ex.Message}");
                throw new AppException("Internal error code: 5593");
            }
        }

        public async Task<bool> SaveAntrianTransaction(DataAmbilAntrianFKRTL request, BlAntrian blAntrian)
        {
            // NOTE: request KodePoliJKN adalah Kode Subspesialis/Sub Poli BPJS/BPJS, bukan kode Poli !
            // ---------------------------------------------------------------------------------------
            try
            {
                var totalFound = await _sqlCtx.MjknAntrianTransaction.CountAsync(x =>x.SAntriId == blAntrian.Id);
                if (totalFound > 1)
                {
                    _logger.LogError($"  MjknService: SaveAntrianTransaction Ditemukan lebih dari satu data transaksi antrian dengan Id: {blAntrian.Id}");
                    return false;
                }
                else if (totalFound == 1)
                {
                    var dat = await _sqlCtx.MjknAntrianTransaction.SingleAsync(x => x.SAntriId == blAntrian.Id);
                    if (!(dat.OKodeBooking.Trim() == blAntrian.TokenAntrian.Trim() && dat.RNomorRm.Trim() == blAntrian.NomorRekamMedis.Trim()))
                    {
                        _logger.LogError($"  MjknService: SaveAntrianTransaction Id: {blAntrian.Id}, No.RM: {blAntrian.NomorRekamMedis} atau Token Antrian: {blAntrian.TokenAntrian} sudah tidak sesuai");
                        return false;
                    }
                }
                else if (totalFound == 0)
                {
                    _logger.LogDebug($"  MjknService: SaveAntrianTransaction Menginsert transaksi antrian, Token Antrian: {blAntrian.TokenAntrian.Trim()}");

                    var dat = new MjknAntrianTransaction();

                    // Request fields
                    dat.RNomorKartu       = request.NomorKartu;
                    dat.RNik              = request.Nik;
                    dat.RNomorHp          = request.NomorHP;
                    dat.RKodePoliJkn      = request.KodePoliJKN;
                    dat.RNomorRm          = request.NomorRekamMedis;
                    dat.RTanggalPeriksa   = DateOnly.FromDateTime(DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null));
                    dat.RKodeDokterJkn    = request.KodeDokterJKN;
                    dat.RJamPraktek       = request.JamPraktek;
                    dat.RJenisKunjungan   = request.JenisKunjungan;
                    dat.RNomorReferensi   = request.NomorReferensi;
                    dat.RSource           = request.AppSource;

                    // Internal fields
                    dat.SUsername         = string.IsNullOrWhiteSpace(blAntrian.CreatedBy) ? string.Empty : blAntrian.CreatedBy.Trim();
                    dat.SUseredit         = string.IsNullOrWhiteSpace(blAntrian.EditedBy) ? string.Empty : blAntrian.EditedBy.Trim();
                    dat.SPasienBaru       = blAntrian.PasienBaru;
                    dat.SStatus           = blAntrian.StatusAntri;
                    dat.SBookedAt         = blAntrian.BookedAt;
                    dat.SCheckinAt        = blAntrian.CheckinAt;
                    dat.SCancelledAt      = blAntrian.CancelledAt;
                    dat.SServedAt         = blAntrian.ServedAt;
                    dat.SAntriId          = blAntrian.Id;
                    dat.SJadwalId         = blAntrian.JadwalId;
                    dat.SKodepoliInternal = string.IsNullOrWhiteSpace(blAntrian.KodePoli) ? string.Empty : blAntrian.KodePoli.Trim();
                    dat.SKodokterInternal = request.KodeDokterINT;
                    dat.SNamaHari         = string.IsNullOrWhiteSpace(blAntrian.NamaHari) ? string.Empty : blAntrian.NamaHari.Trim();
                    dat.SNamaPasien       = string.IsNullOrWhiteSpace(blAntrian.NamaPasien) ? string.Empty : blAntrian.NamaPasien.Trim();
                    dat.SAlamatPasien     = string.IsNullOrWhiteSpace(blAntrian.Alamat) ? string.Empty : blAntrian.Alamat.Trim();
                    dat.SPhone            = string.IsNullOrWhiteSpace(blAntrian.Phone) ? string.Empty : blAntrian.Phone.Trim();
                    dat.STanggalLahir     = blAntrian.TanggalLahir;
                    dat.SKeterangan       = blAntrian.Keterangan;

                    // Output fields
                    dat.ONomorAntrian     = string.IsNullOrWhiteSpace(blAntrian.NomorAntrian) ? string.Empty : blAntrian.NomorAntrian.Trim();
                    dat.OAngkaAntrian     = blAntrian.NomorAntrianInt;
                    dat.OKodeBooking      = blAntrian.TokenAntrian.Trim();
                    dat.ONamaPoli         = string.IsNullOrWhiteSpace(request.NamaPoliJKN) ? string.Empty : request.NamaPoliJKN;
                    dat.ONamaDokter       = string.IsNullOrWhiteSpace(request.NamaDokterJKN) ? string.Empty : request.NamaDokterJKN;
                    dat.OEstimasiDilayani = blAntrian.EstimasiDilayani;
                    dat.OSisaKuotaJkn     = blAntrian.SisaKuotaJkn;
                    dat.OKuotaJkn         = blAntrian.KuotaJkn;
                    dat.OSisaKuotaNonJkn  = blAntrian.SisaKuotaNonJkn;
                    dat.OKuotaNonJkn      = blAntrian.KuotaNonJkn;
                    dat.OKeterangan       = string.IsNullOrWhiteSpace(blAntrian.Keterangan) ? string.Empty : blAntrian.Keterangan.Trim();

                    await _sqlCtx.MjknAntrianTransaction.AddAsync(dat);
                    _sqlCtx.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("  MjknService: SaveAntrianTransaction " + ex.Message);
                throw;
            }
        }
        
        public async Task<bool> CheckKodeBooking(string kodeBooking)
        {
            var found = await _sqlCtx.BlAntrian.AnyAsync(x => x.TokenAntrian == kodeBooking);
            return found;
        }

        public async Task<bool> SaveAmbilAntrian(RequestAmbilAntrianFKRTL request, ResultAmbilAntrianFKRTL result)
        {
            try
            {
                // Update data jadwal dokter MJKN
                var totalFound = await _sqlCtx.MjknAmbilAntrian.CountAsync(x =>
                                       x.NomorKartu == request.NomorKartu
                                    && x.Nik == request.Nik
                                    && x.NomorHp == request.NoHp
                                    && x.KodePoli == request.KodePoli
                                    && x.NomorRm == request.NoRm
                                    && x.TanggalPeriksa == DateOnly.FromDateTime(DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null))
                                    && x.KodeDokter == request.KodeDokter
                                    && x.JamPraktek == request.JamPraktek
                                    && x.JenisKunjungan == request.JenisKunjungan
                                    && x.NomorReferensi == request.NomorReferensi);

                if (totalFound==0)
                {
                    var data = new MjknAmbilAntrian
                    {
                        NomorKartu      = request.NomorKartu,
                        Nik             = request.Nik,
                        NomorHp         = request.NoHp,
                        KodePoli        = request.KodePoli,
                        NomorRm         = request.NoRm,
                        TanggalPeriksa  = DateOnly.FromDateTime(DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null)),
                        KodeDokter      = request.KodeDokter,
                        JamPraktek      = request.JamPraktek,
                        JenisKunjungan  = request.JenisKunjungan,
                        NomorReferensi  = request.NomorReferensi,
                        ReservationId   = -1
                    };

                    await _sqlCtx.MjknAmbilAntrian.AddAsync(data);
                    _sqlCtx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("  MjknService: SaveAmbilAntrian " + ex.Message);
            }

            return true;
        }

        public async Task<ResponseSisaAntrianFKRTL> GetSisaAntrian(RequestSisaAntrianFKRTL request)
        {
            var found = await _sqlCtx.BlAntrian.AnyAsync(x => x.TokenAntrian == request.KodeBooking);
            if (!found) {
                throw new AppException("Kode booking tidak ditemukan");
            }

            var reservasi = await _sqlCtx.BlAntrian.SingleAsync(x => x.TokenAntrian == request.KodeBooking);

            var qryPraktek = from pr in _sqlCtx.BlJadwal
                             join dr in _sqlCtx.BlDokter    on pr.KodeDokter  equals dr.KodeDokter
                             join po in _sqlCtx.BlPoli      on pr.KodePoli    equals po.KodePoli
                             join sp in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals sp.Kode
                             where pr.Id == reservasi.JadwalId
                             select new JadwalDokter
                             {
                                 Id          = pr.Id,
                                 KodeDokter  = pr.KodeDokter,
                                 KodePoli    = pr.KodePoli,
                                 JamMulai    = pr.JamMulai,
                                 JamSelesai  = pr.JamSelesai,
                                 Tanggal     = pr.Tanggal,
                                 Libur       = pr.Libur,
                                 QuotaNonJkn = pr.QuotaNonJkn,
                                 QuotaJkn    = pr.QuotaJkn,
                                 QuotaTotal  = pr.QuotaTotal,
                                 NamaPoli    = sp.Nama,    // use name from MjknPoliSub
                                 NamaDokter  = dr.NamaDokter
                             };

            JadwalDokter viewDrPraktek = null;
            try
            {
                viewDrPraktek = await qryPraktek.SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("  MjknService: GetSisaAntrian" + ex.Message);
                throw new AppException("Kode booking tidak ditemukan");
            }

            var antrianSummary = await GetAntrianSummary(reservasi.JadwalId);

            BlDokter dokter = null;
            try
            {
                dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == reservasi.KodeDokter);
            }
            catch (Exception ex)
            {
                _logger.LogError("  MjknService: GetSisaAntrian Error: " + ex.Message);
                throw new AppException("Dokter tidak ditemukan");
            }

            var pasienTime     = Convert.ToInt32(dokter.PasienTime);
            string kodePoliJkn = await GetKodeSubPoliJKN(reservasi.KodePoli.Trim());
            var nomorAntrian   = kodePoliJkn + "-" + Convert.ToString(reservasi.NomorAntrianInt);
            var waktuTunggu    = (pasienTime * (antrianSummary.SisaAntrianAll - 1)) * 60; // dalam detik

            var response = new ResponseSisaAntrianFKRTL
            {
                NomorAntrean   = nomorAntrian,
                NamaPoli       = viewDrPraktek.NamaPoli.Trim(),
                NamaDokter     = viewDrPraktek.NamaDokter.Trim(),
                SisaAntrean    = antrianSummary.SisaAntrianAll,
                AntreanPanggil = antrianSummary.LastServedAntrian,
                WaktuTunggu    = waktuTunggu,
                Keterangan     = ""
            };

            return response;
        }

        public async Task<ResponseStatusAntrianFKRTL> GetStatusAntrian(RequestStatusAntrianFKRTL request)
        {
            DateTime tglPeriksa = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null).Date;
            string jamMulai     = request.JamPraktek.Substring(0, 5);  // format yang kita mau 14:00
            string jamSelesai   = request.JamPraktek.Substring(6, 5);  // format yang kita mau 16:00

            var qryJadwal  = from pr in _sqlCtx.BlJadwal
                             join dr in _sqlCtx.BlDokter    on pr.KodeDokter  equals dr.KodeDokter
                             join po in _sqlCtx.BlPoli      on pr.KodePoli    equals po.KodePoli
                             join sp in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals sp.Kode
                             where pr.KodePoli                           == request.KodePoliINT
                                 && pr.KodeDokter                        == request.KodeDokterINT
                                 && pr.Tanggal                           == DateOnly.FromDateTime(tglPeriksa.Date) 
                                 && pr.JamMulai.Trim().Substring(0, 5)   == jamMulai
                                 && pr.JamSelesai.Trim().Substring(0, 5) == jamSelesai
                             select new JadwalDokter
                             {
                                 Id          = pr.Id,
                                 KodeDokter  = pr.KodeDokter,
                                 KodePoli    = pr.KodePoli,
                                 JamMulai    = pr.JamMulai,
                                 JamSelesai  = pr.JamSelesai,
                                 Tanggal     = pr.Tanggal,
                                 Libur       = pr.Libur,
                                 QuotaNonJkn = pr.QuotaNonJkn,
                                 QuotaJkn    = pr.QuotaJkn,
                                 QuotaTotal  = pr.QuotaTotal,
                                 NamaPoli    = sp.Nama,    // use name from MjknPoliSub
                                 NamaDokter  = dr.NamaDokter
                             };
            JadwalDokter jadwal = null;
            try
            {
                jadwal = await qryJadwal.SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("MjknService: GetStatusAntrian " + ex.Message);
                throw new AppException("Jadwal tidak ditemukan");
            }

            var summary  = await GetAntrianSummary(jadwal.Id);
            var dokter   = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == request.KodeDokterINT);

            if (summary.QuotaJkn == 0) {
                summary.QuotaJkn = dokter.KapasitasJkn;
            }

            if (summary.QuotaNonJkn == 0) {
                summary.QuotaNonJkn = dokter.KapasitasNonJkn;
            }

            var response = new ResponseStatusAntrianFKRTL
            {
                NamaPoli        = jadwal.NamaPoli.Trim(),
                NamaDokter      = jadwal.NamaDokter.Trim(),
                TotalAntrean    = summary.Totalantrian,
                SisaAntrean     = summary.SisaAntrianAll,
                AntreanPanggil  = summary.LastServedAntrian,
                SisaKuotaJkn    = summary.QuotaJkn - summary.QuotaJknUsed,
                KuotaJkn        = summary.QuotaJkn,
                SisaKuotaNonJkn = summary.QuotaNonJkn - summary.QuotaNonJknUsed,
                KuotaNonJkn     = summary.QuotaNonJkn,
                Keterangan      = ""
            };

            return response;
        }

        public async Task<ResponseStatusAntrianFKRTL> GetStatusAntrianSimrs(string kodepoliINT, string kodedokterINT, string tanggalperiksa, string jampraktek)
        {
            var kodePoliJKN   = await GetKodeSubPoliJKN(kodepoliINT);
            var kodeDokterJKN = await GetKodeDokterJKN(kodedokterINT);

            var req = new RequestStatusAntrianFKRTL
            {
                KodePoli        = kodePoliJKN,
                KodeDokter      = kodeDokterJKN,
                TanggalPeriksa  = tanggalperiksa,
                JamPraktek      = jampraktek,
                KodeDokterINT   = kodedokterINT,
                KodePoliINT     = kodepoliINT
            };

            return await GetStatusAntrian(req);
        }

        public async Task<string> BatalAntrian(RequestBatalAntrianFKRTL request)
        {
            var janjiFound = await _sqlCtx.BlAntrian.CountAsync(x => x.TokenAntrian == request.KodeBooking);
            if (janjiFound <= 0) {
                return "Antrian tidak ditemukan";
            }
            else if (janjiFound > 1)
            {
                _logger.LogError($"  MjknService: BatalAntrian Terdapat lebih dari satu kodebooking {request.KodeBooking} pada ANTRIAN");
                return "Antrian tidak ditemukan";
            }
            else {} // OK

            var janji = await _sqlCtx.BlAntrian.SingleAsync(x => x.TokenAntrian == request.KodeBooking);
            if (janji.StatusAntri == -1) {
                return "Antrian sudah pernah dibatalkan";
            }

            if (janji.StatusAntri == 1) {
                return "Pasien sudah dilayani, antrian tidak dapat dibatalkan";
            }

            if (janji.Tanggal == null ) {
                _logger.LogError("  MjknService: BatalAntrian Tanggal pada antrian tidak valid");
                return "Data internal antrian tidak valid";
            }

            if ( janji.CheckinAt != null &&  janji.CheckinAt.Value.Date == janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue)) {
                return "StatusAntri antrian sudah check in, tidak dapat dibatalkan";
            }

            if (DateTime.Now.Date > janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue)) {
                return "Pembatalan tidak dapat dilakukan setelah hari-H";
            }

            var jadwalFound  = await _sqlCtx.BlJadwal.CountAsync(x => x.Id == janji.JadwalId);
            if (jadwalFound != 1)
            {
                _logger.LogError("  MjknService: BatalAntrian Data internal JADWAL tidak valid");
                return "Data internal jadwal tidak valid";
            }

            var jadwalDokter = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == janji.JadwalId);
            if (string.IsNullOrWhiteSpace(jadwalDokter.JamMulai) || string.IsNullOrWhiteSpace(jadwalDokter.JamSelesai))
            {
                _logger.LogError("  MjknService: BatalAntrian Format jam mulai/selesai pada tabel tidak valid");
                return "Data internal jadwal tidak valid";
            }

            var xJamMulai   = jadwalDokter.JamMulai.Trim();
            var xJamSelesai = jadwalDokter.JamSelesai.Trim();

            if (xJamMulai.Length < 5 || xJamSelesai.Length < 5)
            {
                _logger.LogWarning("  MjknService: BatalAntrian Format/length jam mulai/selesai pada tabel tidak valid");
                return "Data internal jadwal tidak valid";
            }

            xJamMulai   = xJamMulai.Substring(0, 5);
            xJamSelesai = xJamSelesai.Substring(0, 5);

            // jamMulai and jamAkhir is current date
            DateTime jamMulai = new DateTime();
            DateTime jamAkhir = new DateTime();
            try
            {
                // Note: 24 hour format
                jamMulai = DateTime.ParseExact(xJamMulai,   "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                jamAkhir = DateTime.ParseExact(xJamSelesai, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message);
                _logger.LogError("  MjknService: BatalAntrian Gagal mengkonversi data jam mulai/akhir");
                return "Data jadwal tidak dapat dikonversi";
            }

            // set correct jamMulai dan jamAkhir date with janji.Tanggal.Date
            jamMulai = janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue).AddHours(jamMulai.Hour).AddMinutes(jamMulai.Minute);
            jamAkhir = janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue).AddHours(jamAkhir.Hour).AddMinutes(jamAkhir.Minute);

            if ( DateTime.Now > jamAkhir) {
                return "Pembatalan tidak dapat dilakukan setelah jadwal praktek berakhir";
            }

            janji.CancelledAt = DateTime.Now;
            janji.StatusAntri = StatusAntri.CANCELLED;
            janji.Keterangan  = request.Keterangan;

            _sqlCtx.BlAntrian.Update(janji);
            _sqlCtx.SaveChanges();

            await UpdateAntrianStatusHistory(janji, request.Keterangan);

            // simpan perubahan ke table transaksi antrian, bila diset
            if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
            {
                var totalFound = await _sqlCtx.MjknAntrianTransaction.CountAsync(x => x.SAntriId == janji.Id);
                if (totalFound > 1) {
                    _logger.LogError($"  MjknService: BatalAntrian Ditemukan lebih dari satu data transaksi antrian dengan Id: {janji.Id}");
                }
                else if (totalFound == 1)
                {
                    var transData           = await _sqlCtx.MjknAntrianTransaction.SingleAsync(x => x.SAntriId == janji.Id);
                    transData.SStatus       = janji.StatusAntri;
                    transData.SKeterangan   = janji.Keterangan;
                    transData.SCancelledAt  = janji.CancelledAt;

                    _sqlCtx.MjknAntrianTransaction.Update(transData);
                    _sqlCtx.SaveChanges();
                }
                else {}
            }

            return "SUKSES";
        }

        public async Task<string> CheckIn(RequestCheckInFKRTL request)
        {
            var janjiFound = await _sqlCtx.BlAntrian.CountAsync(x => x.TokenAntrian == request.KodeBooking);
            if (janjiFound <= 0) {
                return "Antrian tidak ditemukan";
            }
            else if (janjiFound > 1)
            {
                _logger.LogError($"  MjknService: CheckIn Terdapat lebih dari satu kodebooking {request.KodeBooking} pada ANTRIAN");
                return "Antrian tidak ditemukan - invalid internal data";
            }
            else { } // OK

            DateTime currentTime  = DateTime.Now;
            DateTime checkinTime  = Utils.UnixTimeStampToDateTime(request.Waktu);

            _logger.LogDebug("  MjknService: CheckIn Proses check-in, Kodebooking: " + request.KodeBooking + ", Checkin Time: " + checkinTime.ToString("yyyy-MM-dd HH:mm:ss.fffK") + ", Server Time: " + currentTime.ToString("yyyy-MM-dd HH:mm:ss.fffK"));

            if (_appSettings.CheckInMjknHarusSesuaiTanggalServer)
            {
                if (checkinTime.Date != currentTime.Date)
                    return "Tidak dapat melakukan check in diluar hari-H";
            }

            var janji = await _sqlCtx.BlAntrian.SingleAsync(x => x.TokenAntrian == request.KodeBooking);

            var jadwalFound = await _sqlCtx.BlJadwal.CountAsync(x => x.Id == janji.JadwalId);
            if (jadwalFound != 1) 
            {
                _logger.LogError("  MjknService: CheckIn Data internal JADWAL tidak valid");
                return "Data internal jadwal tidak valid";
            }

            var jadwalDokter  = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == janji.JadwalId);
            if (string.IsNullOrWhiteSpace(jadwalDokter.JamMulai) || string.IsNullOrWhiteSpace(jadwalDokter.JamSelesai))
            {
                _logger.LogError("  MjknService: CheckIn Format jam mulai/selesai pada tabel tidak valid");
                return "Data internal jadwal tidak valid";
            }

            var xJamMulai   = jadwalDokter.JamMulai.Trim();
            var xJamSelesai = jadwalDokter.JamSelesai.Trim();

            if (xJamMulai.Length < 5 || xJamSelesai.Length < 5)
            {
                _logger.LogError("  MjknService: CheckIn Format/length jam mulai/selesai pada tabel tidak valid");
                return "Data internal jadwal tidak valid";
            }

            xJamMulai   = xJamMulai.Substring(0, 5);
            xJamSelesai = xJamSelesai.Substring(0, 5);

            // jamMulai and jamAkhir is current date
            DateTime jamMulai = new DateTime();
            DateTime jamAkhir = new DateTime();
            try
            {
                jamMulai = DateTime.ParseExact(xJamMulai,   "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                jamAkhir = DateTime.ParseExact(xJamSelesai, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.Message);
                _logger.LogError("  MjknService: CheckIn Gagal mengkonversi data jam mulai/akhir");
                return "Data jadwal tidak dapat dikonversi";
            }
            // set correct jamMulai dan jamAkhir date with janji.Tanggal.Date
            // waktu checkin minimal 3 jam sebelum jam praktek, maksimal 1 jam sebelum jam praktek selesai
            var earlyLimit = -3;
            var lateLimit  = -1;

            if (janji.Tanggal == null)
            {
                _logger.LogError("  MjknService: CheckIn Tanggal pada antrian tidak valid");
                return "Data internal antrian tidak valid";
            }

            jamMulai = janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue).AddHours(jamMulai.Hour + earlyLimit).AddMinutes(jamMulai.Minute); 
            jamAkhir = janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue).AddHours(jamAkhir.Hour + lateLimit).AddMinutes(jamAkhir.Minute);

            if (checkinTime.Date != janji.Tanggal.Value.ToDateTime(TimeOnly.MinValue)) {
                return "Tanggal check in berbeda dengan jadwal praktek";
            }

            if (janji.StatusAntri == -1) {
                return "Antrian sudah dibatalkan sebelumnya";
            }

            if ( janji.CheckinAt != null &&  janji.CheckinAt >= jamMulai && janji.CheckinAt <= jamAkhir) {
                return "Anda sudah check in pada " + Utils.DateTimeToIndonesianDate(janji.CheckinAt.Value.Date);
            }

            if (_appSettings.CheckInMjknHarusSesuaiTanggalServer)
            {
                if (checkinTime.Date == currentTime.Date && checkinTime >= jamMulai && checkinTime <= jamAkhir)
                {
                    janji.CheckinAt   = checkinTime;
                    janji.StatusAntri = StatusAntri.CHECKED_IN;

                    _sqlCtx.BlAntrian.Update(janji);
                    _sqlCtx.SaveChanges();

                    // simpan perubahan ke table transaksi antrian
                    if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
                    {
                        var totalFound = await _sqlCtx.MjknAntrianTransaction.CountAsync(x => x.SAntriId == janji.Id);
                        if (totalFound > 1) {
                            _logger.LogError($"  MjknService: CheckIn Ditemukan lebih dari satu data transaksi antrian dengan Id: {janji.Id}");
                        }
                        else if (totalFound == 1)
                        {
                            var transData          = await _sqlCtx.MjknAntrianTransaction.SingleAsync(x => x.SAntriId == janji.Id);
                            transData.SCheckinAt = checkinTime;

                            _sqlCtx.MjknAntrianTransaction.Update(transData);
                            _sqlCtx.SaveChanges();
                        }
                        else {}
                    }

                    if (janji.PasienBaru)
                        return "SUKSES_PASIEN_BARU";
                    else
                        return "SUKSES";
                }
                else
                    return "Anda tidak dapat check in diluar jadwal praktek " + janji.JamPraktek;
            }
            else
            {
                if ( checkinTime >= jamMulai && checkinTime <= jamAkhir)
                {
                    janji.CheckinAt = checkinTime;
                    janji.StatusAntri = StatusAntri.CHECKED_IN;

                    _sqlCtx.BlAntrian.Update(janji);
                    _sqlCtx.SaveChanges();

                    await UpdateAntrianStatusHistory(janji,"");

                    // simpan perubahan ke table transaksi antrian
                    var totalFound = await _sqlCtx.MjknAntrianTransaction.CountAsync(x => x.SAntriId == janji.Id);
                    if (totalFound > 1) {
                        _logger.LogError($"  MjknService: CheckIn Ditemukan lebih dari satu data antrian dengan Id: {janji.Id}");
                    }
                    else if (totalFound == 1)
                    {
                        var transData = await _sqlCtx.MjknAntrianTransaction.SingleAsync(x => x.SAntriId == janji.Id);
                        transData.SCheckinAt = checkinTime;
                        _sqlCtx.MjknAntrianTransaction.Update(transData);
                        _sqlCtx.SaveChanges();
                    }
                    else {}

                    if (janji.PasienBaru)
                        return "SUKSES_PASIEN_BARU";
                    else
                        return "SUKSES";
                }
                else
                    return "Anda tidak dapat check in diluar jadwal praktek " + janji.JamPraktek;
            }
        }

        public async Task<ResponsePasienBaruFKRTL> CreatePasienBaru(RequestPasienBaruFKRTL request)
        {
            DateTime dtTglLahir = DateTime.ParseExact(request.TanggalLahir, "yyyy-MM-dd", null);
            if (dtTglLahir.Date > DateTime.Now.Date) {
                throw new AppException("Tanggal lahir tidak valid");
            }

            if (await CheckPasienExistingByJKN(request.NomorKartu))
            {
                _logger.LogError("  MjknService: CreatePasienBaru Nomor peserta sudah ada pada PASIEN");
                throw new AppException("Data nomor peserta sudah pernah dientrikan");
            }

            var found = await _sqlCtx.BlPasien.AnyAsync(x => x.NomorIdentitas == request.Nik);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru nomor NIK sudah ada pada PASIEN");
                throw new AppException("Data NIK peserta sudah pernah dientrikan");
            }

            // periksa table mjkn_pasien
            found = await _sqlCtx.MjknPasien.AnyAsync(x => x.Nik == request.Nik);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Nomor peserta sudah ada pada MjknPasien");
                throw new AppException("Data NIK peserta sudah pernah dientrikan");
            }

            found = await _sqlCtx.MjknPasien.AnyAsync(x => x.NomorKartu == request.NomorKartu);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru Nomor NIK sudah ada pada MjknPasien");
                throw new AppException("Data nomor peserta sudah pernah dientrikan");
            }

            // Try get new medical record number
            string nomorRm = "";
            bool newRekmedNotReady = true;
            while (newRekmedNotReady)
            {
                nomorRm = await CreateNomorRekamMedis();
                if (nomorRm != "NEW_REKMED_NOT_READY")
                    newRekmedNotReady = false;
            }

            if (string.IsNullOrWhiteSpace(nomorRm)) {
                throw new AppException($"Gagal mendapat nomor rekam medis baru");
            }

            MjknPasien pasienBaru = new MjknPasien
            {
                NomorKartu      = request.NomorKartu,
                Nik             = request.Nik,
                NomorKk         = request.NomorKK,
                NomorRm         = nomorRm,
                Nama            = request.Nama,
                JenisKelamin    = request.JenisKelamin,
                TanggalLahir    = DateOnly.FromDateTime(dtTglLahir),
                NomorHp         = request.NoHp,
                Alamat          = request.Alamat,
                KodePropinsi    = request.KodeProp,
                NamaPropinsi    = request.NamaProp,
                KodeDati2       = request.KodeDati2,
                NamaDati2       = request.NamaDati2,
                KodeKecamatan   = request.KodeKec,
                NamaKecamatan   = request.NamaKec,
                KodeKelurahan   = request.KodeKel,
                NamaKelurahan   = request.NamaKel,
                Rw              = request.Rw,
                Rt              = request.Rt,
                LastReservation = "",
                CreatedAt       = DateTime.Now
            };

            BlPasien pasienBaruInternal = new BlPasien
            {
                NomorRekamMedis    = nomorRm,
                NomorIdentitas     = request.Nik,
                NomorKartuJkn      = request.NomorKartu,
                NomorKartuKeluarga = request.NomorKK,
                Nama               = request.Nama,
                Honorific          = "",
                Gender             = request.JenisKelamin,
                TanggalLahir       = DateOnly.FromDateTime(dtTglLahir),
                Phone              = request.NoHp,
                Alamat             = request.Alamat,
                KodePropinsi       = request.KodeProp,
                NamaPropinsi       = request.NamaProp,
                KodeDati2          = request.KodeDati2,
                NamaDati2          = request.NamaDati2,
                KodeKecamatan      = request.KodeKec,
                NamaKecamatan      = request.NamaKec,
                KodeKelurahan      = request.KodeKel,
                NamaKelurahan      = request.NamaKel,
                Rt                 = request.Rw,
                Rw                 = request.Rt,
                InsuranceId        = Insurance.BPJS,
                CreatedAt          = DateTime.Now
            };

            try
            {
                await _sqlCtx.BlPasien.AddAsync(pasienBaruInternal);
                _sqlCtx.SaveChanges();

                await _sqlCtx.MjknPasien.AddAsync(pasienBaru);
                _sqlCtx.SaveChanges();
            }
            catch (Exception)
            {
                _logger.LogError("  MjknService: CreatePasienBaru Gagal waktu menyimpan ke database");
                throw new AppException("Internal error menyimpan pasien baru");
            }

            var response = new ResponsePasienBaruFKRTL
            {
                NoRm = nomorRm
            };

            return response;
        }

        public async Task<List<ResponseJadwalOperasiFKRTL>> JadwalOperasiRS(RequestJadwalOperasiFKRTL request)
        {
            DateTime dtawal =  DateTime.ParseExact(request.TanggalAwal,  "yyyy-MM-dd", null);
            DateTime dtakhir = DateTime.ParseExact(request.TanggalAkhir, "yyyy-MM-dd", null);

            IQueryable<ResponseJadwalOperasiFKRTL> queryjadwal = null;

            _logger.LogDebug($"MjknService: GetListJadwalOperasi Ambil jadwal operasi Tanggal: {request.TanggalAwal} s/d {request.TanggalAkhir}");

            // NOTE_JEFRI: we must first convert utc to string then parse to long , before be set LastUpdate value.
            // If we directly use ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds(), then when serialized as JSON, LastUpdate will use timestamp in localtime
            string utcTimeStamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
            long lastUpdateTime = long.Parse(utcTimeStamp);

            queryjadwal   = from bj   in _sqlCtx.BlJadwalBedah
                            join pas  in _sqlCtx.BlPasien    on bj.NomorRekamMedis    equals pas.NomorRekamMedis
                            join dr   in _sqlCtx.BlDokter    on bj.KodeDokterOperator equals dr.KodeDokter
                            join pol  in _sqlCtx.BlPoli      on dr.KodePoli           equals pol.KodePoli
                            join jkn  in _sqlCtx.MjknPoliSub on pol.KodePoliJkn       equals jkn.Kode
                            where bj.TanggalOperasi >= DateOnly.FromDateTime(dtawal) && bj.TanggalOperasi <= DateOnly.FromDateTime(dtakhir)
                            orderby bj.TanggalOperasi descending
                            select new ResponseJadwalOperasiFKRTL
                            {
                                KodeBooking    = bj.NomorJadwal.Trim(),
                                TanggalOperasi = bj.TanggalOperasi.ToString("yyyy-MM-dd"),
                                JenisTindakan  = bj.JenisTindakan.Trim(),
                                KodePoli       = jkn.Kode.Trim(),
                                NamaPoli       = jkn.Nama.Trim(),
                                Terlaksana     = bj.Terlaksana ? 1: 0,
                                NoPeserta      = pas.NomorKartuJkn.Trim(),
                                LastUpdate     = lastUpdateTime
                            };

            return await queryjadwal.ToListAsync();
        }

        public async Task<List<ResponseJadwalOperasiPasien>> JadwalOperasiPasien(RequestJadwalOperasiPasien request)
        {
            IQueryable<ResponseJadwalOperasiPasien> qryKode = null;

            _logger.LogDebug($"MjknService: JadwalOperasiPasien Ambil list jadwal operasi peserta BPJS Nomor: {request.NoPeserta}");

            
            qryKode =   from bj   in _sqlCtx.BlJadwalBedah
                        join pas  in _sqlCtx.BlPasien    on bj.NomorRekamMedis    equals pas.NomorRekamMedis
                        join dr   in _sqlCtx.BlDokter    on bj.KodeDokterOperator equals dr.KodeDokter
                        join pol  in _sqlCtx.BlPoli      on dr.KodePoli           equals pol.KodePoli
                        join jkn  in _sqlCtx.MjknPoliSub on pol.KodePoliJkn       equals jkn.Kode
                        where pas.NomorKartuJkn == request.NoPeserta
                        orderby bj.TanggalOperasi descending
                        select new ResponseJadwalOperasiPasien
                        {
                            KodeBooking    = bj.NomorJadwal.Trim(),
                            TanggalOperasi = bj.TanggalOperasi.ToString("yyyy-MM-dd"),
                            JenisTindakan  = bj.JenisTindakan.Trim(),
                            KodePoli       = jkn.Kode.Trim(),
                            NamaPoli       = jkn.Nama.Trim(),
                            Terlaksana     = bj.Terlaksana ? 1 : 0
                        };

            return await qryKode.ToListAsync();
        }


        // Services terkait WS disisi BPJS
        // -------------------------------------------------------------------------------------
        
        /// Ambil jadwal dokter dari HFIS, berdasarkan * KODE POLI BPJS * ( BUKAN Kode Sub Spesialis)
        public async Task<List<MjknJadwalDokterHfis>> GetJadwalDokterHFIS(string kodePoliJKN, string tanggal)
        {
            _logger.LogDebug("  MjknService: GetReferensiDokter");

            VclaimMjknService vclaimMjknService = new VclaimMjknService(_appSettings, _logger, _configuration);
            var jadwalHfis = await vclaimMjknService.GetReferensiJadwalDokter(kodePoliJKN, tanggal);

            if (jadwalHfis == null) {
                return null;
            }

            int nAdded, nNotAdded, nExist, nUpdated, nNotUpdated;
            nAdded = nNotAdded = nExist = nUpdated = nNotUpdated = 0;

            foreach (var jadwal in jadwalHfis)
            {
                string _kodesubspesialis = string.IsNullOrWhiteSpace(jadwal.KodeSubSpesialis) ? "" : jadwal.KodeSubSpesialis.Trim();
                int    _hari             = (jadwal.Hari == 0) ? -1 : jadwal.Hari;
                int    _kapasitaspasien  = (jadwal.KapasitasPasien == 0) ? -1 : jadwal.KapasitasPasien;
                bool   _libur            = (jadwal.Libur == 0) ? false : true;
                string _namahari         = string.IsNullOrWhiteSpace(jadwal.NamaHari) ? "" : jadwal.NamaHari.Trim();
                string _jadwal           = string.IsNullOrWhiteSpace(jadwal.Jadwal) ? "" : jadwal.Jadwal.Trim();
                string _namasubspesialis = string.IsNullOrWhiteSpace(jadwal.NamaSubspesialis) ? "" : jadwal.NamaSubspesialis.Trim();
                string _namadokter       = string.IsNullOrWhiteSpace(jadwal.NamaDokter) ? "" : jadwal.NamaDokter.Trim();
                string _kodepoli         = string.IsNullOrWhiteSpace(jadwal.KodePoli) ? "" : jadwal.KodePoli.Trim();
                string _namapoli         = string.IsNullOrWhiteSpace(jadwal.NamaPoli) ? "" : jadwal.NamaPoli.Trim();
                int    _kodedokter       = (jadwal.KodeDokter == 0) ? -1 : jadwal.KodeDokter;
                string _jamMulai         = string.IsNullOrWhiteSpace(_jadwal) ? "" : _jadwal.Trim().Substring(0, 5);
                string _jamTutup         = string.IsNullOrWhiteSpace(_jadwal) ? "" : _jadwal.Trim().Substring(6, 5);

                string kodeDokterRs = await GetKodeDokterINT(_kodedokter);
                if (kodeDokterRs == "ERROR_DOKTER_NOT_FOUND_JKN")
                {
                    // Tambahkan data dokter MJKN
                    _logger.LogDebug("  MjknService: GetReferensiDokter menambahkan data MjknDokter Kode: " + _kodedokter.ToString() + " Nama: " + _namadokter);

                    MjknDokter dokter = new MjknDokter
                    {
                        KodedokterInternal = "",
                        KodedokterJkn      = _kodedokter,
                        NamaDokter         = _namadokter
                    };

                    _sqlCtx.MjknDokter.Add(dokter);
                    _sqlCtx.SaveChanges();
                }
                else if (kodeDokterRs == "ERROR_DOKTER_NOT_FOUND_SIMRS")
                {
                }
                else
                {
                    // Existing, update data dokter MJKN
                    var dokterFound = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterJkn == _kodedokter);
                    if (dokterFound.NamaDokter.Trim() != _namadokter)
                    {
                        _logger.LogDebug("  MjknService: GetReferensiDokter Mengupdate data MjknDokter Kode: " + _kodedokter.ToString() + " Nama: " + _namadokter);
                        dokterFound.KodedokterInternal = kodeDokterRs;
                        dokterFound.NamaDokter = _namadokter;
                        _sqlCtx.MjknDokter.Update(dokterFound);
                        _sqlCtx.SaveChanges();
                    }
                }

                var _kodedokterRs = "";
                if ( ! kodeDokterRs.StartsWith("ERROR_DOKTER_NOT_FOUND") ) {
                    _kodedokterRs = kodeDokterRs;
                }

                /////////////////////////////////////////////////////////////////////////////////////////////
                // Hapus data existing (mark dengan _ )
                if (_configuration["AppSettings:HanyaBolehAdaSatuJadwalPadaMjknJadwalDokter"] == true.ToString())
                { 
                    try
                    {
                        // Kita ambil data jadwal tidak berdasarkan jam praktek

                        var oldDataQry = from x in _sqlCtx.MjknJadwalDokter
                                         where x.KodedokterJkn    == jadwal.KodeDokter
                                            && x.KodeSubspesialis == jadwal.KodeSubSpesialis
                                            && x.Hari             == jadwal.Hari
                                            && x.KodePoli         == jadwal.KodePoli
                                         orderby x.Id ascending
                                         select x;

                        var oldDatas = await oldDataQry.ToListAsync();
                        if (oldDatas.Count > 0)
                        {
                            foreach (var old in oldDatas)
                            {
                                var operation       = "";
                                var oldJamMulai     = old.Jadwal.Trim().Substring(0, 5);
                                var oldJamSelesai   = old.Jadwal.Trim().Substring(6, 5);

                                var dtOldJamMulai   = Utils.DateFromStringTime(oldJamMulai);
                                var dtOldJamSelesai = Utils.DateFromStringTime(oldJamSelesai);
                                var dtamMulai       = Utils.DateFromStringTime(_jamMulai);
                                var dtJamSelesai    = Utils.DateFromStringTime(_jamTutup);

                                bool foundDifferent = false;
                                if (oldJamMulai == _jamMulai && oldJamSelesai == _jamTutup )
                                {
                                    if (oldDatas.Count == 1)
                                    {
                                        // Only one data, and  has same data
                                        break;
                                    }
                                }
                                else if (oldJamMulai == _jamMulai && oldJamSelesai != _jamTutup )
                                {
                                    operation = "Remove data jam mulainya sama, jam selesai berbeda";
                                    foundDifferent = true;
                                }
                                else if (oldJamMulai != _jamMulai && oldJamSelesai == _jamTutup)
                                {
                                    operation = "Remove data jam mulainya berbeda, jam selesai sama";
                                    foundDifferent = true;
                                }
                                else
                                {
                                    operation = $"Data existing, oldStart: {oldJamMulai}, oldEnd: {oldJamSelesai}, newStart: {_jamMulai}, newEnd: {_jamTutup}";
                                    foundDifferent = true;
                                }

                                if (foundDifferent)
                                {
                                    try
                                    {
                                        bool updateData = false;

                                        if (updateData)
                                        {
                                            old.KodedokterJkn      = -1 * jadwal.KodeDokter;
                                            old.NamaDokter         = "_" + _namadokter;
                                            old.KodedokterInternal = "_" + _kodedokterRs;
                                            old.LastUpdate         = DateTime.Now;

                                            _sqlCtx.MjknJadwalDokter.Update(old);
                                            _sqlCtx.SaveChanges();

                                        }
                                        else
                                        { 
                                            _sqlCtx.MjknJadwalDokter.Remove(old);
                                            _sqlCtx.SaveChanges();
                                        }

                                        _logger.LogDebug($"  MjknService: GetReferensiDokter {operation}");
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.InnerException != null)
                                            _logger.LogError($"  MjknService: GetReferensiDokter Error {operation} : {ex.Message}  {ex.InnerException.Message}");
                                        else
                                            _logger.LogError($"  MjknService: GetReferensiDokter Error {operation} : {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"  MjknService: GetReferensiDokter Error cleaning data MjknJadwalDokter: {ex.Message}");
                    } 
                }

                /////////////////////////////////////////////////////////////////////////////////////////////

                // Bila ada data yang sama(identical), update
                // Bila tidak ada, buat entri baru
                try
                {
                    // Existing data, update data jadwal dokter MJKN
                    var sch = await _sqlCtx.MjknJadwalDokter.SingleAsync(x =>
                                       x.KodedokterJkn    == jadwal.KodeDokter
                                    && x.KodeSubspesialis == jadwal.KodeSubSpesialis
                                    && x.Hari             == jadwal.Hari
                                    && x.KodePoli         == jadwal.KodePoli
                                    && x.Jadwal           == jadwal.Jadwal);

                    sch.NamaDokter         = _namadokter;
                    sch.KodedokterInternal = _kodedokterRs;
                    sch.NamaPoli           = _namapoli;
                    sch.NamaSubspesialis   = _namasubspesialis;
                    sch.JamMulai           = _jamMulai;
                    sch.JamTutup           = _jamTutup;
                    sch.NamaHari           = _namahari;
                    sch.Libur              = _libur;
                    sch.KapasitasPasien    = _kapasitaspasien;
                    sch.LastUpdate         = DateTime.Now;

                    try
                    {
                        _sqlCtx.MjknJadwalDokter.Update(sch);
                        _sqlCtx.SaveChanges();
                        nUpdated += 1;
                    }
                    catch (Exception)
                    {
                        nNotUpdated += 1;
                    }

                }
                catch (InvalidOperationException)
                {
                    // jadwal dokter jkn belum ada, tambahkan ke database
                    MjknJadwalDokter newSch = new MjknJadwalDokter
                    {
                        KodedokterJkn      = _kodedokter,
                        KodedokterInternal = _kodedokterRs,
                        KodePoli           = _kodepoli,
                        KodeSubspesialis   = _kodesubspesialis,
                        NamaDokter         = _namadokter,
                        NamaPoli           = _namapoli,
                        NamaSubspesialis   = _namasubspesialis,
                        Hari               = _hari,
                        Jadwal             = _jadwal,
                        JamMulai           = _jamMulai,
                        JamTutup           = _jamTutup,
                        NamaHari           = _namahari,
                        Libur              = _libur,
                        KapasitasPasien    = _kapasitaspasien,
                        LastUpdate         = DateTime.Now
                    };

                    try
                    {
                        _sqlCtx.MjknJadwalDokter.Add(newSch);
                        _sqlCtx.SaveChanges();
                        nAdded += 1;
                    }
                    catch(Exception)
                    {
                        nNotAdded += 1;
                    }
                }
                catch (ArgumentNullException ex)
                {
                    _logger.LogError("  MjknService: GetReferensiDokter Error:" + ex.Message);
                    throw new AppException("Error occured when getting jadwal dokter from SIMRS database");
                }
                catch (Exception ex)
                {
                    _logger.LogError("  MjknService: GetReferensiDokter Error:" + ex.Message);
                    throw new AppException("Error occured when getting jadwal dokter from SIMRS database");
                }
            }

            // notifikasi pada user
            string pesan = "";
            if (nAdded > 0)
            {
                pesan = nAdded.ToString() + " data jadwal dokter berhasil ditambahkan ke database";
                _logger.LogDebug("  MjknService: GetReferensiDokter " + pesan);
            }

            if (nNotAdded > 0)
            {
                pesan = nNotAdded.ToString() + " data jadwal dokter gagal ditambahkan ke database";
                _logger.LogDebug("  MjknService: GetReferensiDokter " + pesan);
            }

            if (nUpdated > 0)
            {
                pesan = nUpdated.ToString() + " data jadwal dokter berhasil diupdate ke database";
                _logger.LogDebug("  MjknService: GetReferensiDokter " + pesan);
            }

            if (nNotUpdated > 0)
            {
                pesan = nNotUpdated.ToString() + " data jadwal dokter gagal diupdate ke database";
                _logger.LogDebug("  MjknService: GetReferensiDokter " + pesan);
            }

            return jadwalHfis;
        }

        public async Task<bool> CreateJadwalPraktekDariHFIS(MjknJadwalDokter jadwalHFIS, string tanggalPeriksa)
        {
            if (!Utils.IsValidDate(tanggalPeriksa)) {
                throw new AppException("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }

            DateTime tanggalPraktek = DateTime.ParseExact(tanggalPeriksa, "yyyy-MM-dd", null);
            var kodeDokterINT = await GetKodeDokterINT(jadwalHFIS.KodedokterJkn);

            BlDokter dokter = null;
            try
            {
                dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == kodeDokterINT.Trim());
            }
            catch (Exception)
            {
                _logger.LogError($"  MjknService: CreateJadwalPraktekDariHFIS, Dokter tidak valid, Kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, Kode dokter RS: {kodeDokterINT}");
                throw new AppException($"Dokter tidak valid, Nama: {jadwalHFIS.NamaDokter}, Kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, Kode dokter RS: {kodeDokterINT}");
            }

            var kodePoliINT = await GetKodePoliINT(jadwalHFIS.KodeSubspesialis.Trim());

            if (_configuration["AppSettings:HanyaBolehAdaSatuJadwalPadaBlJadwal"] == true.ToString())
            {
                // Periksa apakah terjadi perubahan jadwal(jam awal,jam akhir) bila berubah, update jam praktek dengan jadwal baru
                await CleanJadwalLocalSehari(kodePoliINT, tanggalPeriksa, kodeDokterINT, jadwalHFIS.JamMulai, jadwalHFIS.JamTutup);
            }

            // Cek/ambil jadwal praktek di table bl_jadwal
            var jadwalDokter = await GetJadwalDokter(kodePoliINT, tanggalPeriksa, kodeDokterINT, jadwalHFIS.JamMulai, jadwalHFIS.JamTutup);
            if (jadwalDokter == null)
            {
                _logger.LogDebug($"  MjknService: CreateJadwalPraktekDariHFIS Menginsert JADWAL Poli: { jadwalHFIS.KodeSubspesialis.Trim()}, Dokter: {kodeDokterINT}, Jam praktek: {jadwalHFIS.Jadwal}, QuotaJKN: {jadwalHFIS.KapasitasPasien}, QuotaNonJKN: {dokter.KapasitasNonJkn}, TglPeriksa: {tanggalPeriksa}");
                // Generate kode jadwal praktek : DR016 + 0045
                var nomorUrutJadwal = dokter.NomorUrutJadwal + 1;
                var kodePraktek = kodeDokterINT.Trim() + Convert.ToInt32(nomorUrutJadwal).ToString("D4");

                // Tambahkan/Buat jadwal praktek baru
                BlJadwal praktek = new BlJadwal
                {
                    KodeDokter   = dokter.KodeDokter.Trim(),
                    KodePoli     = kodePoliINT.Trim(),
                    NamaHari     = jadwalHFIS.NamaHari.Trim(),
                    Keterangan   = "Sinkron dari WSMJKN-HFIS",
                    JamMulai     = jadwalHFIS.JamMulai,
                    JamSelesai   = jadwalHFIS.JamTutup,
                    KodeJadwal   = kodePraktek,
                    Tanggal      = DateOnly.FromDateTime(tanggalPraktek),
                    Libur        = jadwalHFIS.Libur,
                    QuotaNonJkn  = dokter.KapasitasNonJkn,
                    QuotaJkn     = jadwalHFIS.KapasitasPasien,
                    QuotaTotal   = dokter.KapasitasNonJkn + jadwalHFIS.KapasitasPasien // sesuaikan total kuota layanan
                };

                try
                {
                    // Update kuota layanan dokter
                    dokter.KapasitasJkn   = jadwalHFIS.KapasitasPasien;
                    dokter.TotalKapasitas = dokter.KapasitasNonJkn + jadwalHFIS.KapasitasPasien;
                    // Update nomor urut praktek
                    dokter.NomorUrutJadwal = nomorUrutJadwal;
                    _sqlCtx.BlDokter.Update(dokter);


                    // Save new Jadwal
                    _sqlCtx.BlJadwal.Add(praktek);

                    await _sqlCtx.SaveChangesAsync();

                    _logger.LogDebug($"  MjknService: CreateJadwalPraktekDariHFIS Insert succeed with JADWAL Id: {praktek.Id} tglPraktek: {praktek.Tanggal.ToString("yyyy-MM-dd")}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"  MjknService: CreateJadwalPraktekDariHFIS Kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, Kode dokter RS: {kodeDokterINT}, Gagal menyimpan data: {ex.Message}");
                    if (ex.InnerException!=null)
                    {
                        _logger.LogError($"  InnerException:  {ex.InnerException.Message}");
                    }
                    
                    throw new AppException($"Gagal membuat jadwal, Kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, Kode dokter RS: {kodeDokterINT}, gagal menyimpan data");
                }
            }
            else
            {
                // Jadwal sudah ada (Harus ada hanya SATU jadwal)
                // cek apakah slot sudah dibuat di bl_jadwal
                var existingSlot = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalDokter.Id);
                if (existingSlot > 0)
                {
                    // Bila slot sudah ada, jangan update jadwal di bl_jadwal
                    // Cukup update quota di bl_dokter

                    _logger.LogDebug($"  MjknService: CreateJadwalPraktekDariHFIS Slot antrian pada ANTRIAN sudah ada: {existingSlot}, JADWAL Id: {jadwalDokter.Id.ToString()} Poli: {jadwalDokter.NamaPoli.Trim()}, Dokter: {kodeDokterINT}, Jam praktek: {jadwalHFIS.Jadwal}");

                    try
                    {
                        // Update kuota layanan dokter
                        dokter.KapasitasJkn = jadwalHFIS.KapasitasPasien;
                        dokter.TotalKapasitas = dokter.KapasitasNonJkn + jadwalHFIS.KapasitasPasien;

                        _sqlCtx.BlDokter.Update(dokter);

                        await _sqlCtx.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"  MjknService: CreateJadwalPraktekDariHFIS Kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, Kode dokter RS: {kodeDokterINT}, Gagal menyimpan data quota dokter: {ex.Message}");
                        throw new AppException($"Gagal menyimpan data quota dokter, kode dokter BPJS: {jadwalHFIS.KodedokterJkn}, kode dokter RS: {kodeDokterINT}");
                    }
                }
                else
                { 
                    string jadwalId      = jadwalDokter.Id.ToString();
                    string namaPos       = jadwalDokter.NamaPoli.Trim();
                    string namaDokter    = jadwalDokter.NamaDokter.Trim();

                    var praktek          = _sqlCtx.BlJadwal.Find(jadwalDokter.Id);
                    //praktek.NamaHari   = jadwalHFIS.NamaHari.Trim();
                    praktek.Keterangan   = "Update dari WSMJKN-HFIS";
                    praktek.Libur        = jadwalHFIS.Libur;
                    praktek.QuotaNonJkn  = dokter.KapasitasNonJkn;
                    praktek.QuotaJkn     = jadwalHFIS.KapasitasPasien;
                    praktek.QuotaTotal   = dokter.KapasitasNonJkn + jadwalHFIS.KapasitasPasien;

                    _logger.LogDebug($"  MjknService: CreateJadwalPraktekDariHFIS Mengupdate JADWAL Id: {jadwalDokter.Id} Poli: {jadwalDokter.NamaPoli.Trim()}, Dokter: {kodeDokterINT}, Jam praktek: {jadwalHFIS.Jadwal}, QuotaJKN_Old: {praktek.QuotaJkn}, QuotaJKN_New: {jadwalHFIS.KapasitasPasien}, QuotaNonJKN: {dokter.KapasitasNonJkn}");

                    _sqlCtx.BlJadwal.Update(praktek);

                    // Update kuota layanan dokter
                    dokter.KapasitasJkn = jadwalHFIS.KapasitasPasien;
                    dokter.TotalKapasitas  = dokter.KapasitasNonJkn + jadwalHFIS.KapasitasPasien;

                    _sqlCtx.BlDokter.Update(dokter);

                    await _sqlCtx.SaveChangesAsync();
                    return true;
                }
            }

            return true;
        }

        public async Task<bool> CreateJadwalPraktekDariHFIS(string tglPraktek, string kodeSubPoliJKN, int kodeDokterJKN, string jamMulai, string jamSelesai)
        {
            MjknJadwalDokter jadwalPraktekMjkn = null;

            try
            {
                string namaHari = Utils.ISODateTimeToIndonesianDay(tglPraktek).ToUpper();

                // Update data jadwal dokter MJKN
                jadwalPraktekMjkn = await _sqlCtx.MjknJadwalDokter.SingleAsync(x =>
                                                       x.KodedokterJkn             == kodeDokterJKN
                                                    && x.NamaHari.Trim().ToUpper() == namaHari
                                                    && x.JamMulai                  == jamMulai
                                                    && x.JamTutup                  == jamSelesai
                                                    && x.KodeSubspesialis          == kodeSubPoliJKN
                                                );

                return await CreateJadwalPraktekDariHFIS(jadwalPraktekMjkn, tglPraktek);
            }
            catch (InvalidOperationException)
            {
                _logger.LogError($"  MjknService: CreateJadwalPraktekDariHFIS Jadwal tidak ada, Tanggal: {tglPraktek}, Sub Poli BPJS: {kodeSubPoliJKN}, Dokter BPJS: {kodeDokterJKN.ToString()}, Jam: {jamMulai}-{jamSelesai}");
            }
            return false;
        }

        public async Task TambahAntrian(string kodebooking)
        {
            _logger.LogDebug("  MjknService: TambahAntrian memposting data antrian kodebooking: " + kodebooking + " ke web service BPJS");

            try
            {
                // TODO_JEFRI: Periksa apakah tanggal di bl_antrian null

                var qryReserv = from j  in _sqlCtx.BlAntrian
                                join np in _sqlCtx.BlPoli      on j.KodePoli        equals np.KodePoli
                                join ps in _sqlCtx.MjknPoliSub on np.KodePoliJkn    equals ps.Kode
                                join dj in _sqlCtx.MjknDokter  on j.KodeDokter      equals dj.KodedokterInternal
                                join d  in _sqlCtx.BlDokter    on j.KodeDokter      equals d.KodeDokter
                                join p  in _sqlCtx.BlPasien    on j.NomorRekamMedis equals p.NomorRekamMedis
                                where j.TokenAntrian == kodebooking
                                select new RequestMjknTambahAntrian
                                {
                                    KodeBooking      = j.TokenAntrian,
                                    JenisPasien      = (j.InsuranceId.Trim() == Insurance.BPJS ? "BPJS" : "NON BPJS"),
                                    NomorKartu       = j.NomorKartuJkn.Trim(),
                                    Nik              = p.NomorIdentitas.Trim(),
                                    NoHp             = j.Phone.Trim(),
                                    KodePoli         = ps.Kode.Trim(),
                                    NamaPoli         = ps.Nama.Trim(),
                                    PasienBaru       = (j.PasienBaru == false) ? 0 : 1,
                                    NoRm             = j.NomorRekamMedis.Trim(),
                                    TanggalPeriksa   = j.Tanggal.Value.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-dd"),
                                    KodeDokter       = dj.KodedokterJkn,
                                    NamaDokter       = d.NamaDokter.Trim(),
                                    JamPraktek       = j.JamPraktek.Trim(),
                                    JenisKunjungan   = j.JenisKunjungan,
                                    NomorReferensi   = j.NomorRujukan.Trim(),
                                    NomorAntrean     = j.NomorAntrian.Trim(),
                                    AngkaAntrean     = j.NomorAntrianInt,
                                    EstimasiDilayani = j.EstimasiDilayani,
                                    SisaKuotaJkn     = j.KuotaJkn,
                                    KuotaJkn         = j.SisaKuotaJkn,
                                    SisaKuotaNonJkn  = j.SisaKuotaNonJkn,
                                    KuotaNonJkn      = j.KuotaNonJkn,
                                    Keterangan       = j.Keterangan
                                };

                var reservationData = await qryReserv.SingleAsync();

                var vclaimMjknService = new VclaimMjknService(_appSettings, _logger, _configuration);

                var success = await vclaimMjknService.TambahAntrian(reservationData);
            }
            catch (BpjsException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("  MjknService: TambahAntrian Data kodebooking: " + kodebooking + " tidak ditemukan di database");

                throw new AppException("Kode booking tidak ditemukan");
            }
            catch (Exception ex)
            {
                _logger.LogError("  MjknService: TambahAntrian Data kodebooking: " + kodebooking + ", terjadi error: " + ex.Message);

                throw new AppException(ex.Message);
            }
        }

        public async Task BatalAntrian(string kodebooking, string keterangan)
        {
            _logger.LogDebug("  MjknService: BatalAntrian Memposting data pembatalan antrian kodebooking: " + kodebooking + " ke web service BPJS");

            var batalData = new RequestMjknBatalAntrian
            {
                KodeBooking = kodebooking,
                Keterangan  = keterangan
            };

            try
            {
                VclaimMjknService vclaimMjknService = new VclaimMjknService(_appSettings, _logger, _configuration);

                var success = await vclaimMjknService.BatalAntrian(batalData);
            }
            catch (BpjsException)
            {
                throw;
            }
        }

        public async Task UpdateWaktuCheckinAntrian(string KodeBooking, ulong waktu, int taskId)
        {
            _logger.LogDebug($"  MjknService: UpdateWaktuCheckinAntrian Memposting data update waktu check-in antrian kodebooking: {KodeBooking} ke web service BPJS");
            

            var updateData = new RequestMjknUpdateWaktu
            {
                KodeBooking = KodeBooking,
                TaskId      = taskId,
                Waktu       = waktu
            };

            try
            {
                VclaimMjknService vclaimMjknService = new VclaimMjknService(_appSettings, _logger, _configuration);

                var success = await vclaimMjknService.UpdateWaktuAntrian(updateData);
            }
            catch (BpjsException)
            {
                throw;
            }
        }


        // Digunakan secara internal SIMRS, untuk mensinkron jadwal prakter di HFIS
        // menjadi jadwal praktek di database SIMRS
        // -------------------------------------------------------------------------------------
        public async Task<object> SinkronJadwalPraktekDokterDariDataHFIS(string kodePoliRS, string tglPraktek)
        {
            string kodeSubPoliJkn = await GetKodeSubPoliJKN(kodePoliRS);
            if (kodeSubPoliJkn == "ERROR_POLI_NOT_FOUND") {
                throw new AppException("Kode Poli RS/BPJS tidak valid");
            }

            // Cari kode poli bpjs dari kode sub polinya
            var kodePoliBPJS = await GetKodePoliJKN(kodeSubPoliJkn);
            if (kodePoliBPJS == "ERROR_POLI_NOT_FOUND") {
                throw new AppException("Kode Poli BPJS tidak valid");
            }

            // Periksa dulu jadwal dokter di database MJKN HFIS, dan bila ada simpan ke database SIMRS
            // ambil jadwal berdasarkan kode poli bpjs (bukan kode subspesialis bpjs)
            var listJadwalHfis = await GetJadwalDokterHFIS(kodePoliBPJS, tglPraktek);

            try
            {
                string namaHari = Utils.ISODateTimeToIndonesianDay(tglPraktek).ToUpper();

                // Update data jadwal dokter MJKN
                var listJadwal = await _sqlCtx.MjknJadwalDokter.Where(x =>
                                                           x.NamaHari.Trim().ToUpper() == namaHari
                                                        && x.KodeSubspesialis == kodeSubPoliJkn
                                                        && x.KodedokterJkn > 0 ).ToListAsync();
                                            
                int total       = listJadwal.Count();
                int processed   = 0;
                int failed      = 0;
                var messageList = new List<String>();

                foreach (var jadwal in listJadwal)
                {
                    try
                    {
                        bool ok = await CreateJadwalPraktekDariHFIS(jadwal, tglPraktek);
                        if (ok) {
                            processed++;
                        }
                    }
                    catch(AppException ex)
                    {
                        failed++;
                        messageList.Add(ex.Message);
                    }
                }

                return new { Processed = processed, Failed = failed, Messages = messageList };
            }
            catch (InvalidOperationException)
            {
                _logger.LogError($"  MjknService: SinkronJadwalPraktekDokterDariDataHFIS Jadwal tidak valid, Tanggal: {tglPraktek}, Poli BPJS: {kodeSubPoliJkn}");
                throw new AppException("Jadwal belum tersedia, gagal sinkron jadwal hfis");
            }
        }


        /*
         * appSource :
                AND_WEBSVC   : via aplikasi Android Tobasa untuk pasien BPJS dan non BPJS
                SIMRS_WEBSVC : via aplikasi SIMRS untuk pasien BPJS dan non BPJS
                MJKN_WEBSVC  : via aplikasi Mobile BPJS BPJS
         */
        public async Task<CekSlotResult> CekSlotReservasi(long jadwalId, string appSource)
        {
            try
            {
                if ( await _sqlCtx.BlJadwal.CountAsync(x => x.Id == jadwalId) != 1 ) {
                    return new CekSlotResult("ERROR_RCS_INVALID_JADWAL_ID");
                }
                var jadwal = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == jadwalId);

                if (await _sqlCtx.BlDokter.CountAsync(x => x.KodeDokter == jadwal.KodeDokter) != 1) {
                    return new CekSlotResult("ERROR_RCS_INVALID_DOCTOR_CODE");
                }
                var dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == jadwal.KodeDokter);

                if ( jadwal.JamMulai.Trim().Length < 5 || jadwal.JamSelesai.Trim().Length < 5 )
                {
                    _logger.LogError("  MjknService: CekSlotReservasi Format/length jam mulai/selesai pada BLJADWAL tidak valid");
                    return new CekSlotResult("ERROR_RCS_INVALID_TJADWAL");
                }

                if ( ((dokter.KapasitasNonJkn + dokter.KapasitasJkn) != dokter.TotalKapasitas) || dokter.TotalKapasitas == 0)
                {
                    _logger.LogError("  MjknService: CekSlotReservasi Parameter quota antrian pada DOKTER tidak valid, JADWAL Id: " + jadwalId);
                    return new CekSlotResult("ERROR_RCS_INVALID_TDOKTER");
                }

                if ( jadwal.QuotaNonJkn != dokter.KapasitasNonJkn ||
                     jadwal.QuotaJkn    != dokter.KapasitasJkn    ||
                     jadwal.QuotaTotal  != dokter.TotalKapasitas )
                {
                    // Telah terjadi perubahan data master dokter
                    _logger.LogWarning($"  MjknService: CekSlotReservasi Data quota antrian berubah, JADWAL Id: {jadwalId}");
                    _logger.LogWarning($"    Jadwal.QuotaNonJkn: {jadwal.QuotaNonJkn},    Jadwal.QuotaJkn: {jadwal.QuotaJkn},    Jadwal.QuotaTotal: {jadwal.QuotaTotal}");
                    _logger.LogWarning($"    Dokter.QuotaNonJkn: {dokter.KapasitasNonJkn}, Dokter.QuotaJkn: {dokter.KapasitasJkn}, Dokter.QuotaTotal: {dokter.TotalKapasitas}");
                }

                // Cek apakah slot antrian layanan untuk tanggal praktek yg diinginkan telah tersedia/ diset pada bl_jadwal
                var reservedSlot = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId);
                if ( reservedSlot > 0 )
                {
                    if (reservedSlot >= jadwal.QuotaTotal )
                    {
                        // Slot antrian telah ada, dan informasi quota telah diset pada table bl_jadwal

                        // quota antrian untuk pasien BPJS yang sudah digunakan
                        var quotaJknUsed = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId == Insurance.BPJS);
                        // quota antrian untuk pasien non BPJS yang sudah digunakan
                        var quotaNonJknUsed = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalId && x.NomorRekamMedis.Length > 0 && x.InsuranceId != Insurance.BPJS);

                        _logger.LogDebug($"  MjknService: CekSlotReservasi Slot Jadwal: {jadwal.QuotaTotal}, Slot Aktual: {reservedSlot}, QuotaJknUsed: {quotaJknUsed}, QuotaNonJknUsed: {quotaNonJknUsed}, JADWAL Id: " + jadwalId);

                        return new CekSlotResult
                        {
                            Message          = "SUCCESS_RCS_QUOTA_AVAILABLE",
                            TotalQuota       = Convert.ToInt32(dokter.TotalKapasitas),
                            QuotaBpjsUsed    = quotaJknUsed,
                            QuotaNonBpjsUsed = quotaNonJknUsed,
                            QuotaBpjs        = Convert.ToInt32(jadwal.QuotaJkn),
                            QuotaNonBpjs     = Convert.ToInt32(jadwal.QuotaNonJkn),
                            Kodepos          = jadwal.KodePoli
                        };
                    }
                    else
                    {
                        _logger.LogError($"  MjknService: CekSlotReservasi Slot reservasi ANTRIAN tidak sesuai dengan jumlah quota di JADWAL Id: {jadwalId} ");
                        return new CekSlotResult("ERROR_RCS_INVALID_JADWAL_ANTRIAN");
                    }
                }
                else
                {
                    // Slot reservasi belum digenerate pada bl_jadwal
                    _logger.LogDebug($"  MjknService: CekSlotReservasi Membuat slot reservasi, JADWAL Id: {jadwalId} ");

                    // Untuk quota, gunakan data dari master dokter
                    // Update bl_jadwal, dengan data quota dari bl_dokter
                    jadwal.QuotaNonJkn = dokter.KapasitasNonJkn;
                    jadwal.QuotaJkn    = dokter.KapasitasJkn;
                    jadwal.QuotaTotal  = dokter.TotalKapasitas;

                    _sqlCtx.BlJadwal.Update(jadwal);
                    await _sqlCtx.SaveChangesAsync();

                    // Generate slot reservasi antrian sebanyak total quota
                    // --------------------------------------------------------
                    int antrino    = 0;
                    int interval   = dokter.PasienTime;
                    // pastikan format jam mulai/selesai benar => 00:00
                    var xJamMulai  = jadwal.JamMulai.Trim().Substring(0, 5);
                    var dtJamMulai = DateTime.ParseExact(xJamMulai, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);

                    while (antrino < jadwal.QuotaTotal)
                    {
                        antrino += 1;
                        
                        if (antrino >1)
                            dtJamMulai = dtJamMulai.AddMinutes(interval);

                        var slotJanji = CreateAntrian(jadwal, appSource, dtJamMulai, antrino);
                        
                        _sqlCtx.BlAntrian.Add(slotJanji);
                        await _sqlCtx.SaveChangesAsync();
                    }

                    _logger.LogDebug("  MjknService: CekSlotReservasi Slot reservasi telah dibuat sebanyak: " + jadwal.QuotaTotal + ", JADWAL Id: " + jadwalId);

                    var slotResult = new CekSlotResult
                    {
                        Message          = "SUCCESS_RCS_QUOTA_CREATED",
                        TotalQuota       = Convert.ToInt32(jadwal.QuotaTotal),
                        QuotaBpjsUsed    = 0,
                        QuotaNonBpjsUsed = 0,
                        SlotsAvailable   = Convert.ToInt32(jadwal.QuotaTotal),
                        QuotaBpjs        = Convert.ToInt32(jadwal.QuotaJkn),
                        QuotaNonBpjs     = Convert.ToInt32(jadwal.QuotaNonJkn),
                        Kodepos          = jadwal.KodePoli
                    };

                    return slotResult;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: CekSlotReservasi JADWAL Id: {jadwalId}, " + ex.ToString());
                throw new AppException("Gagal membuat reservasi, code: 2032");
            }
        }

        public async Task<ResultAmbilAntrianFKRTL> GetAppointment( DataAmbilAntrianFKRTL request, long jadwalID)
        {
            if ( request.JenisPasien == Insurance.BPJS)
                _logger.LogDebug("  MjknService: GetAppointment Ambil antrian baru untuk peserta No.BPJS: " + request.NomorKartu);
            else
                _logger.LogDebug("  MjknService: GetAppointment Ambil antrian baru untuk pasien No.RM: " + request.NomorRekamMedis);

            try
            {
                BlAntrian reservasi = null;
                var rMessage = "";

                // --------------------------------------------------------------------------------
                // Reservasi via Aplikasi SIMRS dan Aplikasi Android Tobasa
                // cocokkan MRN, tanggal praktek, kode dokter dan jenis pasien
                if ((request.AppSource == "SIMRS_WEBSVC" || request.AppSource == "AND_WEBSVC") && !string.IsNullOrWhiteSpace(request.NomorRekamMedis))
                {
                    try
                    {
                        if (request.JenisPasien == Insurance.BPJS)
                        {
                            reservasi = await _sqlCtx.BlAntrian.SingleAsync(x =>
                                               x.JadwalId        == jadwalID
                                            && x.NomorRekamMedis == request.NomorRekamMedis
                                            && x.InsuranceId     == request.JenisPasien
                                            && x.NomorRujukan    == request.NomorReferensi
                                            && x.StatusAntri     >= StatusAntri.BOOKED );
                        }
                        else
                        {
                            reservasi = await _sqlCtx.BlAntrian.SingleAsync(x =>
                                               x.JadwalId        == jadwalID
                                            && x.NomorRekamMedis == request.NomorRekamMedis
                                            && x.InsuranceId     == request.JenisPasien
                                            && x.StatusAntri     >= StatusAntri.BOOKED);
                        }

                        rMessage = "SUCCESS_EXISTS";
                    }
                    catch (Exception)
                    {
                        _logger.LogDebug("  MjknService: GetAppointment, Antrian belum ada");
                    }
                }
                // Reservasi via aplikasi MobileJKN BPJS
                // reservasi via MJKN wajib memiliki nomor rujukan yang masih valid (90 hari)
                // cocokkan MRN, tanggal praktek, kode dokter dan jenis pasien dan nomor rujukan bpjs dan Status Antri TIDAK DIBATALKAN
                // pengecekan ini untuk pasien yang sudah terdaftar sebagai pasien di RS (memiliki MRN)
                else if (request.AppSource == "MJKN_WEBSVC" && !string.IsNullOrWhiteSpace(request.NomorRekamMedis) && request.JenisPasien == Insurance.BPJS)
                {
                    try
                    {
                        reservasi = await _sqlCtx.BlAntrian.SingleAsync(x =>
                                           x.JadwalId        == jadwalID
                                        && x.NomorRekamMedis == request.NomorRekamMedis
                                        && x.InsuranceId     == request.JenisPasien
                                        && x.NomorRujukan    == request.NomorReferensi
                                        && x.StatusAntri     >= StatusAntri.BOOKED);

                        rMessage = "SUCCESS_EXISTS";
                    }
                    catch (Exception)
                    {
                        _logger.LogDebug("  MjknService: GetAppointment, Antrian belum ada");
                    }
                }
                else
                {
                    // SHOULD never reach here
                    _logger.LogError("  MjknService: GetAppointment 5107, No matching parameters App source, Jenis Pasien and MRN when checking for existing antrian");
                    throw new AppException("Internal error: 5107");
                }

                // --------------------------------------------------------------------------------------

                if (rMessage == "SUCCESS_EXISTS")
                {
                    if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
                    {
                        await SaveAntrianTransaction(request, reservasi);
                    }

                    var antrianSummary = await GetAntrianSummary(jadwalID);
                    var result = new ResultAmbilAntrianFKRTL
                    {
                        Message           = "SUCCESS_EXISTS",
                        RegistrationToken = reservasi.TokenAntrian,
                        IssuedAt          = reservasi.BookedAt.ToString("s"), // yyyy-MM-ddTHH:mm:ss
                        NomorAntrian      = reservasi.NomorAntrian,
                        AngkaAntrian      = reservasi.NomorAntrianInt,
                        EstimatedTime     = reservasi.JamMulai,
                        EstimatedTimeUnix = reservasi.EstimasiDilayani,
                        AntrianSummary    = antrianSummary,
                        StatusAntri       = reservasi.StatusAntri,
                        NamaPoli          = await GetNamaPoliINT(reservasi.KodePoli),
                        Keterangan        = reservasi.Keterangan,
                        ReturnCode        = 0
                    };

                    return result;
                }
                else
                {
                    // Create new antrian   
                    var jadwal = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == jadwalID);
                    AntrianSummary antrianSummary = null;

                    var jamMulaiDt = DateTime.Now;
                    int newNumber = 0;

                    var totalAntri = await _sqlCtx.BlAntrian.CountAsync(x => x.JadwalId == jadwalID);
                    if (totalAntri > 0 )
                    {
                        var dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == jadwal.KodeDokter);
                        int interval = dokter.PasienTime;

                        // sudah ada antrian/reservasi, cari nomor antrian terakhir
                        var lastNumber = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalID).Select(s => s.NomorAntrianInt).MaxAsync();
                        var lastAntri  = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalID && x.NomorAntrianInt == lastNumber).SingleAsync();

                        // Sesuaikan jam mulai/estimasi pelayanan antrian
                        if (DateOnly.FromDateTime(DateTime.Now.Date) == jadwal.Tanggal)
                        {
                            // Pengambilan antrian dilakukan pada hari H

                            // cek apakah antrian terakhir dibooking pada hari-H
                            if (lastAntri.BookedAt.Date == DateTime.Now.Date) { }

                            //// pastikan format jam mulai/selesai benar => 00:00
                            //var lastJamMulaiStr = lastAntri.JamMulai.Trim().Substring(0, 5);
                            //var lastJamMulaiDt = DateTime.ParseExact(lastJamMulaiStr, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(lastAntri.EstimasiDilayani);
                            DateTime lastJamMulaiDt = dto.LocalDateTime;
                            var newJamMulaiDt = lastJamMulaiDt.AddMinutes(interval);
                            var dtNow = DateTime.Now;
                            if (dtNow >= newJamMulaiDt)
                                jamMulaiDt = dtNow;
                            else
                                jamMulaiDt = newJamMulaiDt;
                        }
                        else
                        {
                            // untuk pengambilan harian sebelum hari-H, ikuti saja interval jam mulai antrian terakhir
                            DateTimeOffset dto = DateTimeOffset.FromUnixTimeMilliseconds(lastAntri.EstimasiDilayani);
                            DateTime lastJamMulaiDt = dto.LocalDateTime;
                            jamMulaiDt = lastJamMulaiDt.AddMinutes(interval);
                        }

                        newNumber = lastNumber + 1;
                        antrianSummary = await GetAntrianSummary(jadwalID);
                    }
                    else
                    {
                        // Antrian pertama, jam mulai sesuai jadwal
                        // pastikan format jam mulai/selesai benar => 00:00
                        var jamMulaiStr = jadwal.JamMulai.Trim().Substring(0, 5);
                        jamMulaiDt = DateTime.ParseExact(jamMulaiStr, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                        newNumber = 1;
                    }

                    int sisaQuotaJkn = 0;
                    int sisaQuotaNonJkn = 0;
                    if (antrianSummary != null && antrianSummary.LastNumber>0)
                    {
                        sisaQuotaJkn    = antrianSummary.QuotaJkn - antrianSummary.QuotaJknUsed;
                        sisaQuotaNonJkn = antrianSummary.QuotaNonJkn - antrianSummary.QuotaNonJknUsed;
                    }
                    else
                    {
                        sisaQuotaJkn    = jadwal.QuotaJkn;
                        sisaQuotaNonJkn = jadwal.QuotaNonJkn;
                    }

                    // if BPJS quota is exhausted, cannot reserve
                    if (sisaQuotaJkn <= 0 && request.JenisPasien == Insurance.BPJS)
                    {
                        _logger.LogError($"  MjknService: GetAppointment 5107A, Sisa kuota BPJS untuk jadwalId {jadwalID} sudah habis");
                        throw new AppException("Kuota BPJS sudah habis");
                    }

                    // if Non BPJS quota is exhausted, cannot reserve
                    if (sisaQuotaNonJkn <= 0 && request.JenisPasien != Insurance.BPJS)
                    {
                        _logger.LogError($"  MjknService: GetAppointment 5107B, Sisa kuota Non BPJS untuk jadwalId {jadwalID} sudah habis");
                        throw new AppException("Kuota Non BPJS sudah habis");
                    }

                    if (request.JenisPasien == Insurance.BPJS) 
                    {
                        sisaQuotaJkn -= 1;     // reservation decreases BPJS quota
                        jadwal.QuotaJknUsed++;
                    }
                    else 
                    {
                        sisaQuotaNonJkn -= 1;  // reservation decreases Non BPJS quota
                        jadwal.QuotaNonJknUsed++;
                    }

                    // create a new antrian, then update all the fields 
                    var newAntrian = CreateAntrian(jadwal, request.AppSource, jamMulaiDt, newNumber );

                    var pasien = await _sqlCtx.BlPasien.SingleAsync(x => x.NomorRekamMedis == request.NomorRekamMedis);

                    newAntrian.NamaPasien      = pasien.Nama;
                    newAntrian.Alamat          = pasien.Alamat;
                    newAntrian.Phone           = string.IsNullOrWhiteSpace(request.NomorHP) ? pasien.Phone : request.NomorHP;
                    newAntrian.NomorRekamMedis = request.NomorRekamMedis;
                    newAntrian.BookedAt        = DateTime.Now;     // waktu reservasi dibuat
                    //newAntrian.EditedBy      = request.AppSource;
                    newAntrian.InsuranceId     = request.JenisPasien;
                    newAntrian.TanggalLahir    = pasien.TanggalLahir;
                    newAntrian.NomorKartuJkn   = request.NomorKartu;
                    newAntrian.NomorRujukan    = request.NomorReferensi;
                    newAntrian.NomorAntrian    = jadwal.KodePoli + "-" + newNumber;
                    newAntrian.SisaKuotaJkn    = sisaQuotaJkn;
                    newAntrian.SisaKuotaNonJkn = sisaQuotaNonJkn;
                    newAntrian.KuotaJkn        = jadwal.QuotaJkn;
                    newAntrian.KuotaNonJkn     = jadwal.QuotaNonJkn;
                    newAntrian.JenisKunjungan  = request.JenisKunjungan;

                    // NOTE_JEFRI: untuk FKTP,Keterangan berisi keluhan pasien
                    newAntrian.Keterangan      = "";
                    // NOTE_JEFRI: test only
                    //SaveToAntrianNote("Keluhan", request.Keluhan, newAntrian);  

                    // Save to database
                    _sqlCtx.BlAntrian.Add(newAntrian);
                    await _sqlCtx.SaveChangesAsync();

                    // kunci jadwal, karena sudah ada yang antri
                    jadwal.Locked = true;

                    _sqlCtx.BlJadwal.Update(jadwal);
                    await _sqlCtx.SaveChangesAsync();
                    

                    // Update antrian status history
                    var newAntrianUpdated = await _sqlCtx.BlAntrian
                        .Where(x => x.JadwalId == jadwalID && x.TokenAntrian == newAntrian.TokenAntrian)
                        .SingleAsync();
                    
                    // Note_JEFRI: untuk FKRTL tidak ada keluhan, hanya untuk FKTP
                    await UpdateAntrianStatusHistory(newAntrianUpdated, "");

                    // Save transaction history
                    if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
                    {
                        await SaveAntrianTransaction(request, newAntrianUpdated);
                    }

                    // get updated antrian summary so we can get latest stats of jadwal
                    antrianSummary = await GetAntrianSummary(jadwalID);
                    // return the result
                    var result = new ResultAmbilAntrianFKRTL
                    {
                        Message           = "SUCCESS",
                        RegistrationToken = newAntrian.TokenAntrian,
                        IssuedAt          = newAntrian.BookedAt.ToString("s"), // yyyy-MM-ddTHH:mm:ss
                        NomorAntrian      = newAntrian.NomorAntrian,
                        AngkaAntrian      = newAntrian.NomorAntrianInt,
                        EstimatedTime     = newAntrian.JamMulai,
                        EstimatedTimeUnix = newAntrian.EstimasiDilayani,
                        AntrianSummary    = antrianSummary,
                        StatusAntri       = newAntrian.StatusAntri,
                        NamaPoli          = await GetNamaPoliINT(newAntrian.KodePoli),
                        Keterangan        = newAntrian.Keterangan,
                        ReturnCode        = 0
                    };
                    return result;
                }
            }
            catch (AppException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDebug("  MjknService: GetAppointment " + ex.ToString());
                _logger.LogError("  MjknService: GetAppointment " + ex.Message);
                throw;
            }
        }

        // Digunakan secara internal SIMRS untuk menggenerate jadwal praktek
        // -------------------------------------------------------------------------------------
        public async Task<int> CreateJadwalPraktek(RequestCreateJadwalPraktek request)
        {
            var found = await _sqlCtx.BlDokter.CountAsync(x => x.KodeDokter == request.KodeDokter);
            if (found != 1)
                throw new AppException("Data dokter tidak ditemukan");

            var dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == request.KodeDokter);

            if ( ((dokter.KapasitasNonJkn + dokter.KapasitasJkn) != dokter.TotalKapasitas) || dokter.TotalKapasitas == 0 || string.IsNullOrWhiteSpace(dokter.KodePoli))
                throw new AppException("Data quota antrian DOKTER tidak valid");

            var currDate = DateTime.ParseExact(request.TanggalAwal, "yyyy-MM-dd", null).Date;
            var tglAkhir = currDate.Date.AddDays(request.JumlahHari);

            int i = 0;
            while (currDate.Date < tglAkhir.Date)
            {
                i++;
                var namaHari = Utils.ISODateTimeToIndonesianDay(currDate).ToUpper();

                //if (currDate.Kind == DateTimeKind.Unspecified)
                //    currDate = DateTime.SpecifyKind(currDate, DateTimeKind.Utc);  

                var jadwalExist = await _sqlCtx.BlJadwal.CountAsync(x =>
                                    x.KodeDokter == request.KodeDokter
                                && x.KodePoli == dokter.KodePoli
                                && x.Tanggal == DateOnly.FromDateTime(currDate)
                                && x.JamMulai.Trim().Substring(0, 5) == request.JamMulai
                                && x.JamSelesai.Trim().Substring(0, 5) == request.JamAkhir);

                if (jadwalExist >= 1)
                {
                    currDate = currDate.Date.AddDays(1);
                    continue;
                }
                else
                {
                    // Generate kode praktek : DR016 + 0045
                    var nomorUrutJadwal = dokter.NomorUrutJadwal + 1;
                    var kodePraktek = dokter.KodeDokter.Trim() + Convert.ToInt32(nomorUrutJadwal).ToString("D4");

                    _logger.LogDebug($"  MjknService: CreateJadwalPraktek Insert hari: {namaHari}, kodePraktek: { kodePraktek}");
                    // Tambahkan jadwal praktek
                    BlJadwal praktek = new BlJadwal
                    {
                        KodeDokter   = dokter.KodeDokter.Trim(),
                        KodePoli     = dokter.KodePoli.Trim(),
                        NamaHari     = namaHari,
                        Keterangan   = "Created by WSMJKN-SIMRS",
                        JamMulai     = request.JamMulai,
                        JamSelesai   = request.JamAkhir,
                        KodeJadwal   = kodePraktek,
                        Tanggal      = DateOnly.FromDateTime(currDate),
                        Libur        = false,
                        Locked       = false,
                        QuotaNonJkn  = dokter.KapasitasNonJkn,
                        QuotaJkn     = dokter.KapasitasJkn,
                        QuotaTotal   = dokter.TotalKapasitas,
                        QuotaJknUsed    = 0,
                        QuotaNonJknUsed = 0
                    };

                    // Update nomor urut praktek
                    dokter.NomorUrutJadwal = nomorUrutJadwal;
                    _sqlCtx.BlDokter.Update(dokter);

                    // Save new DrPraktek
                    _sqlCtx.BlJadwal.Add(praktek);

                    await _sqlCtx.SaveChangesAsync();

                    _logger.LogDebug($"  MjknService: CreateJadwalPraktek Insert succeed with JADWAL Id: {praktek.Id} TglPraktek: {currDate.ToString("yyyy-MM-dd")}");

                    currDate = currDate.Date.AddDays(1);
                }
            }

            return i;
        }

        private BlAntrian CreateAntrian(BlJadwal jadwal, string appSource, DateTime dtJamMulai, int antriNumber, string note="")
        {
            // pastikan format jam mulai/selesai benar => 00:00
            var mulai   = jadwal.JamMulai.Trim().Substring(0, 5);
            var selesai = jadwal.JamSelesai.Trim().Substring(0, 5);
            // jadwal praktek: 14:00-16:00
            var jamPraktek  = mulai + "-" + selesai;

            // 12 chars
            string kodeBooking = (DateTime.Now.ToString("yyMMdd") + Utils.CreateRandomString(6)).ToUpper();

            // untuk bl_jadwal format jam mulai => HH:mm:ss - 24 hour fomat
            var jamMulai = dtJamMulai.ToString("T", CultureInfo.InvariantCulture); // 17:10:23

            var datetimeJanji = new DateTime(jadwal.Tanggal.Year, jadwal.Tanggal.Month, jadwal.Tanggal.Day,
                                         dtJamMulai.Hour, dtJamMulai.Minute, dtJamMulai.Second);

            var newAntrian = new BlAntrian
            {
                NomorRegistrasi  = "",
                JadwalId         = jadwal.Id,
                Tanggal          = jadwal.Tanggal,
                JamMulai         = jamMulai,             // HH:mm:ss, estimasi pelayanan
                KodeDokter       = jadwal.KodeDokter,
                CreatedBy        = appSource,
                EditedBy         = appSource,
                NomorAntrianInt  = antriNumber,
                TokenAntrian     = kodeBooking,
                JamPraktek       = jamPraktek,
                KodePoli         = jadwal.KodePoli,
                EstimasiDilayani = ((DateTimeOffset)datetimeJanji).ToUnixTimeMilliseconds(),
                NamaHari         = jadwal.NamaHari,
                StatusAntri      = StatusAntri.BOOKED,
                NomorRujukan     = "",                  // Hanya untuk FKTP
                Keterangan       = ""
            };

            return newAntrian;
        }

        // TODO_JEFRI
        public async Task<CreateQueueResultData> AmbilAntrianFarmasi(CreateQueueRequestData request)
        {
            _logger.LogDebug($"  MjknService: AmbilAntrianFarmasi Kode booking: {request.BookingCode.Trim()}");

            var dataCtx = _sqlCtx;

            var antrian = await GetNomorAntrianFarmasi(request.BookingCode.Trim());
            if (antrian == null)
                return null;

            return new CreateQueueResultData
            {
                QueueNumber = antrian.NomorAntri,
                PrefixCode  = antrian.PrefixCode,
                PostCode    = "FARMASI",
                StartTime   = antrian.StartTime,
                JenisResep  = antrian.JenisResep
            };
        }

        // TODO_JEFRI
        public async Task<ResponseStatusAntrianFarmasi> StatusAntrianFarmasi(StatusAntrianRequest request)
        {
            var dataCtx = _sqlCtx;

            var antrian = await GetNomorAntrianFarmasi(request.KodeBooking);
            if (antrian == null)
                return null;

            ResponseStatusAntrianFarmasi resultStatus = new ResponseStatusAntrianFarmasi();

            string sqlTotalAntrian =  
              @"SELECT ";

            string sqlSisaAntrian =
              @"SELECT ";

            string sqlSudahDilayani =
              @"SELECT ";


            try
            {
                //var tanggalResep = DateTime.Now;
                var tanggalResep = DateTime.ParseExact(antrian.TanggalResep, "yyyy-MM-dd HH:mm:ss", null);

                // QuotaTotal Antrian
                using (DbCommand cmdSelect = dataCtx.Connection().CreateCommand())
                {
                    cmdSelect.CommandText = sqlTotalAntrian;
                    dataCtx.AddParameter(cmdSelect, "tgl", tanggalResep, DbType.Date);
                    using (DbDataReader reader = await cmdSelect.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            var jumlahResep = reader.IsDBNull(1) ? 0  : reader.GetInt32(1);
                            resultStatus.TotalAntrean = jumlahResep.ToString();
                        }
                    }
                }

                // Sisa Antrian
                using (DbCommand cmdSelect = dataCtx.Connection().CreateCommand())
                {
                    cmdSelect.CommandText = sqlSisaAntrian;
                    dataCtx.AddParameter(cmdSelect, "tgl", tanggalResep, DbType.Date);
                    using (DbDataReader reader = await cmdSelect.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            var jumlahResep = reader.IsDBNull(1) ? 0  : reader.GetInt32(1);
                            resultStatus.SisaAntrean = jumlahResep.ToString();
                        }
                    }
                }

                // Sudah Dilayani
                using (DbCommand cmdSelect = dataCtx.Connection().CreateCommand())
                {
                    cmdSelect.CommandText = sqlSudahDilayani;
                    dataCtx.AddParameter(cmdSelect, "tgl", tanggalResep, DbType.Date);
                    using (DbDataReader reader = await cmdSelect.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();
                            var endResep    = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            resultStatus.AntreanPanggil = endResep;
                        }
                    }
                }

                resultStatus.JenisResep = antrian.JenisResep;

                return resultStatus;
            }
            catch(Exception ex)
            {
                _logger.LogError($"  MjknService: StatusAntrianFarmasi Gagal mengambil status antrian farmasi, Kode booking: {request.KodeBooking.Trim()}");
                _logger.LogError(ex.Message);
                
                throw new AppException("Gagal mengambil status antrian farmasi");
            }
        }

        // TODO_JEFRI
        private async Task<ResultGetAntrianFarmasi> GetNomorAntrianFarmasi(string kodeBooking)
        {
            var dataCtx = _sqlCtx;

            try
            { 
                using (DbCommand cmdSelect = dataCtx.Connection().CreateCommand())
                {
                    cmdSelect.CommandText =
                        @"SELECT 
                          WHERE mjkn_token = @token";
                
                    dataCtx.AddParameter(cmdSelect, "token", kodeBooking.Trim(), DbType.String);
                
                    using (DbDataReader reader = await cmdSelect.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            // Nomor urut antri dengan kode (misal A-0001)
                            var nomorAntrian  = reader.IsDBNull(1) ? "" : reader.GetString(1).Trim();
                            var noreg         = reader.IsDBNull(2) ? "" : reader.GetString(2).Trim();
                            var tglResep      = reader.IsDBNull(3) ? "" : reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss");
                            // Nomor urut antri (integer)
                            var nomorAntriInt = reader.IsDBNull(4) ? 0  : (int)reader.GetDecimal(4);
                            var statusAntriI  = reader.IsDBNull(5) ? 0  : (int)reader.GetInt32(5);
                            var jamResep      = reader.IsDBNull(6) ? "" : reader.GetString(6).ToString();

                            string statusAntri = "NA";

                            if (statusAntriI == 0)
                                statusAntri = "NA";
                            else if(statusAntriI == 1)
                                statusAntri = "NA";
                            else if(statusAntriI == 2)
                                statusAntri = "WAIT";
                            else if(statusAntriI == 3)   
                                statusAntri = "NA";
                            else if(statusAntriI == 4)
                                statusAntri = "FINISHED";


                            string prefixCode = "";
                            string jenisResep = "";
                            if (nomorAntrian.StartsWith("A"))
                            {
                                jenisResep = "Non Racikan";
                                prefixCode = "A";
                            }
                            if (nomorAntrian.StartsWith("B"))
                            {
                                jenisResep = "Racikan";
                                prefixCode = "B";
                            }

                            _logger.LogDebug($"  MjknService: GetNomorAntrianFarmasi Kode booking: {kodeBooking.Trim()} , Nomor Antrian: {nomorAntrian}, NomorAntriInt: {nomorAntriInt}");

                            DateTime dtJamResep     = (DateTime)Utils.DateFromStringTimeWithSeconds(jamResep);
                            DateTime dtTanggalResep = DateTime.ParseExact(tglResep, "yyyy-MM-dd HH:mm:ss", null);

                            return new ResultGetAntrianFarmasi
                            {
                                Noreg        = noreg,
                                TanggalResep = tglResep,
                                JenisResep   = jenisResep,
                                NomorAntrian = nomorAntrian,
                                NomorAntri   = nomorAntriInt, 
                                StatusAntri  = statusAntri,
                                PrefixCode   = prefixCode,
                                StartTime    = dtTanggalResep.ToString("yyyy-MM-dd") + " " + dtJamResep.ToString("HH:mm:ss")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: GetNomorAntrianFarmasi Gagal mengambil status antrian farmasi, Kode booking: {kodeBooking.Trim()}");
                _logger.LogError(ex.Message);

                throw new AppException("Gagal mengambil nomor antrian farmasi");
            }

            return null;
        }
    
        private async Task<bool> UpdateAntrianStatusHistory(BlAntrian antrian, string keterangan)
        {
            try
            {
                var antrianHistory = new BlAntrianStatusHistory
                {
                    AntrianId = antrian.Id,
                    Status    = StatusAntri.GetStatusStr(antrian.StatusAntri),
                    ChangedAt = DateTime.Now,
                    ChangedBy = antrian.EditedBy,
                    Note      = keterangan ?? string.Empty
                };
                _sqlCtx.BlAntrianStatusHistory.Add(antrianHistory);
                await _sqlCtx.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: UpdateAntrianStatusHistory, Gagal menyimpan status antrian: {ex.Message}");
                return false;
            }
        }

    }
}