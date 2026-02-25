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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Tobasa.App;
using Tobasa.Models;
using Tobasa.Models.Mjkn;
using Tobasa.Services;

namespace Tobasa.Controllers
{
    // /api/fktp
    [Route("api/[controller]")]
    [ApiController]
    public class FktpController : ControllerBase
    {
        private readonly IUserService     _userService;
        private readonly IMjknServiceFKTP _jknServiceFKTP;
        private readonly AppSettings      _appSettings;
        private readonly IConfiguration   _configuration;
        private readonly ILogger          _logger;

        public FktpController(
            IUserService          userService,
            IMjknServiceFKTP      jknServiceFKTP,
            IOptions<AppSettings> appSettings,
            IConfiguration        configuration,
            ILogger<MjknService>  logger)
        {
            _userService    = userService;
            _jknServiceFKTP = jknServiceFKTP;
            _appSettings    = appSettings.Value;
            _configuration  = configuration;
            _logger         = logger;
        }


        // Get JWT Token
        // GET: /api/fktp/token
        [AllowAnonymous]
        [HttpGet("token")]
        public IActionResult GetJwtToken()
        {
            _logger.LogTrace("Menjalankan GetJwtToken");

            var userName = Request.Headers["x-username"].ToString();
            var password = Request.Headers["x-password"].ToString();

            // AppException and other Exceptions will be handled by ErrorHandlerMiddleWare

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return new ApiNegativeResult("User atau password tidak valid");

            var user = _userService.Authenticate(userName, password);
            if (user == null)
                return new ApiNegativeResult("User atau password tidak sesuai");

            var tokenString = _userService.GenerateJwt(user.Id, user.UserName);
            return new ApiResult(new { Token = tokenString });
        }


        // Status Antrian
        // GET: /api/fktp/status_antrian/{kodePoli}/{tanggalPeriksa}
        [Authorize]
        [HttpGet("status_antrian/{kodePoli}/{tanggalPeriksa}")]
        public async Task<IActionResult> StatusAntrian(string kodePoli, string tanggalPeriksa)
        {
            _logger.LogTrace($"Menjalankan StatusAntrian, Kode poli {kodePoli}, tanggal {tanggalPeriksa}");

            var vKodePoli       = kodePoli == null ? "" : kodePoli.Trim();
            var vTanggalPeriksa = tanggalPeriksa == null ? "" : tanggalPeriksa.Trim();

            // NOTE: kodePoli adalah Kode SubPoli BPJS, bukan kode Poli Internal
            if (string.IsNullOrWhiteSpace(vKodePoli))
            {
                _logger.LogError("StatusAntrian, Kode poli belum diisi");
                return new ApiNegativeResult("Kode poli belum diisi");
            }

            // Konversi Kode SubPoli BPJS menjadi kodepoli FKTP
            string kodePoliInternal = await _jknServiceFKTP.GetKodePoliINT(vKodePoli);
            if (kodePoliInternal == "ERROR_POLI_NOT_FOUND")
            {
                _logger.LogError("StatusAntrian, Kode poli tidak tersedia");
                return new ApiNegativeResult("Poli tidak ada di FKTP");
            }

            if (string.IsNullOrWhiteSpace(vTanggalPeriksa))
            {
                _logger.LogError("StatusAntrian, Tanggal periksa belum diisi");
                return new ApiNegativeResult("Tanggal periksa belum diisi");
            }
            if (!Utils.IsValidDate(vTanggalPeriksa))
            {
                _logger.LogError("StatusAntrian, Format tanggal periksa salah");
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }
            DateTime tglPeriksa = DateTime.ParseExact(vTanggalPeriksa, "yyyy-MM-dd", null);
            if (tglPeriksa.Date < DateTime.Now.Date)
            {
                _logger.LogError("StatusAntrian, Tanggal periksa telah lewat");
                throw new AppException("Tanggal periksa telah lewat");
            }

            var request = new RequestStatusAntrianFKTP
            {
                KodePoliInternal = kodePoliInternal,
                KodePoliJKN      = vKodePoli,
                TanggalPeriksa   = vTanggalPeriksa
            };
            var result = await _jknServiceFKTP.GetStatusAntrian(request);
            return new ApiResult(result.StatusList, 200);
        }


