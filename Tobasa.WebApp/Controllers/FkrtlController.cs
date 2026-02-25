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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tobasa.App;
using Tobasa.Models.Mjkn;
using Tobasa.Services;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using Tobasa.Models.Queue;

namespace Tobasa.Controllers
{
    // /api/fkrtl
    [Route("api/[controller]")]
    [ApiController]
    public class FkrtlController : ControllerBase
    {
        private readonly IUserService   _userService;
        private readonly IMjknService   _jknService;
        private readonly AppSettings    _appSettings;
        private readonly IConfiguration _configuration;
        private readonly ILogger        _logger;

        public FkrtlController(
            IUserService          userService,
            IMjknService          jknService,
            IOptions<AppSettings> appSettings,
            IConfiguration        configuration,
            ILogger<MjknService>  logger)
        {
            _userService   = userService;
            _jknService    = jknService;
            _appSettings   = appSettings.Value;
            _configuration = configuration;
            _logger        = logger;
        }

        // TokenAntrian
        // GET: /api/fkrtl/token
        [AllowAnonymous]
        [HttpGet("token")]
        public IActionResult Token()
        {
            _logger.LogTrace("Menjalankan prosedur TokenAntrian");

            var userName = Request.Headers["x-username"].ToString();
            var password = Request.Headers["x-password"].ToString();

            // AppException and other Exceptions will be handled by ErrorHandlerMiddleWare

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password)) {
                return new ApiNegativeResult("Username atau password tidak valid");
            }

            var user = _userService.Authenticate(userName, password);
            if (user == null) {
                return new ApiNegativeResult("Username atau password tidak sesuai");
            }

