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

using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Tobasa.App;
using Microsoft.Extensions.Options;
using Tobasa.Services;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using Tobasa.Models.Vclaim;
using Microsoft.Extensions.Logging;
using Tobasa.Models.SimrsAntrian;
using System.Text.Json;
using Tobasa.Models.Mjkn;
using Tobasa.Services.Vclaim;

namespace Tobasa.Controllers
{
    // /api/simrs
    [ApiController]
    [Route("api/[controller]")]
    public class SimrsController : ControllerBase
    {
        private IUserService            _userService;
        private IMjknService            _jknService;
        private IVClaimApiService       _vclaimApiService;
        private readonly AppSettings    _appSettings;
        private readonly IConfiguration _configuration;
        private readonly ILogger        _logger;

        public SimrsController(
            IUserService            userService,
            IMjknService            jknService,
            IVClaimApiService       vclaimApiService,
            IOptions<AppSettings>   appSettings,
            IConfiguration          configuration,
            ILogger<MjknService>    logger)
        {
            _userService      = userService;
            _jknService       = jknService;
            _vclaimApiService = vclaimApiService;
            _appSettings      = appSettings.Value;
            _configuration    = configuration;
            _logger           = logger;
        }

        // ----------------------------------------------------------------------------------------------------//
        //                    INTERNAL SERVICES ACCESSED FROM LOCAL SIMRS APP #VCLAIM                            // 
        // ----------------------------------------------------------------------------------------------------//

        // Get Peserta by NIK
        // GET: /api/simrs/vclaim/get_peserta_by_nik/{nik}/{tglSep}
        [Authorize]
        [HttpGet("vclaim/get_peserta_by_nik/{nik}/{tglSep}")]
        public async Task<IActionResult> GetPesertaByNik(string nik, string tglSep)
        {
            _logger.LogInformation("Menjalankan prosedur GetPesertaByNik");
            var result = await _vclaimApiService.GetPesertaByNik(nik, tglSep);

            if (result != null) {
                return new ApiResultCamelCase(result);
            }

            return new ApiNegativeResultCamelCase("Peserta tidak ada", 201, 200);
        }


        // Get Peserta by nomor kartu BPJS kesehatan
        // GET: /api/simrs/vclaim/get_peserta_by_nocard/{nomorBpjs}/{tglSep}
        [Authorize]
        [HttpGet("vclaim/get_peserta_by_nocard/{nomorBpjs}/{tglSep}")]
        public async Task<IActionResult> GetPesertaByNocard(string nomorBpjs, string tglSep)
        {
            _logger.LogInformation("Menjalankan prosedur GetPesertaByNocard");
            var result = await _vclaimApiService.GetPesertaByNocard(nomorBpjs, tglSep);

            if (result != null) {
                return new ApiResultCamelCase(result);
            }

            return new ApiNegativeResultCamelCase("Peserta tidak ada", 201, 200);
        }


        // Get data Rujukan by nomor rujukan
        // GET: /api/simrs/vclaim/get_rujukan_by_norujuk/{nomorRujukan}
        [Authorize]
        [HttpGet("vclaim/get_rujukan_by_norujuk/{nomorRujukan}")]
        public async Task<IActionResult> GetRujukanByNoRujukan(string nomorRujukan)
        {
            _logger.LogInformation("Menjalankan prosedur GetRujukanByNoRujukan");
            var result = await _vclaimApiService.GetRujukanByNoRujukan(nomorRujukan);

            if (result != null) {
                return new ApiResultCamelCase(result);
            }

            return new ApiNegativeResultCamelCase("Data Rujukan Tidak Ditemukan", 201, 200);
        }