        // Get Antrian
        // POST: /api/fktp/ambil_antrian
        [Authorize]
        [HttpPost("ambil_antrian")]
        public async Task<IActionResult> AmbilAntrian([FromBody] RequestAmbilAntrianFKTP request)
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
            string kodePoliINT = await _jknServiceFKTP.GetKodePoliINT(request.KodePoli);
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
            string kodeDokterINT = await _jknServiceFKTP.GetKodeDokterINT(request.KodeDokter);
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

            if (string.IsNullOrWhiteSpace(request.Keluhan))
            {
                _logger.LogError("AmbilAntrian, Keluhan belum diisi");
                return new ApiNegativeResult("Keluhan belum diisi");
            }

            // Try fix nomor rekam medis dari Request bila diperlukan
            request.NoRm = Utils.FixNomorRekamMedis(request.NoRm, _configuration, _logger);
            string realRekmed = "";
            bool pasienExisting = await _jknServiceFKTP.CheckPasienExisting(request.Nik, request.NomorKartu, request.NoRm);
            if (pasienExisting) {
                realRekmed = request.NoRm;
            }
            else
            {
                // Cari pasien berdasarkan NIK dan Nomor Kartu BPJS, bila ditemukan gunakan 
                // nomor rekam medis yang ada di database

                realRekmed = await _jknServiceFKTP.GetNomorRekamMedis(request.Nik, request.NomorKartu);
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
                var kodePoliBPJS   = await _jknServiceFKTP.GetKodePoliJKN(request.KodePoli);
                var listJadwalHfis = await _jknServiceFKTP.GetJadwalDokterHFIS(kodePoliBPJS, request.TanggalPeriksa);

                var poliTutup = await _jknServiceFKTP.CheckJadwalPoliTutup(request.TanggalPeriksa, request.KodeDokter, request.KodePoli, request.JamPraktek);
                if (poliTutup)
                {
                    _logger.LogError($"  AmbilAntrian No.BPJS: {request.NomorKartu}: Pendaftaran ke poli: {request.KodePoli} sedang tutup");
                    return new ApiNegativeResult("Pendaftaran ke poli ini sedang tutup");
                }

                // Create jadwal praktek SIMRS dari data HFIS (menggunakan kode sub spesialis) - request.KodePoli adalah kode sub spesialis BPJS
                await _jknServiceFKTP.CreateJadwalPraktekDariHFIS(request.TanggalPeriksa, request.KodePoli, request.KodeDokter, jamMulai, jamSelesai);
            }


            // --------------------------------------------------------------------------
            // Ambil Jadwal dokter internal
            var jadwalSvc = await _jknServiceFKTP.GetJadwalDokter(kodePoliINT, request.TanggalPeriksa, kodeDokterINT, jamMulai, jamSelesai);
            if (jadwalSvc == null)
            {
                _logger.LogError($"  AmbilAntrian {request.NomorKartu}: Jadwal {request.JamPraktek} Poli {kodePoliINT} belum tersedia");
                return new ApiNegativeResult("Jadwal belum tersedia, silahkan reschedule tanggal dan jam praktek lainnya");
            }

            var dataAntrian = new DataAmbilAntrianFKTP(request)
            {
                KodePoliINT   = kodePoliINT,
                KodeDokterINT = kodeDokterINT,
                NamaDokterJKN = await _jknServiceFKTP.GetNamaDokterJKN(request.KodeDokter),
                NamaPoliJKN   = await _jknServiceFKTP.GetNamaSubPoliJKN(request.KodePoli),
            };

            // --------------------------------------------------------------------------
            // Ambil Antrian
            _logger.LogInformation($"  Mengambil Antrian untuk Pasien No.BPJS: {request.NomorKartu} No RM: {realRekmed}, SubPoli: {request.KodePoli}, Dokter: {dataAntrian.NamaDokterJKN}");
            var resAntrian = await _jknServiceFKTP.GetAppointment(dataAntrian, jadwalSvc.Id);
            var nomorAntrian = kodePoliINT + "-" + Convert.ToString(resAntrian.AngkaAntrian);
            
            // Save request data
            if (resAntrian.Message == "SUCCESS" || resAntrian.Message == "SUCCESS_EXISTS")
            {
                //await _jknServiceFKTP.SaveAmbilAntrian(request, resAntrian);
            }
            
