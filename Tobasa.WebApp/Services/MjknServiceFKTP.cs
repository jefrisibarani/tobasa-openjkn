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

using Azure.Core;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tobasa.App;
using Tobasa.Data;
using Tobasa.Entities;
using Tobasa.Models.Jkn;
using Tobasa.Models.Mjkn;
using Tobasa.Models.Queue;
using Tobasa.Models.SimrsAntrian;
using Tobasa.Models.Vclaim;
using Tobasa.Services.Vclaim;


namespace Tobasa.Services
{
    public interface IMjknServiceFKTP
    {
        Task<BlPasien> GetPasien(string nomorRM);
        Task<string> CreateNomorRekamMedis();
        Task<string> GetNomorRekamMedis(string nomorJKN);
        Task<string> GetNomorRekamMedis(string nomorNIK, string nomorJKN);
        Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN, string nomorRM);
        Task<bool> CheckPasienExisting(string nomorNIK, string nomorJKN);
        Task<bool> CheckPasienExistingByMRN(string nomorRM);
        Task<bool> CheckPasienExistingByJKN(string nomorJKN);

        Task<string> GetKodePoliJKN(string kodeSubSpesialisJKN);
        Task<string> GetKodePoliINT(string kdSubPoliJKN);
        Task<bool> CekKodePoliINT(string kodePoliINT);
        Task<bool> CekKodeDokterINT(string kodeDokterINT);
        Task<string> GetKodeSubPoliJKN(string kodePoliINT);
        Task<int> GetKodeDokterJKN(string kodeDokterINT);
        Task<string> GetNamaDokterJKN(int kdDokterJKN);
        Task<string> GetKodeDokterINT(int kdDokterJKN);
        Task<string> GetNamaPosPoliINT(string kdSubPoliJKN);
        Task<string> GetNamaPoliINT(string kodePoliINT);
        Task<string> GetNamaSubPoliJKN(string kdSubPoliJKN);
        Task<ViewPoli> GetSubPoli(int kdDokterJKN);

        // --------------------------------------------------------------------
        //
        // --------------------------------------------------------------------
        Task<JadwalDokterView> GetJadwalDokter(string kodePoliINT, string tglPraktek, string kodeDokterINT, string jamMulai, string jamSelesai);
        Task<List<BlJadwal>> GetJadwalDokterByKodePoliAndTanggal(string kodePoliINT, string tglPraktek);
        Task<bool> CheckJadwalPoliTutup(string tanggalPeriksa, int kodeDokterJKN, string kdSubPoliJKN, string jamPraktek);
        Task<AntrianSummary> GetAntrianSummary(long jadwalId);
        Task<InfoAntrian> GetInfoAntrian(string nomorKartuJKN, string kodePoliINT, string tanggalPeriksa);
        Task<bool> SaveAntrianTransaction(DataAmbilAntrianFKTP request, BlAntrian blAntrian);


        // --------------------------------------------------------------------
        // Services terkait untuk diakses BPJS
        // --------------------------------------------------------------------
        Task<ResultAmbilAntrianFKTP> GetAppointment(DataAmbilAntrianFKTP request, long jadwalID);
        Task<ResponseSisaAntrianFKTP> GetSisaAntrian(RequestSisaAntrianFKTP request);
        Task<ResponseStatusAntrianFKTP> GetStatusAntrian(RequestStatusAntrianFKTP request);
        Task<string> BatalAntrian(RequestBatalAntrianFKTP request);
        Task<bool> CreatePasienBaru(RequestPasienBaruFKTP request);