        // Get data Rujukan by nomor kartu BPJS kesehatan
        // GET: /api/simrs/vclaim/get_rujukan_by_nocard_single/{nomorBpjs}
        [Authorize]
        [HttpGet("vclaim/get_rujukan_by_nocard_single/{nomorBpjs}")]
        public async Task<IActionResult> GetRujukanByNocardSingle(string nomorBpjs)
        {
            _logger.LogInformation("Menjalankan prosedur GetRujukanByNocardSingle");
            var result = await _vclaimApiService.GetRujukanByNocardSingle(nomorBpjs);

            if (result != null) {
                return new ApiResultCamelCase(result);
            }

            return new ApiNegativeResultCamelCase("Rujukan tidak ada", 201, 200);
        }


        // Get data Rujukan by nomor kartu BPJS kesehatan (multiple result)
        // GET: /api/simrs/vclaim/get_rujukan_by_nocard_multi/{nomorBpjs}
        [Authorize]
        [HttpGet("vclaim/get_rujukan_by_nocard_multi/{nomorBpjs}")]
        public async Task<IActionResult> GetRujukanByNocardMulti(string nomorBpjs)
        {
            _logger.LogInformation("Menjalankan prosedur GetRujukanByNocardMulti");
            var result = await _vclaimApiService.GetRujukanByNocardMulti(nomorBpjs);

            if (result != null) {
                return new ApiResultCamelCase(result);
            }

            return new ApiNegativeResultCamelCase("Rujukan tidak ada", 201, 200);
        }


        // Get data Surat Kontrol
        // GET: /api/simrs/vclaim/get_surat_kontrol/{noSuratKontrol}
        [Authorize]
        [HttpGet("vclaim/get_surat_kontrol/{noSuratKontrol}")]
        public async Task<IActionResult> GetSuratKontrol(string noSuratKontrol)
        {
            _logger.LogInformation("Menjalankan prosedur GetSuratKontrol");
            var result = await _vclaimApiService.GetSuratKontrol(noSuratKontrol);

            if (result != null)
            {
                var apiResult = new ApiResultCamelCase(result);
                return apiResult;
            }

            return new ApiNegativeResultCamelCase("Surat kontrol tidak ditemukan", 201, 200);
        }


        // Get Data Histori Pelayanan Peserta
        // GET: /api/simrs/vclaim/get_history_pelayanan/{nomorBPJS}/{tanggalAwal}/{tanggalAkhir}
        [Authorize]
        [HttpGet("vclaim/get_history_pelayanan/{nomorBPJS}/{tanggalAwal}/{tanggalAkhir}")]
        public async Task<IActionResult> GetHistoriPelayananPeserta(string nomorBPJS, string tanggalAwal, string tanggalAkhir)
        {
            _logger.LogInformation("Menjalankan prosedur GetHistoriPelayananPeserta");

            if (!Utils.IsValidDate(tanggalAwal))
                return new ApiNegativeResult("Format tanggal awal salah, seharusnya yyyy-MM-dd");

            if (!Utils.IsValidDate(tanggalAkhir))
                return new ApiNegativeResult("Format tanggal akhir salah, seharusnya yyyy-MM-dd");

            var result = await _vclaimApiService.GetHistoriPelayananPeserta(nomorBPJS, tanggalAwal, tanggalAkhir);

            if (result != null)
            {
                var apiResult = new ApiResultCamelCase(result);
                return apiResult;
            }

            return new ApiNegativeResultCamelCase("Data history pelayanan tidak ditemukan", 201, 200);
        }
        
        
        // ----------------------------------------------------------------------------------------------------//
        //                            INTERNAL SERVICES ACCESSED FROM LOCAL SIMRS APP                            // 
        // ----------------------------------------------------------------------------------------------------//