            // Reservasi baru berhasil didaftarkan
            if (resAntrian.Message == "SUCCESS")
            {
                ResponseAmbilAntrianFKTP response = new ResponseAmbilAntrianFKTP
                {
                    NomorAntrean   = nomorAntrian,
                    AngkaAntrean   = resAntrian.AngkaAntrian,
                    NamaPoli       = dataAntrian.NamaPoliJKN,
                    SisaAntrian    = resAntrian.AntrianSummary.SisaAntrianJkn,
                    AntreanPanggil = resAntrian.AntrianSummary.LastServedAntrian, 
                    Keterangan     = ""
                };

                _logger.LogInformation($"  Antrian untuk Pasien No.BPJS: {request.NomorKartu} berhasil didaftarkan, Token: {resAntrian.RegistrationToken}");

                if (_appSettings.AutoUpdateTambahAntrianKeWsBPJS)
                {
                    // Kirim data antrian ke WS BPJS
                    //await _jknServiceFKTP.TambahAntrian(resAntrian.RegistrationToken);
                }

                return new ApiResult(response);
            }

            // Reservasi sudah ada
            if (resAntrian.Message == "SUCCESS_EXISTS")
            {
                string msg = "Antrian pemeriksaan untuk tanggal "
                            + Utils.ISODateToIndonesianDate(request.TanggalPeriksa) + " telah didaftarkan pada: "
                            + Utils.ISODateTimeToIndonesianDate(resAntrian.IssuedAt, true) + " dengan Nomor Antrian: "
                            + nomorAntrian + ", Dokter: " + dataAntrian.NamaDokterJKN;

                // cek apakah sudah dibatalkan
                if (resAntrian.StatusAntri == StatusAntri.CANCELLED )
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
        // GET: /api/fktp/sisa_antrian/{nomorkartu_jkn}/{kode_poli}/{tanggalperiksa}
        [Authorize]
        [HttpGet("sisa_antrian/{nomorKartuJkn}/{kodePoli}/{tanggalPeriksa}")]
        public async Task<IActionResult> SisaAntrian(string nomorKartuJkn, string kodePoli, string tanggalPeriksa)
        {
            _logger.LogInformation($"Menjalankan SisaAntrean, {nomorKartuJkn}, {kodePoli}, {tanggalPeriksa}");
            var vNomorKartuJkn  = nomorKartuJkn  == null ? "" : nomorKartuJkn.Trim();
            var vKodePoli       = kodePoli       == null ? "" : kodePoli.Trim();
            var vTanggalPeriksa = tanggalPeriksa == null ? "" : tanggalPeriksa.Trim();


            if (string.IsNullOrWhiteSpace(vNomorKartuJkn))
            {
                _logger.LogError("SisaAntrian, Nomor Kartu BPJS belum diisi");
                return new ApiNegativeResult("Nomor Kartu BPJS belum diisi");
            }
            if (vNomorKartuJkn.Length != 13 || !vNomorKartuJkn.All(char.IsNumber))
            {
                _logger.LogError("SisaAntrian, Format Nomor Kartu BPJS tidak valid");
                return new ApiNegativeResult("Nomor kartu BPJS harus berupa angka, dan 13 digit");
            }

            // NOTE: kodePoli adalah Kode Subspesialis/Sub Poli BPJS, bukan kode Poli Internal
            if (string.IsNullOrWhiteSpace(vKodePoli))
            {
                _logger.LogError("SisaAntrian, Kode poli belum diisi");
                return new ApiNegativeResult("Kode poli belum diisi");
            }
            // konversi kode sub spesialis bpjs menjadi kode poli internal
            string kodePoliInternal = await _jknServiceFKTP.GetKodePoliINT(vKodePoli);
            if (kodePoliInternal == "ERROR_POLI_NOT_FOUND")
            {
                _logger.LogError("SisaAntrian, Kode poli tidak tersedia");
                return new ApiNegativeResult("Poli tidak ada di FKTP");
            }

            if (string.IsNullOrWhiteSpace(vTanggalPeriksa))
            {
                _logger.LogError("SisaAntrian, Tanggal periksa belum diisi");
                return new ApiNegativeResult("Tanggal periksa belum diisi");
            }
            if (!Utils.IsValidDate(vTanggalPeriksa))
            {
                _logger.LogError("SisaAntrian, Format tanggal periksa salah");
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }
            //DateTime tglPeriksa = DateTime.ParseExact(vTanggalPeriksa, "yyyy-MM-dd", null);
            //if (tglPeriksa.Date != DateTime.Now.Date)
            //{
            //    _logger.LogError("AmbilAntrian, Tanggal periksa tidak valid. Hanya bisa melihat sisa antrian di hari H pelayanan");
            //    throw new AppException("Hanya bisa melihat infromasi antrian di hari H pelayanan");
            //}

            if (!await _jknServiceFKTP.CheckPasienExistingByJKN(vNomorKartuJkn))
            {
                _logger.LogError("SisaAntrian, Pasien tidak terdaftar");
                throw new AppException("Pasien tidak terdaftar");
            }

            var request = new RequestSisaAntrianFKTP
            {
                NomorKartuJKN    = vNomorKartuJkn,
                KodePoliJKN      = vKodePoli,
                KodePoliInternal = kodePoliInternal,
                TanggalPeriksa   = vTanggalPeriksa
            };
            var response = await _jknServiceFKTP.GetSisaAntrian(request); 
            
            if (response != null) 
            {
                _logger.LogInformation($"  SisaAntrian, {vNomorKartuJkn}, {kodePoli}, {vTanggalPeriksa} sukses");
                return new ApiResult(response, 200);
            }

            return new ApiNegativeResult("Data tidak tersedia");
        }


        // Batal Antrian
        // PUT: /api/fktp/batal_antrian
        [Authorize]
        [HttpPut("batal_antrian")]
        public async Task<IActionResult> BatalAntrian([FromBody] RequestBatalAntrianFKTP request)
        {
            _logger.LogInformation($"Menjalankan BatalAntrian, nocard: {request.NomorKartu}, poli: {request.KodePoli}, {request.Keterangan}");
            _logger.LogDebug(JsonSerializer.Serialize(request));

            if (string.IsNullOrWhiteSpace(request.NomorKartu))
            {
                _logger.LogError("BatalAntrian, Nomor Kartu BPJS belum diisi");
                return new ApiNegativeResult("Nomor Kartu BPJS belum diisi");
            }
            if (request.NomorKartu.Length != 13 || !request.NomorKartu.All(char.IsNumber))
            {
                _logger.LogError("BatalAntrian, Forma Nomor Kartu BPJS tidak valid");
                return new ApiNegativeResult("Nomor kartu BPJS harus berupa angka, dan 13 digit");
            }
            if (!await _jknServiceFKTP.CheckPasienExistingByJKN(request.NomorKartu))
            {
                _logger.LogError("BatalAntrian, Nomor Kartu BPJS tidak terdaftar");
                throw new AppException("Nomor Kartu BPJS tidak terdaftar");
            }


            // NOTE: kodePoli adalah Kode Subspesialis/Sub Poli BPJS, bukan kode Poli Internal
            if (string.IsNullOrWhiteSpace(request.KodePoli))
            {
                _logger.LogError("BatalAntrian, Kode poli belum diisi");
                return new ApiNegativeResult("Kode poli belum diisi");
            }
            // konversi kode sub spesialis bpjs menjadi kode poli internal
            string kodePoliInternal = await _jknServiceFKTP.GetKodePoliINT(request.KodePoli);
            if (kodePoliInternal == "ERROR_POLI_NOT_FOUND")
            {
                _logger.LogError("BatalAntrian, Kode poli tidak tersedia");
                return new ApiNegativeResult("Poli tidak ada di FKTP");
            }

            if (string.IsNullOrWhiteSpace(request.TanggalPeriksa))
            {
                _logger.LogError("BatalAntrian, Tanggal periksa belum diisi");
                return new ApiNegativeResult("Tanggal periksa belum diisi");
            }
            if (!Utils.IsValidDate(request.TanggalPeriksa))
            {
                _logger.LogError("BatalAntrian, Format tanggal periksa salah");
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");
            }
            DateTime tglPeriksa = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null);
            if (tglPeriksa.Date < DateTime.Now.Date)
            {
                _logger.LogError("BatalAntrian, Tanggal periksa telah lewat");
                throw new AppException("Tanggal periksa telah lewat");
            }


            if (string.IsNullOrWhiteSpace(request.Keterangan))
            {
                _logger.LogError("BatalAntrian, Keterangan belum diisi");
                return new ApiNegativeResult("Keterangan belum diisi");
            }


            RequestBatalAntrianFKTP vRequest = request;
            vRequest.KodePoliInternal = kodePoliInternal;
            var result = await _jknServiceFKTP.BatalAntrian(vRequest);
            if (result == "SUCCESS")
            {
                if (_appSettings.AutoUpdateBatalAntrianKeWsBPJS)
                {
                    // Kirim data pembatalan antrian ke WS BPJS
                    //_jknServiceFKTP.BatalAntrian(request);
                }

                _logger.LogInformation($"  BatalAntrian, nocard: {vRequest.NomorKartu}, poli: {vRequest.KodePoli} Sukses ");
                return new ApiResult(200);
            }
            else
            {
                _logger.LogError($"  BatalAntrian, nocard: {vRequest.NomorKartu}, poli: {vRequest.KodePoli} Gagal");
                return new ApiNegativeResult("Gagal membatalkan Antrian", 201, 200);
            }
        }