            var tokenString = _userService.GenerateJwt(user.Id, user.UserName);
            return new ApiResult(new { Token = tokenString });
        }


        // StatusAntri Antrian
        // POST: /api/fkrtl/status_antrian
        [Authorize]
        [HttpPost("status_antrian")]
        public async Task<IActionResult> StatusAntrian([FromBody] RequestStatusAntrianFKRTL request)
        {
            // NOTE: request KodePoliInternal adalah Kode Subspesialis/Sub Poli BPJS, bukan kode Poli!
            // -------------------------------------------------------------------------------

            if (string.IsNullOrWhiteSpace(request.KodePoli)) 
            {
                _logger.LogError($"  StatusAntrian, kode sub spesialis tidak valid");
                return new ApiNegativeResult("Kode sub spesialis poli tidak valid");
            }

            string kodeSubPoliBPJS = request.KodePoli.Trim();

            _logger.LogInformation($"Menjalankan StatusAntrian {kodeSubPoliBPJS}, {request.KodeDokter}, {request.TanggalPeriksa}, {request.JamPraktek}");

            string kodeDokter = request.KodeDokter.ToString();

            if (string.IsNullOrWhiteSpace(kodeDokter))
                return new ApiNegativeResult("Kode dokter tidak valid");

            if (!Utils.IsValidDate(request.TanggalPeriksa))
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");

            DateTime dtPeriksa = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null).Date;
            var maxHari = _appSettings.MaximalJumlahHariPengambilanAntrian;
            if (dtPeriksa.Date < DateTime.Today.Date || dtPeriksa.Date > DateTime.Today.AddDays(maxHari).Date)
                return new ApiNegativeResult("Tanggal periksa tidak berlaku");

            if (string.IsNullOrWhiteSpace(request.JamPraktek))
                return new ApiNegativeResult("Jam praktek tidak valid");

            if (request.JamPraktek.Length != 11)
                return new ApiNegativeResult("Jam praktek tidak valid");

            string jamMulai   = request.JamPraktek.Substring(0, 5);
            string jamSelesai = request.JamPraktek.Substring(6, 5);

            string kodePoliINT = await _jknService.GetKodePoliINT(kodeSubPoliBPJS);
            if (kodePoliINT == "ERROR_POLI_NOT_FOUND")
                return new ApiNegativeResult("Poli tidak ada di Rumah Sakit");
            request.KodePoliINT = kodePoliINT;

            string kodeDokterINT = await _jknService.GetKodeDokterINT(request.KodeDokter);
            if (kodeDokterINT == "ERROR_DOKTER_NOT_FOUND" || kodeDokterINT == "ERROR_DOKTER_NOT_FOUND_JKN")
                return new ApiNegativeResult("Dokter tidak valid");
            request.KodeDokterINT = kodeDokterINT;

            if (_appSettings.AutoImportJadwalPraktekDokterDariDataHfis)
            {
                // Periksa dulu jadwal dokter di database MJKN HFIS, dan bila ada simpan ke database SIMMRS
                // ambil jadwal berdasarkan kode poli bpjs (bukan kode subspesialis bpjs)
                var kodePoliBPJS   = await _jknService.GetKodePoliJKN(kodeSubPoliBPJS);
                var listJadwalHfis = await _jknService.GetJadwalDokterHFIS(kodePoliBPJS, request.TanggalPeriksa);

                // Create jadwal praktek SIMRS dari data HFIS (menggunakan kode sub spesialis) - kodeSubPoliBPJS adalah kode sub spesialis BPJS
                await _jknService.CreateJadwalPraktekDariHFIS(request.TanggalPeriksa, kodeSubPoliBPJS, request.KodeDokter, jamMulai, jamSelesai);
            }

            var response = await _jknService.GetStatusAntrian(request);
            if (response != null)
            {
                _logger.LogDebug($"  StatusAntrian, {kodeSubPoliBPJS}, {request.KodeDokter}, {request.TanggalPeriksa}, {request.JamPraktek} sukses");
                return new ApiResult(response);
            }

            _logger.LogDebug($"  StatusAntrian, {kodeSubPoliBPJS}, {request.KodeDokter}, {request.TanggalPeriksa}, {request.JamPraktek}: data tidak tersedia");
            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Get Antrian
        // POST: /api/fkrtl/ambil_antrian
        [Authorize]
        [HttpPost("ambil_antrian")]
        public async Task<IActionResult> AmbilAntrian([FromBody] RequestAmbilAntrianFKRTL request)
        {
            // NOTE: request KodePoliInternal adalah Kode Subspesialis/Sub Poli BPJS, bukan kode Poli!
            // -------------------------------------------------------------------------------
            _logger.LogInformation("Menjalankan AmbilAntrian MJKN");
            _logger.LogDebug(JsonSerializer.Serialize(request));

            // Validasi Nomor Kartu BPJS
            if (string.IsNullOrWhiteSpace(request.NomorKartu))
            {
                _logger.LogError("AmbilAntrian, Nomor kartu BPJS belum diisi");
                return new ApiNegativeResult("Nomor kartu BPJS belum diisi");
            }
            if (request.NomorKartu.Length != 13 || !request.NomorKartu.All(char.IsNumber))
            {
                _logger.LogError("AmbilAntrian, Forma Nomor Kartu BPJS tidak valid");
                return new ApiNegativeResult("Nomor kartu BPJS harus berupa angka, dan 13 digit");
            }

            // Validasi Nomor NIK
            if (string.IsNullOrWhiteSpace(request.Nik))
            {
                _logger.LogError("AmbilAntrian, Nomor NIK belum diisi");
                return new ApiNegativeResult("Nomor NIK belum diisi");
            }
            if (request.Nik.Length != 16 || !request.Nik.All(char.IsNumber))
            {
                _logger.LogError("AmbilAntrian, Format NIK tidak valid");
                return new ApiNegativeResult("Nomor NIK harus berupa angka, dan 16 digit");
            }

            // Validasi Kode Poli
            if (string.IsNullOrWhiteSpace(request.KodePoli))
            {
                _logger.LogError("AmbilAntrian, Kode poli belum diisi");
                return new ApiNegativeResult("Kode poli belum diisi");
            }
            // konversi kode sub spesialis bpjs menjadi kodepoli INTernal Rumah Sakit
            string kodePoliINT = await _jknService.GetKodePoliINT(request.KodePoli);
            if (kodePoliINT == "ERROR_POLI_NOT_FOUND")
            {
                _logger.LogError("AmbilAntrian, Kode poli tidak tersedia");
                return new ApiNegativeResult("Poli tidak tersedia");
            }

            // Validasi Tanggal Periksa
            if (string.IsNullOrWhiteSpace(request.TanggalPeriksa))
            {
                _logger.LogError("AmbilAntrian, Tanggal periksa belum diisi");
                return new ApiNegativeResult("Tanggal periksa belum diisi");
            }
            if (!Utils.IsValidDate(request.TanggalPeriksa))
            {
                _logger.LogError("AmbilAntrian, Format tanggal periksa salah");
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }
            DateTime tglPeriksa = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null);
            if (tglPeriksa.Date < DateTime.Now.Date)
            {
                _logger.LogError("AmbilAntrian, Tanggal periksa telah lewat");
                throw new AppException("Tanggal periksa telah lewat");
            }
            if (!Utils.IsDateAllowed(request.TanggalPeriksa, _appSettings.MaximalJumlahHariPengambilanAntrian))
            {
                var jumlahHari = _appSettings.MaximalJumlahHariPengambilanAntrian.ToString();
                return new ApiNegativeResult("Tanggal periksa tidak valid, antrian hanya bisa diambil untuk hari ini atau maximal H+" + jumlahHari + " dari jadwal");
            }

            // Validasi Kode Dokter
            string kodeDokterJKN = request.KodeDokter.ToString();
            if (string.IsNullOrWhiteSpace(kodeDokterJKN))
            {
                _logger.LogError("AmbilAntrian, Kode dokter BPJS tidak valid");
                return new ApiNegativeResult("Kode dokter BPJS tidak valid");
            }
            string kodeDokterINT = await _jknService.GetKodeDokterINT(request.KodeDokter);
            if (kodeDokterINT == "ERROR_DOKTER_NOT_FOUND" || kodeDokterINT == "ERROR_DOKTER_NOT_FOUND_JKN")
            {
                _logger.LogError("AmbilAntrian, Dokter tidak ditemukan");
                return new ApiNegativeResult("Kode dokter tidak valid");
            }


            if (string.IsNullOrWhiteSpace(request.JamPraktek))
            {
                _logger.LogError("AmbilAntrian, Jam praktek belum diisi");
                return new ApiNegativeResult("Nomor Jam praktek belum diisi");
            }

            if (string.IsNullOrWhiteSpace(request.NoRm))
            {
                _logger.LogError("AmbilAntrian, Nomor rekam medis belum diisi");
                return new ApiNegativeResult("Nomor Rekam Medis belum diisi");
            }

            if (string.IsNullOrWhiteSpace(request.NoHp))
            {
                _logger.LogError("AmbilAntrian, Nomor HP belum diisi");
                return new ApiNegativeResult("Nomor HP belum diisi");
            }

            // tidak bisa mendaftar bila jam pelayanan poli telah selesai
            // ----------------------------------------------------------
            {
                string cekjamSelesai = request.JamPraktek.Substring(6, 5);
                DateTime dtCekJamAkhir = new DateTime();
                try
                {
                    DateTime dtPeriksa = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null).Date;
                    dtCekJamAkhir = DateTime.ParseExact(cekjamSelesai, "HH:mm", DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None);
                    if (dtPeriksa.Date == DateTime.Today.Date)
                    {
                        if (DateTime.Now > dtCekJamAkhir)
                        {
                            _logger.LogError("AmbilAntrian, Waktu layanan poli telah selesai");
                            return new ApiNegativeResult("Waktu layanan poli telah selesai");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex.Message);
                    _logger.LogError("  AmbilAntrian, Gagal mengkonversi data jam mulai/akhir");
                    return new ApiNegativeResult("Format jam layanan poli salah");
                }
            }

            // validasi jenis kunjungan & nomor referensi
            if (request.JenisKunjungan < 1 || request.JenisKunjungan > 4)
                return new ApiNegativeResult("Jenis kunjungan salah");
            if (string.IsNullOrWhiteSpace(request.NomorReferensi))
                return new ApiNegativeResult("Nomor referensi belum diisi");
            if (request.NomorReferensi.Length != 19)
                return new ApiNegativeResult("Nomor referensi tidak valid");

            // Bila perlu validasi nomor peserta BPJS langsung ke sistem VClaim BPJS
            if (_appSettings.BpjsVclaimValidateNomorPeserta)
            {
                var peserta = await _jknService.GetPesertaJknDariVClaim(request.NomorKartu);
                if (peserta.StatusPeserta.Keterangan != "AKTIF")
                    return new ApiNegativeResult("StatusAntri peserta BPJS tidak AKTIF");
            }

            // Try fix nomor rekam medis dari Request bila diperlukan
            request.NoRm = Utils.FixNomorRekamMedis(request.NoRm, _configuration, _logger);
            string realRekmed = "";
            bool pasienExisting = await _jknService.CheckPasienExisting(request.Nik, request.NomorKartu, request.NoRm);
            if (pasienExisting)
            {
                realRekmed = request.NoRm;
            }
            else
            {
                // Cari pasien berdasarkan NIK dan Nomor Kartu BPJS, bila ditemukan gunakan 
                // nomor rekam medis yang ada di database

                realRekmed = await _jknService.GetNomorRekamMedis(request.Nik, request.NomorKartu);
                if (realRekmed == "ERROR_NOMOR_REKAMMEDIS_NOT_FOUND")
                {
                    _logger.LogError($"  AmbilAntrian, Data pasien tidak ditemukan, kirim result 202");
                    return new ApiNegativeResult("Data pasien ini tidak ditemukan, silahkan melakukan registrasi pasien baru", 202);
                }
                else
                {
                    if (request.NoRm != realRekmed)
                    {
                        _logger.LogWarning($"  AmbilAntrian, Nomor rekmed pada request: {request.NoRm} berbeda dengan rekmed pada bl_pasien: {realRekmed}");
                        _logger.LogWarning($"  Proses selanjutnya akan menggunakan data rekmed {realRekmed} dari database");

                        request.NoRm = realRekmed;
                    }
                }
            }

            if (_appSettings.BpjsVclaimValidateRujukanCheckUsage)
            {
                /*
                if (await _jknService.RujukanSudahTerpakai(request.NomorReferensi))
                {
                    if (request.JenisKunjungan == JenisRujukan.FKTP)           // 1
                        return new ApiNegativeResult("Rujukan FKTP sudah digunakan");
                    else if(request.JenisKunjungan == JenisRujukan.INTERNAL)    // 2
                        return new ApiNegativeResult("Rujukan Internal sudah digunakan");
                    else if (request.JenisKunjungan == JenisRujukan.KONTROL)   // 3
                        return new ApiNegativeResult("Surat Kontrol sudah digunakan");
                    else if (request.JenisKunjungan == JenisRujukan.ANTAR_RS)   // 4
                        return new ApiNegativeResult("Rujukan Antar RS sudah digunakan");
                }
                */
            }

            if (_appSettings.BpjsVclaimValidateRujukan)
            {
                if (request.JenisKunjungan == JenisRujukan.FKTP && _appSettings.BpjsVclaimValidateRujukanCheckUsage)
                {
                    if (await _jknService.RujukanSudahExpired(request.NomorReferensi, request.TanggalPeriksa))
                        return new ApiNegativeResult("Masa berlaku rujukan telah habis");
                }
                else if (request.JenisKunjungan == JenisRujukan.INTERNAL) {
                    // Rujukan Internal (2)
                }
                else if (request.JenisKunjungan == JenisRujukan.KONTROL) {
                    // Surat Kontrol (3)
                }
                else if (request.JenisKunjungan == JenisRujukan.ANTAR_RS) {
                    // Rujukan Antar RS (4)
                }
            }

           
            // --------------------------------------------------------------------------
            // ambil id jadwal yang tersedia
            // note: jamMulai & jamSelesai must in format: HH:mm (5 character) - 24 hours format
            // bpjs use jamPraktek eg.: "14:00-16:00"
            string jamMulai   = request.JamPraktek.Substring(0, 5);
            string jamSelesai = request.JamPraktek.Substring(6, 5);

            if (_appSettings.AutoImportJadwalPraktekDokterDariDataHfis)
            {
                // Periksa dulu jadwal dokter di database MJKN HFIS, dan bila ada simpan ke database SIMRS
                // ambil jadwal berdasarkan kode poli bpjs (bukan kode subspesialis bpjs)
                var kodePoliBPJS   = await _jknService.GetKodePoliJKN(request.KodePoli);
                var listJadwalHfis = await _jknService.GetJadwalDokterHFIS(kodePoliBPJS, request.TanggalPeriksa);

                var poliTutup = await _jknService.CheckJadwalPoliTutup(request.TanggalPeriksa, request.KodeDokter, request.KodePoli, request.JamPraktek);
                if (poliTutup)
                {
                    _logger.LogError($"  AmbilAntrian No.BPJS: {request.NomorKartu}: Pendaftaran ke poli: {request.KodePoli} sedang tutup");
                    return new ApiNegativeResult("Pendaftaran ke poli ini sedang tutup");
                }

                // Create jadwal praktek SIMRS dari data HFIS (menggunakan kode sub spesialis) - request.KodePoli adalah kode sub spesialis BPJS
                await _jknService.CreateJadwalPraktekDariHFIS(request.TanggalPeriksa, request.KodePoli, request.KodeDokter, jamMulai, jamSelesai);
            }

            // --------------------------------------------------------------------------
            // Ambil Jadwal dokter internal
            var jadwalSvc = await _jknService.GetJadwalDokter(kodePoliINT, request.TanggalPeriksa, kodeDokterINT, jamMulai, jamSelesai);
            if (jadwalSvc == null)
            {
                _logger.LogError($"  AmbilAntrian {request.NomorKartu}: Jadwal {request.JamPraktek} Poli {request.KodePoli} belum tersedia");
                return new ApiNegativeResult("Jadwal belum tersedia, silahkan reschedule tanggal dan jam praktek lainnya");
            }

            var dataAntrian = new DataAmbilAntrianFKRTL(request)
            {
                KodePoliINT   = kodePoliINT,
                KodeDokterINT = kodeDokterINT,
                NamaDokterJKN  = await _jknService.GetNamaDokterJKN(request.KodeDokter),
                NamaPoliJKN    = await _jknService.GetNamaSubPoliJKN(request.KodePoli),
            };


            // --------------------------------------------------------------------------
            // Ambil Antrian
            _logger.LogInformation($"  Mengambil Antrian untuk Pasien No.BPJS: {request.NomorKartu} No RM: {realRekmed}, SubPoli: {request.KodePoli}, Dokter: {dataAntrian.NamaDokterJKN}");
            var resAntrian = await _jknService.GetAppointment(dataAntrian, jadwalSvc.Id);
            var nomorAntrian = request.KodePoli + "-" + Convert.ToString(resAntrian.AngkaAntrian);

            // Save request data
            if (resAntrian.Message == "SUCCESS" || resAntrian.Message == "SUCCESS_EXISTS") {
                await _jknService.SaveAmbilAntrian(request, resAntrian);
            }

            // Reservasi baru berhasil didaftarkan
            if (resAntrian.Message == "SUCCESS")
            {
                ResponseAmbilAntrianFKRTL response = new ResponseAmbilAntrianFKRTL
                {
                    NomorAntrean     = nomorAntrian,
                    AngkaAntrean     = resAntrian.AngkaAntrian,
                    KodeBooking      = resAntrian.RegistrationToken,
                    NoRm             = request.NoRm,
                    NamaPoli         = dataAntrian.NamaPoliJKN,
                    NamaDokter       = dataAntrian.NamaDokterJKN,
                    EstimasiDilayani = resAntrian.EstimatedTimeUnix,
                    SisaKuotaJkn     = resAntrian.AntrianSummary.QuotaJkn - resAntrian.AntrianSummary.QuotaJknUsed,
                    KuotaJkn         = resAntrian.AntrianSummary.QuotaJkn,
                    SisaKuotaNonJkn  = resAntrian.AntrianSummary.QuotaNonJkn - resAntrian.AntrianSummary.QuotaNonJknUsed,
                    KuotaNonJkn      = resAntrian.AntrianSummary.QuotaNonJkn,
                    Keterangan       = "Peserta harap datang 60 menit lebih awal guna pencatatan administrasi."
                };

                _logger.LogInformation($"  Antrian untuk Pasien No.BPJS: {request.NomorKartu} berhasil didaftarkan, Kode Booking: {resAntrian.RegistrationToken}");

                if (_appSettings.AutoUpdateTambahAntrianKeWsBPJS)
                {
                    // Kirim data antrian ke WS BPJS
                    await _jknService.TambahAntrian(resAntrian.RegistrationToken);
                }

                return new ApiResult(response);
            }

            // Reservasi sudah ada
            if (resAntrian.Message == "SUCCESS_EXISTS")
            {
                string msg = "Reservasi pemeriksaan untuk tanggal "
                            + Utils.ISODateToIndonesianDate(request.TanggalPeriksa) + " telah didaftarkan pada: "
                            + Utils.ISODateTimeToIndonesianDate(resAntrian.IssuedAt, true) + " dengan Nomor Antrian: "
                            + nomorAntrian + ", Kode Booking: " + resAntrian.RegistrationToken
                            + ", Dokter: " + dataAntrian.NamaDokterJKN;

                // cek apakah sudah dibatalkan
                if (resAntrian.StatusAntri == StatusAntri.CANCELLED)
                {
                    msg += ", StatusAntri: Sudah dibatalkan";
                    _logger.LogInformation($"  Reservasi untuk Pasien No.BPJS: {request.NomorKartu} sudah ada, dengan status sudah dibatalkan");
                }
                else
                    _logger.LogInformation($"  Reservasi untuk Pasien No.BPJS: {request.NomorKartu} sudah ada");

                return new ApiNegativeResult(msg);
            }

            // Bila tidak bisa membuat reservasi baru atau reservasi belum ada yang didaftarkan
            // artinya quota layanan sudah penuh, atau terjadi kesalahan internal
            _logger.LogWarning($"  AmbilAntrian No.BPJS: {request.NomorKartu} Gagal membuat/mengambil reservasi untuk Dokter: {dataAntrian.NamaDokterJKN}");

            // ------------------------------------------------------------------------------
            // Fallback !
            if (resAntrian != null)
            {
                _logger.LogDebug($"  AmbilAntrian No.BPJS: {request.NomorKartu} Gagal: {resAntrian.Message}");
                var errMessage = Utils.TransErrCodeSlotReservasi(resAntrian.Message, _appSettings);
                return new ApiNegativeResult(errMessage);
            }
            else
            {
                var message = "Internal error code: 5570";
                _logger.LogDebug($"  AmbilAntrian: 5570, {request.NomorKartu}: Gagal: {message}");
                return new ApiNegativeResult(message);
            }
        }


        // Sisa Antrian
        // POST: /api/fkrtl/sisa_antrian
        [Authorize]
        [HttpPost("sisa_antrian")]
        public async Task<IActionResult> SisaAntrian([FromBody] RequestSisaAntrianFKRTL request)
        {
            _logger.LogInformation($"Menjalankan SisaAntrean, {request.KodeBooking}");

            if (string.IsNullOrWhiteSpace(request.KodeBooking))
                return new ApiNegativeResult("Kode booking tidak valid");

            var response = await _jknService.GetSisaAntrian(request);
            if (response != null)
            {
                _logger.LogInformation($"  SisaAntrean, {request.KodeBooking} sukses");
                return new ApiResult(response);
            }

            _logger.LogError($"  SisaAntrean, {request.KodeBooking}: data tidak tersedia");
            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Batal Antrian
        // POST: /api/fkrtl/batal_antrian
        [Authorize]
        [HttpPost("batal_antrian")]
        public async Task<IActionResult> BatalAntrian([FromBody] RequestBatalAntrianFKRTL request)
        {
            _logger.LogInformation($"Menjalankan BatalAntrian, {request.KodeBooking}, {request.Keterangan}");

            if (string.IsNullOrWhiteSpace(request.KodeBooking))
                return new ApiNegativeResult("Kode booking tidak valid");

            if (string.IsNullOrWhiteSpace(request.Keterangan))
                return new ApiNegativeResult("Keterangan tidak valid");

            var result = await _jknService.BatalAntrian(request);
            if (result == "SUKSES")
            {
                if (_appSettings.AutoUpdateBatalAntrianKeWsBPJS)
                {
                    // Kirim data pembatalan antrian ke WS BPJS
                    _jknService.BatalAntrian(request.KodeBooking, request.Keterangan);
                }

                _logger.LogInformation($"  BatalAntrian, {request.KodeBooking}, {request.Keterangan} sukses");
                // Pembatalan sukses
                return new ApiResult(200);
            }
            else
            {
                _logger.LogError($"  BatalAntrian, {request.KodeBooking}, {request.Keterangan} gagal");
                return new ApiNegativeResult(result);
            }
        }


        // Check in
        // POST: /api/fkrtl/checkin
        [Authorize]
        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] RequestCheckInFKRTL request)
        {
            _logger.LogInformation($"Menjalankan CheckIn, {request.KodeBooking}, {request.Waktu}");

            if (string.IsNullOrWhiteSpace(request.KodeBooking))
                return new ApiNegativeResult("Kode booking tidak valid");

            var result = await _jknService.CheckIn(request);
            if (result == "SUKSES" || result == "SUKSES_PASIEN_BARU")
            {
                if (_appSettings.AutoUpdateWaktuCheckinAntrianKeWsBPJS)
                {
                    bool pasienBaru = false;
                    if (result == "SUKSES_PASIEN_BARU")
                        pasienBaru = true;

                    // Kirim data update waktu checkin antrian ke WS BPJS
                    // https://dvlp.bpjs-kesehatan.go.id:8888/trust-mark/#UpdateWaktuAntrian
                    // Cek in / mulai waktu tunggu untuk pasien baru mulai pada task 1
                    // Cek in / mulai waktu tunggu untuk pasien lama mulai pada task 3

                    if (pasienBaru)
                    { 
                        /*await*/ _jknService.UpdateWaktuCheckinAntrian(request.KodeBooking, request.Waktu, 1);
                    }
                    else
                    {
                        // TODO_JEFRI: delay 10 ms after each update
                        /*await*/ _jknService.UpdateWaktuCheckinAntrian(request.KodeBooking, request.Waktu, 1);
                        /*await*/ _jknService.UpdateWaktuCheckinAntrian(request.KodeBooking, request.Waktu, 2);
                        /*await*/ _jknService.UpdateWaktuCheckinAntrian(request.KodeBooking, request.Waktu, 3);
                    }
                }

                _logger.LogInformation($"  CheckIn, {request.KodeBooking}, {request.Waktu} sukses");
                // CheckIn sukses
                return new ApiResult(200);
            }
            else 
            {
                _logger.LogError($"  CheckIn, {request.KodeBooking}, {request.Waktu} gagal, {result}");
                return new ApiNegativeResult(result);
            }
        }


        // Info Pasien baru
        // POST: /api/fkrtl/info_pasien_baru
        [Authorize]
        [HttpPost("info_pasien_baru")]
        public async Task<IActionResult> InfoPasienBaru([FromBody] RequestPasienBaruFKRTL request)
        {
            _logger.LogInformation("Menjalankan CreatePasienBaru");
            _logger.LogDebug(JsonSerializer.Serialize(request));

            if (string.IsNullOrWhiteSpace(request.NomorKartu))
                return new ApiNegativeResult("Nomor kartu belum diisi");
            if (request.NomorKartu.Length != 13 || !request.NomorKartu.All(char.IsNumber))
                return new ApiNegativeResult("Nomor kartu harus berupa angka, dan 13 digit");

            if (string.IsNullOrWhiteSpace(request.Nik))
                return new ApiNegativeResult("Nomor NIK belum diisi");
            if (request.Nik.Length != 16 || !request.Nik.All(char.IsNumber))
                return new ApiNegativeResult("Nomor NIK harus berupa angka, dan 16 digit");

            if (string.IsNullOrWhiteSpace(request.NomorKK))
                return new ApiNegativeResult("Nomor KK belum diisi");
            if (request.NomorKK.Length != 16 || !request.NomorKK.All(char.IsNumber))
                return new ApiNegativeResult("Nomor KK harus berupa angka, dan 16 digit");

            if (string.IsNullOrWhiteSpace(request.Nama))
                return new ApiNegativeResult("Nama pasien tidak valid");

            if (string.IsNullOrWhiteSpace(request.JenisKelamin))
                return new ApiNegativeResult("Jenis kelamin tidak valid");

            if (string.IsNullOrWhiteSpace(request.TanggalLahir))
                return new ApiNegativeResult("Tanggal lahir tidak valid");

            if (!Utils.IsValidDate(request.TanggalLahir))
                return new ApiNegativeResult("Format tanggal lahir salah, seharusnya yyyy-MM-dd");

            if (string.IsNullOrWhiteSpace(request.NoHp))
                return new ApiNegativeResult("Nomor handphone tidak valid");

            if (string.IsNullOrWhiteSpace(request.Alamat))
                return new ApiNegativeResult("Alamat tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodeProp))
                return new ApiNegativeResult("Kode propinsi tidak valid");

            if (string.IsNullOrWhiteSpace(request.NamaProp))
                return new ApiNegativeResult("Nama propinsi tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodeDati2))
                return new ApiNegativeResult("Kode Dati II tidak valid");

            if (string.IsNullOrWhiteSpace(request.NamaDati2))
                return new ApiNegativeResult("Nama Dati II tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodeKec))
                return new ApiNegativeResult("Kode kecamatan tidak valid");

            if (string.IsNullOrWhiteSpace(request.NamaKec))
                return new ApiNegativeResult("Nama kecamatan tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodeKel))
                return new ApiNegativeResult("Kode kelurahan tidak valid");

            if (string.IsNullOrWhiteSpace(request.NamaKel))
                return new ApiNegativeResult("Nama kelurahan tidak valid");

            if (string.IsNullOrWhiteSpace(request.Rw))
                return new ApiNegativeResult("Nomor RW tidak valid");

            if (string.IsNullOrWhiteSpace(request.Rt))
                return new ApiNegativeResult("Nomor RT tidak valid");

            var response = await _jknService.CreatePasienBaru(request);
            if (response != null) 
            {
                _logger.LogInformation($"  CreatePasienBaru {request.NomorKartu} {request.Nik} sukses");
                return new ApiResult(response, 200, "Harap datang ke admisi untuk melengkapi data rekam medis");
            }

            _logger.LogError($"  CreatePasienBaru {request.NomorKartu} data tidak tersedia");
            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Jadwal Operasi RS
        // POST: /api/fkrtl/jadwal_operasi_rs
        [Authorize]
        [HttpPost("jadwal_operasi_rs")]
        public async Task<IActionResult> JadwalOperasiRS([FromBody] RequestJadwalOperasiFKRTL request)
        {
            _logger.LogInformation($"Menjalankan JadwalOperasiRS, {request.TanggalAwal}, {request.TanggalAkhir}");

            if (!Utils.IsValidDate(request.TanggalAwal))
                return new ApiNegativeResult("Format tanggal salah, seharusnya yyyy-MM-dd");

            if (!Utils.IsValidDate(request.TanggalAkhir))
                return new ApiNegativeResult("Format tanggal salah, seharusnya yyyy-MM-dd");

            DateTime tglAwal  = DateTime.ParseExact(request.TanggalAwal,  "yyyy-MM-dd", null).Date;
            DateTime tglAkhir = DateTime.ParseExact(request.TanggalAkhir, "yyyy-MM-dd", null).Date;

            if (tglAwal > tglAkhir)
                return new ApiResult("Tanggal awal dan akhir tidak valid");

            List<ResponseJadwalOperasiFKRTL> response = await _jknService.JadwalOperasiRS(request);
            if (response != null) 
            {
                _logger.LogInformation($"  JadwalOperasiRS, {request.TanggalAwal}, {request.TanggalAkhir}: sukses, total row: {response.Count}");
                return new ApiResult(new { List = response });
            }

            _logger.LogError($"  JadwalOperasiRS, {request.TanggalAwal}, {request.TanggalAkhir}: data tidak tersedia");
            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Jadwal Operasi Pasien
        // POST: /api/fkrtl/jadwal_operasi_pasien
        [Authorize]
        [HttpPost("jadwal_operasi_pasien")]
        public async Task<IActionResult> JadwalOperasiPasien([FromBody] RequestJadwalOperasiPasien request)
        {
            _logger.LogInformation($"Menjalankan JadwalOperasiPasien, {request.NoPeserta}");

            if (request.NoPeserta.Length != 13 || !request.NoPeserta.All(char.IsNumber))
                return new ApiNegativeResult("Nomor Kartu harus berupa angka, dan 13 digit");

            if ( ! await _jknService.CheckPasienExistingByJKN(request.NoPeserta))
                return new ApiNegativeResult("Gagal, pasien belum terdaftar di Rumah Sakit");

            List<ResponseJadwalOperasiPasien> response = await _jknService.JadwalOperasiPasien(request);
            if (response != null) 
            {
                _logger.LogInformation($"  JadwalOperasiPasien, {request.NoPeserta} sukses, total row: {response.Count}");
                return new ApiResult(new { List = response });
            }

            _logger.LogError($"  JadwalOperasiPasien, {request.NoPeserta}: data tidak tersedia");
            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Get Antrian Farmasi
        // POST: /api/fkrtl/ambil_antrian_farmasi
        [Authorize]
        [HttpPost("ambil_antrian_farmasi")]
        public async Task<IActionResult> AmbilAntrianFarmasi([FromBody] RequestAmbilAntrianFarmasi request)
        {
            _logger.LogInformation($"Menjalankan AmbilAntrianFarmasi, {request.KodeBooking}");

            if (string.IsNullOrWhiteSpace(request.KodeBooking))
                return new ApiNegativeResult("Kode booking tidak valid");
            
            bool kodeBookingOK = await _jknService.CheckKodeBooking(request.KodeBooking);
            if (!kodeBookingOK)
                throw new AppException("Kode booking tidak ditemukan");

            return new ApiNegativeResult("Belum diimplementasikan");
        }


        // Get Antrian Farmasi
        // POST: /api/fkrtl/status_antrian_farmasi
        [Authorize]
        [HttpPost("status_antrian_farmasi")]
        public async Task<IActionResult> StatusAntrianFarmasi([FromBody] RequestStatusAntrianFarmasi request)
        {
            _logger.LogInformation($"Menjalankan StatusAntrianFarmasi {request.KodeBooking}");

            if (string.IsNullOrWhiteSpace(request.KodeBooking))
                return new ApiNegativeResult("Kode booking tidak valid");

            bool kodeBookingOK = await _jknService.CheckKodeBooking(request.KodeBooking);
            if (!kodeBookingOK)
                throw new AppException("Kode booking tidak ditemukan");

            return new ApiNegativeResult("Belum diimplementasikan");
        }
    }
}