        // StatusAntri Antrian
        // GET: /api/simrs/antrian/status_antrian/kodepoli/{kodepoliRs}/kodedokter/{kodedokterRs}/tanggalperiksa/{tanggalperiksa}/jampraktek/{jampraktek}
        [Authorize]
        [HttpGet("antrian/status_antrian/kodepoli/{kodepoliRs}/kodedokter/{kodedokterRs}/tanggalperiksa/{tanggalperiksa}/jampraktek/{jampraktek}")]
        public async Task<IActionResult> StatusAntrian(string kodepoliRs, string kodedokterRs, string tanggalperiksa, string jampraktek)
        {
            _logger.LogInformation($"Menjalankan prosedur StatusAntrian SIMRS, {kodepoliRs}, {kodedokterRs}, {tanggalperiksa}, {jampraktek}");

            if (string.IsNullOrWhiteSpace(kodepoliRs)     || string.IsNullOrWhiteSpace(kodedokterRs) || 
                string.IsNullOrWhiteSpace(tanggalperiksa) || string.IsNullOrWhiteSpace(jampraktek))
            {
                return new ApiNegativeResult("Parameter request tidak valid");
            }

            if ( ! await _jknService.CekKodePoliINT(kodepoliRs))
                return new ApiNegativeResult("Kode poli tidak ditemukan");

            if ( ! await _jknService.CekKodeDokterINT(kodedokterRs))
                return new ApiNegativeResult("Kode Dokter tidak ditemukan");

            if (tanggalperiksa.Length != 10)
                return new ApiNegativeResult("Tanggal periksa tidak valid");

            var response = await _jknService.GetStatusAntrianSimrs(kodepoliRs, kodedokterRs, tanggalperiksa, jampraktek);
            if (response != null) {
                return new ApiResult(response);
            }

            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Tambah Antrian - Menambahkan antrian/reservasi yang telah dibuat ke server BPJS
        // POST: /api/simrs/antrian/bpjs_tambah_antrian/{bookingKode}
        [Authorize]
        [HttpPost("antrian/bpjs_tambah_antrian/{bookingKode}")]
        public async Task<IActionResult> BpjsTambahAntrian(string bookingKode)
        {
            _logger.LogInformation("Menjalankan prosedur BpjsTambahAntrian SIMRS");

            try
            {
                // Kirim data antrian ke WS BPJS
                await _jknService.TambahAntrian(bookingKode);
                return new ApiResult("Antrian ditambahkan");
            }
            catch(BpjsException ex)
            {
                return new ApiNegativeResult(ex.Message);
            }
        }


        // Get Antrian
        // POST: /api/simrs/antrian/ambil_antrian
        [Authorize]
        [HttpPost("antrian/ambil_antrian")]
        public async Task<IActionResult> AmbilAntrian([FromBody] RequestAmbilAntrianSimrs request)
        {
             // NOTE: request KodePoliInternal Kode Poli Rumah Sakit 
             // yang harus dimapping dengan kode sup spesialis (sub poli) BPJS/BPJS 
             // ------------------------------------------------------------------

            _logger.LogInformation("Menjalankan prosedur AmbilAntrian SIMRS");
            _logger.LogDebug(JsonSerializer.Serialize(request));

            if (string.IsNullOrWhiteSpace(request.JenisPasien))
                return new ApiNegativeResult("Jenis pasien tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodePoli))
                return new ApiNegativeResult("Kode poli tidak valid");

            if (string.IsNullOrWhiteSpace(request.NoRm))
                return new ApiNegativeResult("Nomor rekam medis tidak valid");

            if (!Utils.IsValidDate(request.TanggalPeriksa))
                return new ApiNegativeResult("Format tanggal periksa salah, seharusnya yyyy-MM-dd");

            if (!Utils.IsDateAllowed(request.TanggalPeriksa, _appSettings.MaximalJumlahHariPengambilanAntrian))
            {
                var jumlahHari = _appSettings.MaximalJumlahHariPengambilanAntrian.ToString();
                return new ApiNegativeResult("Tanggal periksa tidak valid, antrian hanya bisa diambil untuk hari ini atau maximal H+" + jumlahHari + " dari jadwal");
            }

            if (string.IsNullOrWhiteSpace(request.KodeDokter))
                return new ApiNegativeResult("Kode dokter tidak valid");

            if (string.IsNullOrWhiteSpace(request.JamPraktek))
                return new ApiNegativeResult("Jam praktek tidak valid");

            if (request.JamPraktek.Length != 11)
                return new ApiNegativeResult("Jam praktek tidak valid");

            if (! await _jknService.CekKodePoliINT(request.KodePoli) )
                return new ApiNegativeResult("Poli tidak ada di Rumah Sakit");

            if ( ! await _jknService.CekKodeDokterINT(request.KodeDokter))
                return new ApiNegativeResult("Kode dokter tidak valid");

            // try fix nomor rekam medis
            request.NoRm = Utils.FixNomorRekamMedis(request.NoRm, _configuration, _logger);


            // --------------------------------------------------------------------------
            // Check data local pasien

            var dataAntrian = new DataAmbilAntrianFKRTL(request);

            if (await _jknService.CheckPasienExistingByMRN(request.NoRm)) 
            {
                var kodePoliJKN    = await _jknService.GetKodeSubPoliJKN(request.KodePoli);
                var kodeDokterJKN  = await _jknService.GetKodeDokterJKN(request.KodeDokter);

                dataAntrian.AppSource     = "SIMRS_WEBSVC";
                dataAntrian.KodePoliJKN   = kodePoliJKN;
                dataAntrian.KodeDokterJKN = kodeDokterJKN;

                if (request.JenisPasien == Insurance.BPJS)
                {
                    var pasien = await _jknService.GetPasien(request.NoRm);
                    if (pasien.NomorIdentitas == null)
                        return new ApiNegativeResult("Data NIK pasien tidak valid");

                    dataAntrian.JenisPasien   = Insurance.BPJS;
                    dataAntrian.Nik           = pasien.NomorIdentitas.Trim();
                    dataAntrian.NomorKartu    = pasien.NomorKartuJkn.Trim();
                    dataAntrian.NomorHP       = pasien.Phone.Trim();
                }
                else
                {
                    // Pasien Non BPJS
                    dataAntrian.JenisPasien    = "-";
                    dataAntrian.JenisKunjungan = 1;
                    dataAntrian.NomorReferensi = "";
                }
            }
            else {
                return new ApiNegativeResult("Nomor rekam medis tidak ditemukan");
            }


            if (request.JenisPasien == Insurance.BPJS)
            {
                var wSSimrsAnreanTidakValidasiNomorPeserta = _configuration["AppSettings:WSSimrsAnreanTidakValidasiNomorPeserta"];
                if (wSSimrsAnreanTidakValidasiNomorPeserta != true.ToString())
                {
                    if (string.IsNullOrWhiteSpace(dataAntrian.NomorKartu))
                        return new ApiNegativeResult("Data nomor peserta BPJS tidak valid");

                    var peserta = await _jknService.GetPesertaJknDariVClaim(dataAntrian.NomorKartu);
                    if (peserta.StatusPeserta.Keterangan != "AKTIF")
                        return new ApiNegativeResult("StatusAntri peserta BPJS tidak AKTIF");
                }

                var wSSimrsAnreanTidakValidasiRujukan = _configuration["AppSettings:WSSimrsAnreanTidakValidasiRujukan"];
                if (wSSimrsAnreanTidakValidasiRujukan != true.ToString())
                {
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    // validasi jenis kunjungan

                    if (string.IsNullOrWhiteSpace(request.NomorReferensi))
                        return new ApiNegativeResult("Data nomor referensi tidak valid");

                    string jenisKunjunganTervalidasi = "";

                    // Cek dulu apakah ini Surat Rujukan
                    var vClaimRujukanService = new VclaimRujukanService(_appSettings, _logger, _configuration);
                    var rujukanJKN = await vClaimRujukanService.GetRujukanByNoRujukan(request.NomorReferensi);
                
                    if (rujukanJKN != null)
                    {
                        jenisKunjunganTervalidasi = "RUJUKAN_FKTP";
                        var tglPeriksa   = DateTime.ParseExact(request.TanggalPeriksa, "yyyy-MM-dd", null);
                        var tglKunjungan = DateTime.ParseExact(rujukanJKN.TglKunjungan, "yyyy-MM-dd", null);

                        if (tglPeriksa.Date >= tglKunjungan.AddDays(90).Date)
                            return new ApiNegativeResult("Masa berlaku rujukan telah habis");
                    }
                    else
                    {
                        // Cek apakah ini Surat Kontrol
                        var vClaimRKontrol = new VclaimRencanaKontrolService(_appSettings, _logger, _configuration);
                        SuratKontrol skontrol = await vClaimRKontrol.GetSuratKontrol(request.NomorReferensi);
                        if (skontrol != null)
                            jenisKunjunganTervalidasi = "RUJUKAN_KONTROL";
                        else
                            return new ApiNegativeResult("Nomor Surat Kontrol tidak terdaftar di Vclaim");

                        if (jenisKunjunganTervalidasi != "RUJUKAN_KONTROL")
                            return new ApiNegativeResult("Nomor Surat Rujukan tidak terdaftar di Vclaim");
                    }


                    if (jenisKunjunganTervalidasi == "RUJUKAN_FKTP") {
                        request.JenisKunjungan = 1;
                    }
                    else if (jenisKunjunganTervalidasi == "RUJUKAN_INTERNAL") {
                        request.JenisKunjungan = 2;
                    }
                    else if (jenisKunjunganTervalidasi == "RUJUKAN_KONTROL") {
                        request.JenisKunjungan = 3;
                    }
                    else if (jenisKunjunganTervalidasi == "RUJUKAN_ANTAR_RS") {
                        request.JenisKunjungan = 4;
                    }

                    _logger.LogDebug($"  Jenis rujukan tervalidasi adalah {jenisKunjunganTervalidasi}");
                }

                var sSSimrsAnreanTidakValidasiSep = _configuration["AppSettings:WSSimrsAnreanTidakValidasiSep"];
                if (sSSimrsAnreanTidakValidasiSep != true.ToString())
                {
                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                    // Validasi SEP belum dibuat
                    // Cek apakah ini Surat Kontrol

                    if (string.IsNullOrWhiteSpace(request.NomorReferensi))
                        return new ApiNegativeResult("Data nomor referensi tidak valid");

                    var pasien = await _jknService.GetPasien(request.NoRm);

                    _logger.LogDebug($"  Melakukan pemeriksaan SEP");
                    var vClaimMonitor  = new VclaimMonitoringService(_appSettings, _logger, _configuration);
                    var historyLayanan = await vClaimMonitor.GetDataHistoriPelayananPeserta(pasien.NomorKartuJkn.Trim(), DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.ToString("yyyy-MM-dd"));
                    if (historyLayanan != null)
                    {
                        foreach (var history in historyLayanan)
                        {
                            if (history.NoRujukan == request.NomorReferensi)
                            {
                                if (!String.IsNullOrWhiteSpace(history.NoSep))
                                    return new ApiNegativeResult($"Nomor SEP {history.NoSep} sudah diterbitkan untuk kunjungan ini");
                            }
                        }
                    }
                    else
                        _logger.LogDebug($"  History layanan tidak ditemukan: {pasien.NomorKartuJkn} {DateTime.Now.ToString("yyyy-MM-dd")} - {DateTime.Now.ToString("yyyy-MM-dd")}" );
                }
            }


            // --------------------------------------------------------------------------
            // ambil id jadwal yang tersedia
            // note: jamMulai & jamSelesai must in format: HH:mm (5 character) - 24 hours format
            // bpjs use jamPraktek eg.: "14:00-16:00"
            string jamMulai   = request.JamPraktek.Substring(0, 5);
            string jamSelesai = request.JamPraktek.Substring(6, 5);

            List<string> listKodePoliNonSinkron = new List<string>();
            if (_appSettings.ListKodePoliNonSinkron != null)
                listKodePoliNonSinkron = _appSettings.ListKodePoliNonSinkron;

            List<string> listKodeDokterNonSinkron = new List<string>();
            if (_appSettings.ListKodeDokterNonSinkron != null)
                listKodeDokterNonSinkron = _appSettings.ListKodeDokterNonSinkron;

            if (_appSettings.AutoImportJadwalPraktekDokterDariDataHfis)
            {
                bool skipPoli   = false;
                bool skipDokter = false;

                if (listKodePoliNonSinkron.Count > 0 )
                {
                    foreach (string kodepoli in listKodePoliNonSinkron)
                    {
                        if (request.KodePoli == kodepoli)
                        {
                            _logger.LogDebug($"  AmbilAntrian, Poli {request.KodePoli} ada dalam daftar non sinkron");
                            skipPoli = true;
                            break;
                        }
                    }
                }

                if (listKodeDokterNonSinkron.Count > 0)
                {
                    foreach (string kodeDokter in listKodeDokterNonSinkron)
                    {
                        if (request.KodeDokter == kodeDokter)
                        {
                            _logger.LogDebug($"  AmbilAntrian, Dokter {request.KodeDokter} ada dalam daftar non sinkron");
                            skipDokter = true;
                            break;
                        }
                    }
                }


                if ( ! (skipPoli || skipDokter) )
                {
                    var kodeDokterJKN  = await _jknService.GetKodeDokterJKN(request.KodeDokter);
                    var kodeSubPoliJKN = await _jknService.GetKodeSubPoliJKN(request.KodePoli);

                    // Periksa dulu jadwal dokter di databasa MJKN HFIS, dan bila ada simpan ke database SIMRS
                    // ambil jadwal berdasarkan kode poli bpjs (bukan kode subspesialis bpjs)
                    var kodePoliJKN    = await _jknService.GetKodePoliJKN(kodeSubPoliJKN);
                    var listJadwalHFIS = await _jknService.GetJadwalDokterHFIS(kodePoliJKN, request.TanggalPeriksa);

                    var poliTutup = await _jknService.CheckJadwalPoliTutup(request.TanggalPeriksa, kodeDokterJKN, kodeSubPoliJKN, request.JamPraktek);
                    if (poliTutup)
                        return new ApiNegativeResult("Pendaftaran ke poli ini sedang tutup");

                    // Create jadwal praktek SIMRS dari data HFIS (menggunakan kode sub spesialis) - kodeSubPoliJKN adalah kode sub spesialis BPJS
                    await _jknService.CreateJadwalPraktekDariHFIS(request.TanggalPeriksa, kodeSubPoliJKN, kodeDokterJKN, jamMulai, jamSelesai);
                }
            }

            // --------------------------------------------------------------------------
            // Ambil Jadwal dokter internal
            var jadwalSvc = await _jknService.GetJadwalDokter(request.KodePoli, request.TanggalPeriksa, request.KodeDokter, jamMulai, jamSelesai);
            if (jadwalSvc == null)
            { 
                _logger.LogError($"  AmbilAntrian No.RM: {request.NoRm} Jadwal: {request.JamPraktek} Poli: {request.KodePoli} belum tersedia");
                return new ApiNegativeResult("Jadwal belum tersedia, silahkan reschedule tanggal dan jam praktek lainnya");
            }

            // --------------------------------------------------------------------------
            // Ambil Antrian
            _logger.LogInformation($"  Mengambil Antrian untuk Pasien {request.NoRm} pada Poli {request.KodePoli} dengan Dokter {jadwalSvc.NamaDokter.Trim()}");
            //string namaPos     = jadwalSvc.NamaPoli.Trim();
            string namaDokterINT = jadwalSvc.NamaDokter.Trim();
            string namaPoliINT   = (await _jknService.GetNamaPoliINT(request.KodePoli)).Trim();

            var resAntrian   = await _jknService.GetAppointment(dataAntrian, jadwalSvc.Id);
            var nomorAntrian = request.KodePoli + "-" + Convert.ToString(resAntrian.AngkaAntrian);

            // Reservasi baru berhasil didaftarkan
            if (resAntrian.Message == "SUCCESS")
            {
                ResponseAmbilAntrianFKRTL response = new ResponseAmbilAntrianFKRTL
                {
                    NomorAntrean     = nomorAntrian,
                    AngkaAntrean     = resAntrian.AngkaAntrian,
                    KodeBooking      = resAntrian.RegistrationToken,
                    NoRm             = request.NoRm,
                    NamaPoli         = namaPoliINT,
                    NamaDokter       = namaDokterINT,
                    EstimasiDilayani = resAntrian.EstimatedTimeUnix,
                    SisaKuotaJkn     = resAntrian.AntrianSummary.QuotaJkn - resAntrian.AntrianSummary.QuotaJknUsed,
                    KuotaJkn         = resAntrian.AntrianSummary.QuotaJkn,
                    SisaKuotaNonJkn  = resAntrian.AntrianSummary.QuotaNonJkn - resAntrian.AntrianSummary.QuotaNonJknUsed,
                    KuotaNonJkn      = resAntrian.AntrianSummary.QuotaNonJkn,
                    Keterangan       = "Peserta harap datang 60 menit lebih awal guna pencatatan administrasi."
                };

                _logger.LogInformation($"  Antrian untuk Pasien No.RM: {request.NoRm} berhasil didaftarkan, Kode booking: {resAntrian.RegistrationToken}");

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
                            + ", Dokter: " + namaDokterINT;

                // cek apakah sudah dibatalkan
                if (resAntrian.StatusAntri == StatusAntri.CANCELLED)
                {
                    msg += ", StatusAntri: Sudah dibatalkan";
                    _logger.LogInformation($"  Reservasi untuk Pasien No.RM: {request.NoRm} sudah ada, dengan status sudah dibatalkan");
                }
                else
                    _logger.LogInformation($"  Reservasi untuk Pasien No.RM: {request.NoRm} sudah ada");


                return new ApiNegativeResult(msg);
            }

            // Bila tidak bisa membuat reservasi baru atau reservasi belum ada yang didaftarkan
            // artinya quota layanan sudah penuh, atau terjadi kesalahan internal
            _logger.LogError($"  AmbilAntrian No.RM: {request.NoRm} Gagal membuat/mengambil reservasi untuk dokter {namaDokterINT}");

            // ------------------------------------------------------------------------------
            // Fallback !
            if (resAntrian != null)
            {
                _logger.LogError($"  AmbilAntrian No.RM: {request.NoRm} Gagal: {resAntrian.Message}");
                var errMessage = Utils.TransErrCodeSlotReservasi(resAntrian.Message, _appSettings);
                return new ApiNegativeResult(errMessage);
            }
            else
            {
                var message = "Internal error code: 5571";
                _logger.LogError($"  AmbilAntrian: 5571, No.RM: {request.NoRm} Gagal: {message}");
                return new ApiNegativeResult(message);
            }
        }


        // Create Jadwal Praktek
        // POST: /api/simrs/antrian/create_jadwal_praktek
        [Authorize]
        [HttpPost("antrian/create_jadwal_praktek")]
        public async Task<IActionResult> CreateJadwalPraktek([FromBody] RequestCreateJadwalPraktek request)
        {
            _logger.LogInformation($"Menjalankan prosedur CreateJadwalPraktek");

            if (!Utils.IsValidDate(request.TanggalAwal))
                return new ApiNegativeResult("Format tanggal awal salah, seharusnya yyyy-MM-dd");

            if (request.JumlahHari <= 0)
                return new ApiNegativeResult("Jumlah hari tidak valid");

            if (string.IsNullOrWhiteSpace(request.KodeDokter))
                return new ApiNegativeResult("Kode dokter rumah sakit tidak valid");

            if (string.IsNullOrWhiteSpace(request.JamMulai) || request.JamMulai.Length != 5)
                _logger.LogWarning("Jam mulai tidak valid, seharusnya mm:ss");

            if (string.IsNullOrWhiteSpace(request.JamAkhir) || request.JamMulai.Length != 5)
                _logger.LogWarning("Jam akhir tidak valid, seharusnya mm:ss");

            var created = await _jknService.CreateJadwalPraktek(request);
            if (created > 0) {
                return new ApiResult($"Jadwal di proses: { created }");
            }

            return new ApiNegativeResult("Gagal membuat jadwal", 201, 200);
        }


        // Create Slot Antrian
        // POST: /api/simrs/antrian/create_slot_antrian
        [Authorize]
        [HttpPost("antrian/create_slot_antrian")]
        public async Task<IActionResult> CreateSlotAntrian([FromBody] RequestCreateSlotAntrian request)
        {
            _logger.LogInformation($"Menjalankan prosedur CreateSlotAntrian SIMRS, JADWAL Code: {request.KodeJadwal}");

            if (request.KodeJadwal <= 0)
                return new ApiNegativeResult("Id Jadwal tidak valid");

            if (string.IsNullOrWhiteSpace(request.AppSource))
                request.AppSource = "SIMRS_WEBSVC";

            var result = await _jknService.CekSlotReservasi(request.KodeJadwal, request.AppSource);
            if (result != null)
            {
                if (result.Message.StartsWith("ERROR"))
                {
                    _logger.LogError($"  CreateSlotAntrian error, {result.Message}");
                    string errmessage = Utils.TransErrCodeSlotReservasi(result.Message, _appSettings);
                    return new ApiNegativeResult($"Error: {errmessage}", 201, 200);
                }
                else
                    return new ApiResult(result);
            }

            return new ApiNegativeResult("Gagal membuat slot reservasi", 201, 200);
        }


        // Referensi Jadwal dokter - HFIS
        // GET: /api/simrs/antrian/jadwal_dokter_hfis/{kodePoliJKN}/{tanggal}
        [Authorize]
        [HttpGet("antrian/jadwal_dokter_hfis/{kodePoliJKN}/{tanggal}")]
        public async Task<IActionResult> JadwalDokterHfis(string kodePoliJkn, string tanggal)
        {
            _logger.LogInformation($"Menjalankan prosedur JadwalDokterHfis, {kodePoliJkn}, {tanggal}");

            if ( string.IsNullOrWhiteSpace(kodePoliJkn) || string.IsNullOrWhiteSpace(tanggal))
                return new ApiNegativeResult("Parameter kode poli jkn atau tanggal tidak valid");

            List<MjknJadwalDokterHfis> response = await _jknService.GetJadwalDokterHFIS(kodePoliJkn, tanggal);
            if (response != null) {
                return new ApiResult(response);
            }

            return new ApiNegativeResult("Data tidak tersedia", 201, 200);
        }


        // Sinkron Jadwal dokter - HFIS
        // GET: /api/simrs/antrian/sinkron_jadwal_dokter_hfis/kodepolirs/{kodePoliRs}/tglperiksa/{tanggal}
        [Authorize]
        [HttpGet("antrian/sinkron_jadwal_dokter_hfis/kodepolirs/{kodePoliRs}/tglperiksa/{tanggal}")]
        public async Task<IActionResult> SinkronJadwalPraktekDokterDariDataHfis(string kodePoliRs, string tanggal)
        {
            _logger.LogInformation($"Menjalankan prosedur SinkronJadwalPraktekDokterDariDataHFIS, {kodePoliRs}, {tanggal}");

            if (string.IsNullOrWhiteSpace(kodePoliRs) || string.IsNullOrWhiteSpace(tanggal))
                return new ApiNegativeResult("Parameter kode poli rumah sakit atau tanggal tidak valid");

            if (!Utils.IsValidDate(tanggal))
                throw new AppException("Format tanggal periksa salah, seharusnya yyyy-MM-dd");

            DateTime dtPeriksa = DateTime.ParseExact(tanggal, "yyyy-MM-dd", null).Date;
            if (dtPeriksa.Date < DateTime.Today.Date )
                return new ApiNegativeResult("Hanya dapat mensinkron jadwal untuk hari ini dan seterusnya");

            var result = await _jknService.SinkronJadwalPraktekDokterDariDataHFIS(kodePoliRs, tanggal);
            return new ApiResult(result);
        }

    }
}