        // --------------------------------------------------------------------
        // Services terkait WS disisi BPJS
        // --------------------------------------------------------------------
        // Ambil jadwal dokter dari HFIS, berdasarkan * KODE POLI BPJS * ( BUKAN Kode SubPoli)
        Task<List<MjknJadwalDokterHfis>> GetJadwalDokterHFIS(string kodePoliJKN, string tanggal);
        Task<bool> CreateJadwalPraktekDariHFIS(MjknJadwalDokter jadwalHFIS, string tanggalPeriksa);
        Task<bool> CreateJadwalPraktekDariHFIS(string tglPraktek, string kodePoliJKN, int kodeDokterJKN, string jamMulai, string jamSelesai);
    }

    public class MjknServiceFKTP : IMjknServiceFKTP
    {
        private DataContextAntrian      _sqlCtx;
        private readonly AppSettings    _appSettings;
        private readonly ILogger        _logger;
        private readonly IConfiguration _configuration;

        public MjknServiceFKTP(DataContextAntrian context,
            IOptions<AppSettings> appSettings,
            ILogger<MjknService> logger,
            IConfiguration configuration)
        {
            _sqlCtx         = context;
            _appSettings    = appSettings.Value;
            _logger         = logger;
            _configuration  = configuration;
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
            if (found) {
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
            if (totalFound == 0) {
                return false;
            }
            else if (totalFound == 1) {
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
            if (totalFound == 0) {
                return false;
            }
            else if (totalFound == 1) {
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
            if (totalFound == 0) {
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
            if (totalFound == 0) {
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

        public async Task<bool> CekKodePoliINT(string kodePoliFKTP)
        {
            var found = await _sqlCtx.BlPoli.CountAsync(x => x.KodePoli == kodePoliFKTP);
            if (found == 1) {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CekKodePoliINT, Poli Kode: {kodePoliFKTP} ditemukan: {found}");
                return false;
            }
        }

        public async Task<bool> CekKodeDokterINT(string kodeDokterFKTP)
        {
            var found = await _sqlCtx.BlDokter.CountAsync(x => x.KodeDokter == kodeDokterFKTP);
            if (found == 1) {
                return true;
            }
            else
            {
                _logger.LogError($"  MjknService: CekKodeDokterINT, Dokter Kode: {kodeDokterFKTP} ditemukan: {found}");
                return false;
            }
        }

        public async Task<string> GetKodePoliJKN(string kdSubPoliJKN)
        {
            var found = await _sqlCtx.MjknPoliSub.AnyAsync(x => x.Kode == kdSubPoliJKN);
            if (!found) {
                return "ERROR_POLI_NOT_FOUND";
            }

            var qryPoli = from poJkn in _sqlCtx.MjknPoliSub
                          where poJkn.Kode == kdSubPoliJKN
                          select new { poJkn.KodePoli };

            var totalFound = await qryPoli.CountAsync();
            if (totalFound > 1)
            {
                _logger.LogError($"  MjknService: GetKodePoliJKN, Ditemukan lebih dari satu sub poli dengan Kode: {kdSubPoliJKN}");
                return "ERROR_POLI_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var poli = await qryPoli.SingleAsync();
                if (string.IsNullOrWhiteSpace(poli.KodePoli))
                {
                    _logger.LogError($"  MjknService: GetKodePoliJKN, Data sub poli dengan Kode: {kdSubPoliJKN} null/whitespace");
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
            if (!found) {
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

        public async Task<string> GetKodeSubPoliJKN(string kodePoliFKTP)
        {
            if (!await CekKodePoliINT(kodePoliFKTP))
                return "ERROR_POLI_NOT_FOUND";

            var qryPoli = from po in _sqlCtx.BlPoli
                          join poJkn in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals poJkn.Kode
                          where po.KodePoli == kodePoliFKTP
                          select new { poJkn.Kode };

            var totalFound = await qryPoli.CountAsync();
            if (totalFound > 1)
            {
                _logger.LogError($"  MjknService: GetKodeSubPoliJKN, Ditemukan lebih dari satu poli dengan Kode: {kodePoliFKTP}");
                return "ERROR_POLI_NOT_FOUND";
            }
            else if (totalFound == 1)
            {
                var poli = await qryPoli.SingleAsync();
                if (string.IsNullOrWhiteSpace(poli.Kode))
                {
                    _logger.LogError($"  MjknService: GetKodeSubPoliJKN, Poli dengan Kode: {kodePoliFKTP} null");
                    return "ERROR_POLI_NOT_FOUND";
                }
                else
                    return poli.Kode.Trim();
            }

            return "ERROR_POLI_NOT_FOUND";
        }

        public async Task<int> GetKodeDokterJKN(string kodeDokterFKTP)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterInternal == kodeDokterFKTP);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterInternal == kodeDokterFKTP);
                return dokter.KodedokterJkn;
            }
            else if (found == 0)
            {
                _logger.LogError($"  MjknService: GetKodeDokterJKN, Dokter Kode: {kodeDokterFKTP} tidak ditemukan dalam database");
                return -1; // Dokter tidak ditemukan
            }
            else
            {
                _logger.LogError($"  MjknService: GetKodeDokterJKN, Kode dokter {kodeDokterFKTP} dimapping lebih dari sekali");
                return -2; // Dokter ditemukan lebih dari sekali
            }
        }

        public async Task<string> GetNamaDokterJKN(int kdDokterJkn)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterJkn == kdDokterJkn);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterJkn == kdDokterJkn);
                return dokter.NamaDokter.Trim();
            }
            else if (found == 0)
            {
                _logger.LogError("  MjknService: GetNamaDokterJKN, Dokter Kode: " + kdDokterJkn.ToString() + " tidak ditemukan");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
            else
            {
                _logger.LogError($"  MjknService: GetNamaDokterJKN, Dokter kode: {kdDokterJkn} dimapping lebih dari sekali");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
        }

        public async Task<string> GetKodeDokterINT(int kdDokterJkn)
        {
            var found = await _sqlCtx.MjknDokter.CountAsync(x => x.KodedokterJkn == kdDokterJkn);
            if (found == 1)
            {
                var dokter = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterJkn == kdDokterJkn);
                if (string.IsNullOrWhiteSpace(dokter.KodedokterInternal))
                {
                    _logger.LogError("  MjknService: GetKodeDokterINT, Dokter Kode: " + kdDokterJkn.ToString() + ", Nama: " + dokter.NamaDokter.Trim() + " belum dimapping ke dokter internal");
                    return "ERROR_DOKTER_NOT_FOUND_SIMRS";
                }
                else
                    return dokter.KodedokterInternal.Trim();
            }
            else if (found == 0)
            {
                _logger.LogError("  MjknService: GetKodeDokterINT, Dokter Kode: " + kdDokterJkn.ToString() + " tidak ditemukan");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
            else
            {
                _logger.LogError($"  MjknService: GetKodeDokterINT, Kode dokter {kdDokterJkn} dimapping lebih dari sekali");
                return "ERROR_DOKTER_NOT_FOUND_JKN";
            }
        }

        public async Task<string> GetNamaPoliINT(string kodePoliFKTP)
        {
            var found = await _sqlCtx.BlPoli.CountAsync(x => x.KodePoli == kodePoliFKTP);
            if (found == 1)
            {
                var poli = await _sqlCtx.BlPoli.SingleAsync(x => x.KodePoli == kodePoliFKTP);
                return poli.NamaPoli.Trim();
            }
            else
            {
                _logger.LogError($"  MjknService: GetNamaPoliINT Kode: {kodePoliFKTP} ditemukan: {found}");
                return "ERROR_POLI_NOT_FOUND";
            }
        }

        public async Task<string> GetNamaPosPoliINT(string kdSubPoliJKN)
        {
            var exists = await _sqlCtx.MjknPoliSub.AnyAsync(x => x.Kode == kdSubPoliJKN);
            if (!exists) 
            {
                _logger.LogError($"  MjknService: GetNamaPosPoliINT Kode sub spesialis JKN: {kdSubPoliJKN} tidak ditemukan");
                return "ERROR_POLI_NOT_FOUND";
            }

            // cek apakah kode sub spesialis JKN ini ada di mapping ke poli FKTP
            var qryPoli = from po in _sqlCtx.BlPoli
                          join poJkn in _sqlCtx.MjknPoliSub on po.KodePoliJkn equals poJkn.Kode
                          where poJkn.Kode == kdSubPoliJKN
                          select new { po.NamaPoli };

            var  found = await qryPoli.CountAsync();
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

        public async Task<ViewPoli> GetSubPoli(int kdDokterJKN)
        {
            try
            {
                var qryPoli = from d in _sqlCtx.BlDokter
                              join dj in _sqlCtx.MjknDokter on d.KodeDokterJkn equals dj.KodedokterJkn
                              join p in _sqlCtx.BlPoli on d.KodePoli equals p.KodePoli
                              join spj in _sqlCtx.MjknPoliSub on p.KodePoliJkn equals spj.Kode
                              join pj in _sqlCtx.MjknPoli on spj.KodePoli equals pj.Kode
                              where d.KodeDokterJkn == kdDokterJKN
                              select new ViewPoli
                              {
                                KodePoliJKN    = pj.Kode,
                                NamaPoliJKN    = pj.Nama,
                                KodeSubPoliJKN = spj.Kode,
                                NamSubPoliJKN  = spj.Nama,
                                KodePoliINT    = p.KodePoli,
                                NamaPoliINT    = p.NamaPoli
                              };

                var vPoli = await qryPoli.SingleAsync();
                return vPoli;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // --------------------------------------------------------------------
        //
        // --------------------------------------------------------------------

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

        public async Task<List<BlJadwal>> GetJadwalDokterByKodePoliAndTanggal(string kodePoliFKTP, string tglPraktek)
        {
            _logger.LogDebug($"  MjknService: GetJadwalDokterByKodePoliAndTanggal Poli: {kodePoliFKTP}, Tanggal: {tglPraktek}");

            // NOTE: tglPraktek must in format: yyyy-MM-dd (10 character)
            var tanggalPraktek = DateTime.ParseExact(tglPraktek, "yyyy-MM-dd", null);
            var qryJadwal = from j in _sqlCtx.BlJadwal
                            join p in _sqlCtx.BlPoli   on j.KodePoli   equals p.KodePoli
                            join d in _sqlCtx.BlDokter on j.KodeDokter equals d.KodeDokter
                            where j.KodePoli == kodePoliFKTP
                               && j.Tanggal  == DateOnly.FromDateTime(tanggalPraktek.Date)
                            orderby j.Id descending
                            select j;

            var listJadwal = await qryJadwal.ToListAsync();
            return listJadwal;
        }

        public async Task<bool> CheckJadwalPoliTutup(string tanggalPeriksa, int kodeDokterJKN, string kdSubPoliJKN, string jamPraktek)
        {
            if (!Utils.IsValidDate(tanggalPeriksa))
                return false;

            try
            {
                string namaHari = Utils.ISODateTimeToIndonesianDay(tanggalPeriksa).ToUpper();

                var sch = await _sqlCtx.MjknJadwalDokter.SingleAsync(x =>
                                           x.KodedokterJkn              == kodeDokterJKN
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
                    var lastServedPoliCode = await _sqlCtx.BlPoli.Where(x => x.KodePoli == antriLastServed.KodePoli).Select(s => s.KodePoli).SingleAsync();

                    lastServedAntrianNo = lastServedPoliCode.Trim() + "-" + Convert.ToString(antriLastServedNum);
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

        public async Task<InfoAntrian> GetInfoAntrian(string nomorKartuJKN, string kodePoliINT, string tanggalPeriksa)
        {
            // NOTE: tglPraktek must in format: yyyy-MM-dd (10 character)
            var tanggalPraktek = DateTime.ParseExact(tanggalPeriksa, "yyyy-MM-dd", null);

            var qryInfo = from j in _sqlCtx.BlJadwal
                          join p in _sqlCtx.BlPoli    on j.KodePoli equals p.KodePoli
                          join a in _sqlCtx.BlAntrian on j.Id       equals a.JadwalId
                          where j.KodePoli      == kodePoliINT
                             && j.Tanggal       == DateOnly.FromDateTime(tanggalPraktek)
                             && a.NomorKartuJkn == nomorKartuJKN
                          select new InfoAntrian
                          {
                              JadwalId         = j.Id,
                              AntrianId        = a.Id,
                              NomorAntrian     = a.NomorAntrian,
                              NomorKartuJKN    = a.NomorKartuJkn,
                              NamaPoliInternal = p.NamaPoli,
                              StatusAntrian    = a.StatusAntri,
                          };

            InfoAntrian info = null;
            try
            {
                info = await qryInfo.SingleAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"  MjknService: GetInfoAntrian 5108, {ex.Message}");
            }
            return info;
        }
        
        public async Task<bool> SaveAntrianTransaction(DataAmbilAntrianFKTP request, BlAntrian blAntrian)
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


        // --------------------------------------------------------------------
        // Services terkait untuk diakses BPJS
        // --------------------------------------------------------------------
        public async Task<ResultAmbilAntrianFKTP> GetAppointment(DataAmbilAntrianFKTP request, long jadwalID)
        {
            if (request.JenisPasien == Insurance.BPJS)
                _logger.LogDebug("  MjknService: GetAppointment, Ambil antrian baru untuk peserta No.BPJS: " + request.NomorKartu);
            else
                _logger.LogDebug("  MjknService: GetAppointment, Ambil antrian baru untuk pasien No.RM: " + request.NomorRekamMedis);

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
                                            //&& x.NomorRujukan  == request.NomorRujukan // only for FKTRL
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

                if (rMessage == "SUCCESS_EXISTS")
                {
                    if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
                    {
                        await SaveAntrianTransaction(request, reservasi);
                    }

                    var antrianSummary = await GetAntrianSummary(jadwalID);
                    var result = new ResultAmbilAntrianFKTP
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
                    if (totalAntri > 0)
                    {
                        var dokter = await _sqlCtx.BlDokter.SingleAsync(x => x.KodeDokter == jadwal.KodeDokter);
                        int interval = dokter.PasienTime;

                        // sudah ada antrian/reservasi, cari nomor antrian terakhir
                        var lastNumber = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalID).Select(s => s.NomorAntrianInt).MaxAsync();
                        var lastAntri = await _sqlCtx.BlAntrian.Where(x => x.JadwalId == jadwalID && x.NomorAntrianInt == lastNumber).SingleAsync();

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
                    newAntrian.BookedAt        = DateTime.Now;          // waktu reservasi dibuat
                    //newAntrian.EditedBy      = request.AppSource;
                    newAntrian.InsuranceId     = request.JenisPasien;
                    newAntrian.TanggalLahir    = pasien.TanggalLahir;
                    newAntrian.NomorKartuJkn   = request.NomorKartu;
                    newAntrian.NomorRujukan    = request.NomorReferensi; // untuk FKTP, tidak dipakai
                    newAntrian.NomorAntrian    = jadwal.KodePoli + "-" + newNumber;
                    newAntrian.SisaKuotaJkn    = sisaQuotaJkn;
                    newAntrian.SisaKuotaNonJkn = sisaQuotaNonJkn;
                    newAntrian.KuotaJkn        = jadwal.QuotaJkn;
                    newAntrian.KuotaNonJkn     = jadwal.QuotaNonJkn;
                    newAntrian.JenisKunjungan  = request.JenisKunjungan; // untuk FKTP, tidak dipakai

                    // NOTE_JEFRI: untuk FKTP,Keterangan berisi keluhan pasien
                    newAntrian.Keterangan      = request.Keluhan;
                    // NOTE_JEFRI: test only
                    SaveToAntrianNote("Keluhan", request.Keluhan, newAntrian);  

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
                    
                    // Note_JEFRI: untuk FKTRL tidak ada keluhan, hanya untuk FKTP
                    await UpdateAntrianStatusHistory(newAntrianUpdated, request.Keluhan);

                    // Save transaction history
                    if (_configuration["AppSettings:DisableReservationTransactionHistory"] != true.ToString())
                    {
                        await SaveAntrianTransaction(request, newAntrianUpdated);
                    }

                    // get updated antrian summary so we can get latest stats of jadwal
                    antrianSummary = await GetAntrianSummary(jadwalID);
                    // return the result
                    var result = new ResultAmbilAntrianFKTP
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
                //_logger.LogDebug("  MjknService: GetAppointment, " + ex.ToString());
                _logger.LogError("  MjknService: GetAppointment, " + ex.Message);
                throw;
            }
        }

        public async Task<ResponseSisaAntrianFKTP> GetSisaAntrian(RequestSisaAntrianFKTP request)
        {
            _logger.LogDebug($"  MjknService: GetSisaAntrian, No.JKN: {request.NomorKartuJKN}, Poli: {request.KodePoliJKN}, Tanggal: {request.TanggalPeriksa}");

            InfoAntrian info = await GetInfoAntrian(request.NomorKartuJKN, request.KodePoliInternal, request.TanggalPeriksa);
            if (info != null)
            {
                var antrianSummary = await GetAntrianSummary(info.JadwalId);
                var response = new ResponseSisaAntrianFKTP
                {
                    NomorAntrean    = info.NomorAntrian,
                    NamaPoli        = info.NamaPoliInternal,
                    SisaAntrean     = antrianSummary.SisaAntrianJkn,
                    AntreanPanggil  = antrianSummary.LastServedAntrian,
                    Keterangan      = "",
                };
                return response;
            }
            
            return null;
        }

        public async Task<ResponseStatusAntrianFKTP> GetStatusAntrian(RequestStatusAntrianFKTP request)
        {
            var listJadwal = await GetJadwalDokterByKodePoliAndTanggal(request.KodePoliInternal, request.TanggalPeriksa );
            if (listJadwal == null || listJadwal.Count == 0)
            {
                _logger.LogInformation($"  MjknService: GetStatusAntrian 5109, Tidak ada jadwal dokter untuk Kode Poli: {request.KodePoliInternal} dan Tanggal: {request.TanggalPeriksa}");
                return new ResponseStatusAntrianFKTP
                {
                    StatusList = new List<StatusAntrianFKTP>()
                };
            }

            var responseList = new List<StatusAntrianFKTP>();
            foreach (var jadwal in listJadwal)
            {
                var antrianSummary = await GetAntrianSummary(jadwal.Id);
                if (antrianSummary != null)
                {
                    var dokterJkn = await _sqlCtx.MjknDokter.SingleAsync(x => x.KodedokterInternal == jadwal.KodeDokter);

                    responseList.Add(new StatusAntrianFKTP
                    {
                        NamaPoli       = await GetNamaPoliINT(jadwal.KodePoli),
                        TotalAntrean   = antrianSummary.Totalantrian,
                        SisaAntrean    = antrianSummary.SisaAntrianJkn + antrianSummary.SisaAntrianNonJkn,
                        AntreanPanggil = antrianSummary.LastServedAntrian,
                        Keterangan     = "",
                        KodeDokter     = dokterJkn.KodedokterJkn,
                        NamaDokter     = dokterJkn.NamaDokter,
                        JamPraktek     = jadwal.JamMulai.Trim() + "-" + jadwal.JamSelesai.Trim(),
                    });

                }
            }

            var response = new ResponseStatusAntrianFKTP();
            response.StatusList = responseList;

            return response;
        }

        public async Task<string> BatalAntrian(RequestBatalAntrianFKTP request)
        {
            InfoAntrian info = await GetInfoAntrian(request.NomorKartu, request.KodePoliInternal, request.TanggalPeriksa);
            if (info == null)
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110, Antrian tidak ditemukan untuk No.BPJS: {request.NomorKartu}, Poli: {request.KodePoliInternal}, Tanggal: {request.TanggalPeriksa}");
                throw new AppException("Antrian tidak ditemukan");
            }

            if (info.StatusAntrian == StatusAntri.CANCELLED)
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110A, Antrian sudah dibatalkan sebelumnya");
                throw new AppException("Antrian sudah dibatalkan sebelumnya");
            }
            if (info.StatusAntrian == StatusAntri.SERVED )
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110B, Antrian sudah dilayani, tidak bisa dibatalkan");
                throw new AppException("Antrian sudah dilayani, tidak bisa dibatalkan");
            }

            if (info.StatusAntrian == StatusAntri.CHECKED_IN)
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110C, Antrian sudah check in, tidak bisa dibatalkan");
                throw new AppException("Antrian sudah check in, tidak bisa dibatalkan");
            }

            if (info.StatusAntrian == StatusAntri.NOSHOW)
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110D, Antrian No Show");
                throw new AppException("Antrian Tidak datang, tidak bisa dibatalkan");
            }


            var jadwal = await _sqlCtx.BlJadwal.SingleAsync(x => x.Id == info.JadwalId);
            // jamMulai and jamAkhir is current date
            DateTime jamMulai = new DateTime();
            DateTime jamAkhir = new DateTime();
            try
            {
                // Note: 24 hour format
                jamMulai = DateTime.ParseExact(jadwal.JamMulai,   "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                jamAkhir = DateTime.ParseExact(jadwal.JamSelesai, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
            }
            catch (Exception)
            {
                _logger.LogError("  MjknService: BatalAntrian Gagal mengkonversi data jam mulai/akhir");
                return "Data jam mulai/selesai jadwal tidak dapat dikonversi";
            }

            if (DateTime.Now > jamAkhir)
            {
                _logger.LogError($"  MjknService: BatalAntrian 5110E, Antrian tidak dapat dibatalkan, karena sudah lewat jadwal");
                throw new AppException("Antrian tidak dapat dibatalkan, karena sudah lewat jadwal");
            }

            var antrian = await _sqlCtx.BlAntrian.SingleAsync(x => x.Id == info.AntrianId);
            antrian.StatusAntri = StatusAntri.CANCELLED;
            antrian.CancelledAt = DateTime.Now;

            _sqlCtx.BlAntrian.Update(antrian);
            _sqlCtx.SaveChanges();

            await UpdateAntrianStatusHistory(antrian, request.Keterangan);

            return "SUCCESS";
        }

        public async Task<bool> CreatePasienBaru(RequestPasienBaruFKTP request)
        {
            DateTime dtTglLahir = DateTime.ParseExact(request.TanggalLahir, "yyyy-MM-dd", null);
            if (dtTglLahir.Date > DateTime.Now.Date) {
                throw new AppException("Tanggal lahir tidak valid");
            }

            if (await CheckPasienExistingByJKN(request.NomorKartu))
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Nomor peserta sudah ada pada PASIEN");
                throw new AppException("Data nomor peserta sudah pernah dientrikan");
            }

            var found = await _sqlCtx.BlPasien.AnyAsync(x => x.NomorIdentitas == request.Nik);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru, nomor NIK sudah ada pada PASIEN");
                throw new AppException("Data NIK peserta sudah pernah dientrikan");
            }

            found = await _sqlCtx.MjknPasien.AnyAsync(x => x.Nik == request.Nik);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Nomor peserta sudah ada pada MjknPasien");
                throw new AppException("Internal error: 5111");
            }

            found = await _sqlCtx.MjknPasien.AnyAsync(x => x.NomorKartu == request.NomorKartu);
            if (found)
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Nomor NIK sudah ada pada MjknPasien");
                throw new AppException("Internal error: 5111A");
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

            if (string.IsNullOrWhiteSpace(nomorRm)) 
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Gagal mendapat nomor rekam medis baru");
                throw new AppException("Internal error: 5111B");
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
                NomorHp         = "",
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
                Phone              = "",
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

                return true;
            }
            catch (Exception)
            {
                _logger.LogError("  MjknService: CreatePasienBaru, Gagal waktu menyimpan ke database");
                throw new AppException("Internal error: 5111C");
            }
        }


        // --------------------------------------------------------------------
        // Services terkait WS disisi BPJS
        // --------------------------------------------------------------------

        // Ambil jadwal dokter dari HFIS, berdasarkan * KODE POLI BPJS * ( BUKAN Kode Sub Spesialis)
        public async Task<List<MjknJadwalDokterHfis>> GetJadwalDokterHFIS(string kodePoliJKN, string tanggal)
        {
            _logger.LogDebug("  MjknService: GetReferensiDokter");
            VclaimMjknServiceFKTP vclaimMjknService = new VclaimMjknServiceFKTP(_appSettings, _logger, _configuration);
            var listDokter = await vclaimMjknService.GetReferensiDokter(kodePoliJKN, tanggal);
            var jadwalHfis = await MjknRefDokterFKTP.ToMjknJadwalDokterHfis(listDokter, kodePoliJKN, tanggal, this);

            if (jadwalHfis == null){
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

            //if (_configuration["AppSettings:HanyaBolehAdaSatuJadwalPadaBlJadwal"] == true.ToString())
            //{
            //    // Periksa apakah terjadi perubahan jadwal(jam awal,jam akhir) bila berubah, update jam praktek dengan jadwal baru
            //    await CleanJadwalLocalSehari(kodePoliINT, tanggalPeriksa, kodeDokterINT, jadwalHFIS.JamMulai, jadwalHFIS.JamTutup);
            //}

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
            MjknJadwalDokter jadwalJKN = null;

            try
            {
                string namaHari = Utils.ISODateTimeToIndonesianDay(tglPraktek).ToUpper();

                // Update data jadwal dokter MJKN
                jadwalJKN = await _sqlCtx.MjknJadwalDokter.SingleAsync(x =>
                                                       x.KodedokterJkn             == kodeDokterJKN
                                                    && x.NamaHari.Trim().ToUpper() == namaHari
                                                    && x.JamMulai                  == jamMulai
                                                    && x.JamTutup                  == jamSelesai
                                                    && x.KodeSubspesialis          == kodeSubPoliJKN
                                                );

                return await CreateJadwalPraktekDariHFIS(jadwalJKN, tglPraktek);
            }
            catch (InvalidOperationException)
            {
                _logger.LogError($"  MjknService: CreateJadwalPraktekDariHFIS Jadwal tidak ada, Tanggal: {tglPraktek}, Sub Poli BPJS: {kodeSubPoliJKN}, Dokter BPJS: {kodeDokterJKN.ToString()}, Jam: {jamMulai}-{jamSelesai}");
            }
            return false;
        }


        // --------------------------------------------------------------------
        // Private methods  
        // --------------------------------------------------------------------
        private BlAntrian CreateAntrian(BlJadwal jadwal, string appSource, DateTime dtJamMulai, int antriNumber)
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
    
        private void SaveToAntrianNote(string objectName, string objectValue, BlAntrian antrian)
        {
            // Save to Note field in BlAntrian
            if (string.IsNullOrWhiteSpace(antrian.Note))
            {
                antrian.Note = "{}"; // set empty json object
            }
            var noteObj = JsonSerializer.Deserialize<Dictionary<string, string>>(antrian.Note);
            if (noteObj == null)
            {
                noteObj = new Dictionary<string, string>();
            }
            if (noteObj.ContainsKey(objectName))
            {
                noteObj[objectName] = objectValue;
            }
            else
            {
                noteObj.Add(objectName, objectValue);
            }
            antrian.Note = JsonSerializer.Serialize(noteObj);
        }
    }
}