        // Pasien baru
        // POST: /api/fktp/pasien_baru
        [Authorize]
        [HttpPost("pasien_baru")]
        public async Task<IActionResult> PasienBaru([FromBody] RequestPasienBaruFKTP request)
        {
            _logger.LogInformation("Menjalankan CreatePasienBaru");
            _logger.LogDebug(JsonSerializer.Serialize(request));

            if (string.IsNullOrWhiteSpace(request.NomorKartu)) 
            {
                _logger.LogError("CreatePasienBaru, Nomor Kartu BPJS belum diisi");
                return new ApiNegativeResult("Nomor kartu belum diisi");
            }
            if (request.NomorKartu.Length != 13 || !request.NomorKartu.All(char.IsNumber)) 
            {
                _logger.LogError("CreatePasienBaru, Format Nomor Kartu BPJS tidak valid");
                return new ApiNegativeResult("Nomor kartu harus berupa angka, dan 13 digit");
            }

            if (string.IsNullOrWhiteSpace(request.Nik)) 
            {
                _logger.LogError("CreatePasienBaru, Nomor NIK belum diisi");
                return new ApiNegativeResult("Nomor NIK belum diisi");
            }
            if (request.Nik.Length != 16 || !request.Nik.All(char.IsNumber)) 
            {
                _logger.LogError("CreatePasienBaru, Format NIK tidak valid");
                return new ApiNegativeResult("Nomor NIK harus berupa angka, dan 16 digit");
            }

            if (string.IsNullOrWhiteSpace(request.NomorKK)) 
            {
                _logger.LogError("CreatePasienBaru, Nomor KK belum diisi");
                return new ApiNegativeResult("Nomor KK belum diisi");
            }
            if (request.NomorKK.Length != 16 || !request.NomorKK.All(char.IsNumber)) 
            {
                _logger.LogError("CreatePasienBaru, Format Nomor KK tidak valid");
                return new ApiNegativeResult("Nomor KK harus berupa angka, dan 16 digit");
            }

            if (string.IsNullOrWhiteSpace(request.Nama)) 
            {
                _logger.LogError("CreatePasienBaru, Nama belum diisi");
                return new ApiNegativeResult("Nama belum diisi");
            }

            if (string.IsNullOrWhiteSpace(request.JenisKelamin)) 
            {
                _logger.LogError("CreatePasienBaru, Jenis kelamin belum diisi");
                return new ApiNegativeResult("Jenis kelamin belum diisi");
            }

            if (string.IsNullOrWhiteSpace(request.TanggalLahir)) 
            {
                _logger.LogError("CreatePasienBaru, Tanggal lahir tidak valid");
                return new ApiNegativeResult("Tanggal lahir tidak valid");
            }

            if (!Utils.IsValidDate(request.TanggalLahir))
            {
                _logger.LogError("CreatePasienBaru, Format tanggal lahir salah");
                return new ApiNegativeResult("Format tanggal lahir salah, seharusnya yyyy-MM-dd");
            }

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

            bool success = await _jknServiceFKTP.CreatePasienBaru(request);
            if (success) 
            {
                _logger.LogInformation($"  CreatePasienBaru {request.NomorKartu} {request.Nik} sukses");
                return new ApiResult(200);
            }

            _logger.LogError($"  CreatePasienBaru {request.NomorKartu} Gagal membuat pasien baru");
            return new ApiNegativeResult("Gagal membuat pasien baru", 201, 200);
        }
    }
}